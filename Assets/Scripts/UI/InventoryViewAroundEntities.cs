using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryViewAroundEntities : MonoBehaviour
{
    public static InventoryViewAroundEntities Ins
    {
        get
        {
            return ShowInventoryGUI.Instance.aroundView;
        }
    }
    [Header("References_UI")]
    [SerializeField]
    public GameObject viewItemContainerElement;

    [SerializeField]
    private GameObject content_ItemContainer;

    [SerializeField]
    private ScrollRect viewItemContainerScrollRect;

    [SerializeField]
    public GameObject viewListContainerElement;

    [SerializeField]
    private GameObject content_ListContainer;

    [SerializeField]
    private ScrollRect viewListContainerScrollRect;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject containerPrefab;
    [SerializeField]
    private GameObject itemPrefab;

    [SerializeField]
    private ContainerButton currentlySelectedContainerButton;

    [SerializeField]
    private List<ContainerButton> containerButtons = new List<ContainerButton>();

    public static void SetSelectedContainerButton(ContainerButton button)
    {
        InventoryViewAroundEntities.Ins.currentlySelectedContainerButton = button;
        InventoryViewAroundEntities.Ins.ClearItemContainerViews();
        InventoryViewAroundEntities.Ins.TryGetSelectedContainer(out Container container);
        container.Changed += InventoryViewAroundEntities.Ins.UpdateItemContainerUI; // Subscribe to changes in the container to update the UI when items are added/removed
    }

    public static ContainerButton GetSelectedContainerButton()
    {
        return InventoryViewAroundEntities.Ins.currentlySelectedContainerButton;
    }

    public GameObject GetContentScrollViewOfContainerList()
    {
        return content_ListContainer;
    }

    private void Start()
    {
        if(viewItemContainerElement == null || content_ItemContainer == null || viewItemContainerScrollRect == null
            || viewListContainerElement == null || content_ListContainer == null || viewListContainerScrollRect == null)
        {
            Debug.LogError("InventoryView: One or more UI references are not set!");
        }
    }

    public void MakeContainerListUI(List<Tile> needAdd, List<Tile> needRemove)
    {
        // First, remove any containers that are no longer nearby
        if(needRemove.Count > 0)
        {
            List<GameObject> listOfContainerGOsToRemove = new List<GameObject>();
            foreach(var item in content_ListContainer.GetComponentsInChildren<Transform>().ToList())
            {
                ContainerButton button = item.GetComponent<ContainerButton>();
                if(button != null)
                {
                    GameObject containerGO = button.GetOwner();
                    if(containerGO != null && needRemove.Exists(t => t.GetGameObject() == containerGO))
                    {
                        listOfContainerGOsToRemove.Add(item.gameObject);
                        if(currentlySelectedContainerButton != null && currentlySelectedContainerButton.GetOwner() == containerGO)
                        {
                            Tile tile = needRemove.Find(t => t.GetGameObject() == containerGO);
                            if(tile != null && tile.TryGetContainer(out Container container))
                            {
                                container.Changed -= UpdateItemContainerUI; // Unsubscribe from changes in the container since it's being removed
                            }
                            currentlySelectedContainerButton = null; // Deselect if the currently selected container is being removed
                            ClearItemContainerViews();
                        }
                    }
                }
            }

            foreach (var element in listOfContainerGOsToRemove)
            {
                Destroy(element);
            }
        }
        
        
        if(needAdd.Count > 0)
        {
            // Then, add any new nearby containers
            foreach (Tile tile in needAdd)
            {
                AddContainerListForTile(tile);
            }
        }
    }

    public void TryGetSelectedContainer(out Container container)
    {
        container = null;
        if(currentlySelectedContainerButton == null)
        {
            ClearItemContainerViews();
            return;
        }
        GameObject containerGO = currentlySelectedContainerButton.GetOwner();
        if(containerGO == null)
        {
            ClearItemContainerViews();
            return;
        }
        float rad = ShowInventoryGUI.Instance.aroundSearchRadius;
        Tile tile = ShowInventoryGUI.Instance.GetAllTilesAroundTarget(rad).Find(t => t.GetGameObject() == containerGO);
        container = tile != null && tile.TryGetContainer(out Container c) ? c : null;
        if(container != null)
        {
            MakeItemContainerUI(container);
        }
    }

    private void AddContainerListForTile(Tile tile)
    {
        GameObject containerGO = Instantiate(containerPrefab, content_ListContainer.transform);
        float index = content_ListContainer.GetComponent<RectTransform>().rect.width;
        containerGO.GetComponent<RectTransform>().sizeDelta = new Vector2(index, index);
        ContainerButton containerButton = containerGO.GetComponent<ContainerButton>();
        containerButtons.Add(containerButton);
        containerButton.SetOwner(tile.GetGameObject());
    }

    public void RemoveContainerByTile(ContainerButton uiElement)
    {
        if(containerButtons.Contains(uiElement))
        {
            containerButtons.Remove(uiElement);
        }
    }

    private void ClearItemContainerViews()
    {
        foreach(var item in content_ItemContainer.GetComponentsInChildren<Transform>().ToList())
        {
            if(item.gameObject != content_ItemContainer)
            {
                Destroy(item.gameObject);
            }
        }
    }

    private void MakeItemContainerUI(Container container)
    {
        foreach(var item in container.items)
        {
            GameObject itemGO = Instantiate(itemPrefab, content_ItemContainer.transform);
            itemGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30); // Set height, width will be stretched by layout group
            // Set item info on the UI element here, e.g. item name, quantity, etc.
            TMPro.TextMeshProUGUI text = itemGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            text.enabled = true;
            text.text = item.GetInfo();
        }
    }

    public void UpdateItemContainerUI()
    {
        Debug.LogWarning("Container changed, updating item container UI...!");
        ClearItemContainerViews();
        TryGetSelectedContainer(out Container container);
    }
}
