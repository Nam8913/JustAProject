using UnityEngine;
using UnityEngine.UI;

public class ContainerButton : MonoBehaviour
{

    private Button button;
    [SerializeField]
    private GameObject owner;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("ContainerButton requires a Button component.");
            return;
        }

        button.onClick.AddListener(GetClicked);
    }

    // Update is called once per frame
    void OnGUI()
    {
        if(InventoryViewAroundEntities.GetSelectedContainerButton() != null && InventoryViewAroundEntities.GetSelectedContainerButton() == this)
        {
            GetComponent<Image>().color = new Color(0.35f, 0.48f, 0.74f, 0.98f);
        }else
        {
            GetComponent<Image>().color = new Color(0.23f, 0.23f, 0.23f, 0.95f);
        }
    }
    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }

    public GameObject GetOwner()
    {
        return owner;
    }

    void GetClicked()
    {
        Debug.Log($"Clicked container button with owner {owner.name}");
        InventoryViewAroundEntities.SetSelectedContainerButton(this);
    }

    void OnDestroy()
    {
        ShowInventoryGUI.Instance.aroundView.RemoveContainerByTile(this);
    }
}
