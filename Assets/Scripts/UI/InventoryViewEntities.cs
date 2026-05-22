using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryViewEntities : MonoBehaviour
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

    private void Start()
    {
        if(viewItemContainerElement == null || content_ItemContainer == null || viewItemContainerScrollRect == null
            || viewListContainerElement == null || content_ListContainer == null || viewListContainerScrollRect == null)
        {
            Debug.LogError("InventoryView: One or more UI references are not set!");
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
        //containerGO.GetComponent<ContainerButton>().SetOwner();
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
}
