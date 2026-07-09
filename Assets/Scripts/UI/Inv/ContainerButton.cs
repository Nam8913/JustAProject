#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using UnityEngine;
using UnityEngine.UI;

public class ContainerButton : MonoBehaviour
{
    private IInventoryView ui;

    [SerializeField]
    private Color selectedObjectColor = new Color(0.76f, 0.68f, 0.05f, 0.98f);
    [SerializeField]
    private Color selectedButtonColor = new Color(0.35f, 0.48f, 0.74f, 0.98f);
    [SerializeField]
    private Color normalButtonColor = new Color(1f, 1f, 1f, 0.98f);

    private Button button;
    private Image buttonImage;

    [SerializeField]
    private GameObject owner;

    private SpriteRenderer[] ownerRenderers = new SpriteRenderer[0];
    private Color[] ownerOriginalColors = new Color[0];
    private bool isSelected;

    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    void Start()
    {
        if (button == null)
        {
            Debug.LogError("ContainerButton requires a Button component.");
            return;
        }

        button.onClick.AddListener(GetClicked);
    }

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        ApplyOwnerHighlight();
    }

    public void SetButtonSelected(bool isSelected)
    {
        if (buttonImage == null)
        {
            return;
        }

        buttonImage.color = isSelected ? selectedButtonColor : normalButtonColor;
    }

    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
        CacheOwnerRenderers();
        ApplyOwnerHighlight();
    }

    public GameObject GetOwner()
    {
        return owner;
    }

    private void CacheOwnerRenderers()
    {
        if (owner == null)
        {
            ownerRenderers = new SpriteRenderer[0];
            ownerOriginalColors = new Color[0];
            return;
        }

        ownerRenderers = owner.GetComponentsInChildren<SpriteRenderer>(true);
        ownerOriginalColors = new Color[ownerRenderers.Length];

        for (int index = 0; index < ownerRenderers.Length; index++)
        {
            SpriteRenderer renderer = ownerRenderers[index];
            ownerOriginalColors[index] = renderer != null ? renderer.color : Color.white;
        }
    }

    private void ApplyOwnerHighlight()
    {
        if (owner == null)
        {
            return;
        }

        if (ownerRenderers == null || ownerOriginalColors == null || ownerRenderers.Length != ownerOriginalColors.Length)
        {
            CacheOwnerRenderers();
        }

        for (int index = 0; index < ownerRenderers.Length; index++)
        {
            SpriteRenderer renderer = ownerRenderers[index];
            if (renderer == null)
            {
                continue;
            }

            renderer.color = isSelected ? selectedObjectColor : ownerOriginalColors[index];
        }
    }

    void GetClicked()
    {
        if (owner == null)
        {
            Debug.LogWarning("Clicked container button without an owner.");
            return;
        }

        #if DEBUG_LOG_FLAG && false
        Debug.Log($"Clicked container button with owner {owner.name}");
        #endif
        //InventoryViewAroundEntities.SetSelectedContainerButton(this);
        ui.GetSelectedContainer(this);
    }

    void OnDestroy()
    {
        if (isSelected)
        {
            isSelected = false;
            ApplyOwnerHighlight();
        }

        if (ui != null)
        {
            ui.TryRemoveContainerButton(this);
        }
    }

    public void SetUI(IInventoryView ui)
    {
        this.ui = ui;
    }
}
