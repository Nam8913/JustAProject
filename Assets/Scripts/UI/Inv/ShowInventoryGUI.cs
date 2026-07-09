#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShowInventoryGUI : MonoBehaviour
{
    public static ShowInventoryGUI Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance = FindAnyObjectByType<ShowInventoryGUI>();
                    if (_instance == null)
                    {
                        Debug.LogError("No instance of ShowInventoryGUI found in the scene.");
                    }
                }
            }
            return _instance;
        }
    }
    private static ShowInventoryGUI _instance;
    private static readonly object _lock = new object();

    [SerializeField]
    private DefineThing targetToShow;

    [SerializeField]
    [TextArea(3,5)]
    private string debugStr;

    [SerializeField]
    public InventoryViewEntities targetView;
    
    [SerializeField]
    public InventoryViewAroundEntities aroundView;

    #region Around View 
    private List<Tile> lastNearbyContainer = new List<Tile>();
    #endregion

    private Vector2Int lastTargetTilePos = new Vector2Int(int.MinValue, int.MinValue);

    [SerializeField]
    public float aroundSearchRadius = 2f;

    void Awake()
    { 
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
    }

    void LateUpdate()
    {
        if (targetToShow == null)
        {
            return;
        }

        WorldHandler worldHandler = GameService.GetWorldHandler();
        if (worldHandler == null || !worldHandler.IsInitialized)
        {
            return;
        }

        Vector2Int targetTilePos = GetTargetTilePos();
        if (targetTilePos == lastTargetTilePos)
        {
            return;
        }

        RefreshAll();
    }

    private void OnDrawGizmos()
    {
        if(Application.isEditor && !Application.isPlaying)
        {
            return;
        }
        foreach(var tile in lastNearbyContainer)
        {
            Gizmos.color = Color.green;
            Vector3 pos = new Vector3(tile.tilePosition.x+0.5f, tile.tilePosition.y+0.5f, 0);
            Gizmos.DrawSphere(pos, 0.2f);
        }

        if(targetToShow != null)
        {
            Gizmos.color = Color.red;
            Vector3 pos = new Vector3(targetToShow.transform.position.x, targetToShow.transform.position.y, 0);
            Gizmos.DrawSphere(pos, 0.2f);
        }
    }

    void OnEnable()
    {
        RefreshAll();
    }

    void OnDisable()
    {
        UnsubscribeFromNearbyContainers();
        lastNearbyContainer.Clear();
        lastTargetTilePos = new Vector2Int(int.MinValue, int.MinValue);
    }

    public void SetTargetToShow(DefineThing target)
    {
        #if DEBUG_LOG_FLAG && false
        Debug.Log($"ShowInventoryGUI SetTargetToShow: {target}");
        #endif
        targetToShow = target;
        
        RefreshAll();
    }

    public DefineThing GetTargetToShow()
    {
        return targetToShow;
    }

    public void RefreshAll()
    {
        if (targetToShow == null)
        {
            if (aroundView != null && lastNearbyContainer.Count > 0)
            {
                aroundView.MakeContainerListUI(new List<Tile>(), lastNearbyContainer);
            }

            UnsubscribeFromNearbyContainers();
            lastNearbyContainer.Clear();
            lastTargetTilePos = new Vector2Int(int.MinValue, int.MinValue);
            debugStr = string.Empty;
            return;
        }

        WorldHandler worldHandler = GameService.GetWorldHandler();
        if (worldHandler == null || !worldHandler.IsInitialized)
        {
            return;
        }

        lastTargetTilePos = GetTargetTilePos();
        RefreshAround();
        UpdateDebugStr();
    }

    private void UpdateDebugStr()
    {
        debugStr = string.Empty;
        foreach(var tile in lastNearbyContainer)
        {
            debugStr += $"Tile {tile.tilePosition} has container.\n";
        }
    }

    private void UnsubscribeFromNearbyContainers()
    {
        foreach (var tile in lastNearbyContainer)
        {
            if (tile != null && tile.TryGetExistingContainer(out Container container))
            {
                container.Changed -= CallbackOnAddedItemToEmptyContainer;
            }
        }
    }

    public void RefreshAround()
    {
        List<Tile> getNearbyContainer = GetAllTilesAroundTarget(aroundSearchRadius);

        List<Tile> getDiffrence_add = GetDifferenceBetweenTileLists(getNearbyContainer, lastNearbyContainer);
        List<Tile> getAddContainersHasItems = new List<Tile>();
        
        if(getDiffrence_add.Count > 0)
        {
            // These are new slots with empty containers, so we should add events to them to receive notifications when an item is added to their container.
           foreach(var tile in getDiffrence_add)
           {
                if(tile.HasItems)
                {
                    getAddContainersHasItems.Add(tile);
                }else
                {
                    if(tile.TryGetExistingContainer(out Container c))
                    {
                        c.Changed += CallbackOnAddedItemToEmptyContainer;
                    }
                }
           }
        }
        List<Tile> getDiffrence_remove = GetDifferenceBetweenTileLists(lastNearbyContainer, getNearbyContainer);
        List<Tile> getRemoveContainersHasItems = new List<Tile>();
        if(getDiffrence_remove.Count > 0)
        {
            // These are slots that previously had empty containers but now either have items or are no longer in range, so we should remove the events we added to them.
            foreach(var tile in getDiffrence_remove)
            {
                if(tile.HasItems)
                {
                    getRemoveContainersHasItems.Add(tile);
                }else
                {
                    if(tile.TryGetExistingContainer(out Container c))
                    {
                        c.Changed -= CallbackOnAddedItemToEmptyContainer;
                    }
                }
            }
        }
        lastNearbyContainer = getNearbyContainer;
        if (aroundView != null)
        {
            aroundView.MakeContainerListUI(getAddContainersHasItems, getRemoveContainersHasItems);
        }
    }

    public void CallbackOnAddedItemToEmptyContainer()
    {
        using(DisposableStopwatch s = new DisposableStopwatch("CallbackOnAddedItemToEmptyContainer"))
        {
            if (aroundView == null)
            {
                return;
            }

            List<Tile> tilesHasItems = lastNearbyContainer.Where(t => t.HasItems).ToList();
            Dictionary<GameObject, Tile> tempDic = new Dictionary<GameObject, Tile>();

            tempDic = tilesHasItems.ToDictionary(t => t.GetGameObject(), t => t);

            foreach(var item in aroundView.GetContentScrollViewOfContainerList().GetComponentsInChildren<Transform>())
            {
                ContainerButton button = item.GetComponent<ContainerButton>();
                if(button != null)
                {
                    GameObject containerGO = button.GetOwner();
                    Tile tile = tempDic.TryGetValue(containerGO, out Tile t) ? t : null;
                    if(tile != null && containerGO != null && tile.GetGameObject() == containerGO)
                    {
                        tilesHasItems.Remove(tile);
                    }
                }
            }
            tilesHasItems.ForEach(t =>
            {
                    if(t.TryGetExistingContainer(out Container c))
                {
                    c.Changed -= CallbackOnAddedItemToEmptyContainer;
                }
            });
            aroundView.MakeContainerListUI(tilesHasItems, new List<Tile>());
        }
        
    }

    private List<Tile> GetDifferenceBetweenTileLists(List<Tile> listA, List<Tile> listB)
    {
        HashSet<Tile> setB = new HashSet<Tile>(listB);
        return listA.Where(tile => !setB.Contains(tile)).ToList();
    }

    private Vector2Int GetTargetTilePos()
    {
        if (targetToShow == null)
        {
            return new Vector2Int(int.MinValue, int.MinValue);
        }
        return new Vector2Int(Mathf.RoundToInt(targetToShow.transform.position.x-0.5f), Mathf.RoundToInt(targetToShow.transform.position.y-0.5f));
    }

    public List<Tile> GetAllTilesAroundTarget(float radius)
    {
        Vector2Int pos = GetTargetTilePos();
        return Tile.GetAllTilesAroundPos(pos, radius);
    }
}
