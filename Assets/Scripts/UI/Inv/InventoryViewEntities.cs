#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryViewEntities : MonoBehaviour,IInventoryView
{
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
    
    [Header("Temp")]
    [SerializeField]
    private ContainerButton currentlySelectedContainerButton;

    private Container currentlySelectedContainer;

    [SerializeField]
    private List<ContainerButton> containerButtons = new List<ContainerButton>();

    private bool isGetContainerFromTarget = false;

    private void Start()
    {
        if(viewItemContainerElement == null || content_ItemContainer == null || viewItemContainerScrollRect == null
            || viewListContainerElement == null || content_ListContainer == null || viewListContainerScrollRect == null)
        {
            Debug.LogError("InventoryView: One or more UI references are not set!");
        }

        DefineThing target = ShowInventoryGUI.Instance.GetTargetToShow();
        if(target.TryGetContainer(out Container root))
        {
            SetContainers(new List<Container>() { root });
        }
    }

    void OnGUI()
    {
        DefineThing target = ShowInventoryGUI.Instance.GetTargetToShow();
        if(target == null)
        {
            isGetContainerFromTarget = false;
        }

        if(isGetContainerFromTarget)
        {
            return;
        }

        

        if(target != null)
        {
            if(target.TryGetContainer(out Container root))
            {
                SetContainers(new List<Container>() { root });
                isGetContainerFromTarget = true;
            }
        }
    }

    public void SetContainers(List<Container> containers)
    {
        ClearContainerViews();
        foreach(var container in containers)
        {
            AddContainerView(container);
        }
    }

    private void AddContainerView(Container container)
    {
        GameObject containerGO = Instantiate(containerPrefab, content_ListContainer.transform);
        float index = content_ListContainer.GetComponent<RectTransform>().rect.width;
        containerGO.GetComponent<RectTransform>().sizeDelta = new Vector2(index, index);
        ContainerButton containerButton = containerGO.GetComponent<ContainerButton>();
        containerButton.SetUI(this);
        containerButton.SetOwner(ShowInventoryGUI.Instance.GetTargetToShow().gameObject);
        containerButtons.Add(containerButton);
    }

    private void ClearContainerViews()
    {
        bool hasAnyContainer = content_ListContainer.transform.childCount > 0;
        if(!hasAnyContainer)
        {
            return;
        }
        foreach (Transform child in content_ListContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void RefreshContainerButtonVisuals()
    {
        foreach (ContainerButton button in containerButtons)
        {
            if (button != null)
            {
                button.SetSelected(button == currentlySelectedContainerButton);
                button.SetButtonSelected(button == currentlySelectedContainerButton);
            }
        }
    }

    #region private Methods

    private void TryGetSelectedContainer(out Container container)
    {
        container = null;
    }

    private void UnSelectedCurrContainer()
    {
        if (currentlySelectedContainer != null)
        {
            currentlySelectedContainer.Changed -= CallbackOnContainerChanged;
            currentlySelectedContainer = null;
        }
    }

    private void MakeContainerFromSelectedContainerButton()
    {
        TryGetSelectedContainer(out Container container);
        if (container == null)
        {
            return;
        }

        currentlySelectedContainer = container;
        MakeItemContainerUI(container);
        currentlySelectedContainer.Changed += CallbackOnContainerChanged;
    }
    #endregion

    #region ContainerItem UI Methods
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
    #endregion

    private void CallbackOnContainerChanged()
    {
        #if DEBUG_LOG_FLAG && false
        Debug.Log("Container changed, refreshing item container UI.");
        #endif
        ClearItemContainerViews();
        TryGetSelectedContainer(out Container container);
        if (container == null)
        {
            return;
        }
        MakeItemContainerUI(container);
    }

    public void GetSelectedContainer(ContainerButton button)
    {
        if(button == null)
        {
            return;
        }

        if(currentlySelectedContainerButton == button)
        {
            return; // Already selected
        }

        ClearItemContainerViews();
        this.UnSelectedCurrContainer();
        this.currentlySelectedContainerButton = button;
        this.RefreshContainerButtonVisuals();
        this.MakeContainerFromSelectedContainerButton();
    }

    public void TryRemoveContainerButton(ContainerButton button)
    {
        if(currentlySelectedContainerButton == button)
        {
            UnSelectedCurrContainer();
            ClearItemContainerViews();
        }

        if(containerButtons.Contains(button))
        {
            containerButtons.Remove(button);
        }
    }
}
