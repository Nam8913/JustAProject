using UnityEngine;
using UnityEngine.UI;

public class PlayUI : MonoBehaviour
{
    [Header("Ref_Pages")]
    static PlayUI ins;
    [SerializeField] private CraftWindow craftWindow;
    [SerializeField] private BuildWindow buildWindow;

    [Header("Ref_Buttons")]
    [SerializeField] private Button showBlueprintButton;
    [SerializeField] private Button craftWindowButton;
    [SerializeField] private Button buildWindowButton;

    [Header("Temp")]
    [SerializeField] private RectTransform focusWindow;

    private bool lastObservedStateOfFocusObject = false;

    public static CraftWindow CraftWindow
    {
        get
        {
            return ins.craftWindow;
        }
    }

    public static BuildWindow BuildWindow
    {
        get
        {
            return ins.buildWindow;
        }
    }

    private void Awake()
    {
        if(ins != null && ins != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            ins = this;
        }
        OnAnyButtonClicked();
    }

    private void OnGUI()
    {
        bool hasFocusObject = GameService.Ins.GetFocusObject() != null;
        if(hasFocusObject != lastObservedStateOfFocusObject)
        {
            if(hasFocusObject)
            {
                MakeButtonsInteractable();
            }
            else
            {
                MakeButtonsNotInteractable();
            }
            lastObservedStateOfFocusObject = hasFocusObject;
        }

        if(GameService.PlayerInput.UI.Cancel.WasPressedThisFrame())
        {
            Debug.Log("Cancel button was pressed. Toggling focus window.");
            if(focusWindow != null)
            {
                focusWindow.gameObject.SetActive(false);
            }
        }
    }

    private void MakeButtonsNotInteractable()
    {
        craftWindowButton.interactable = false;
        buildWindowButton.interactable = false;
    }

    private void MakeButtonsInteractable()
    {
        craftWindowButton.interactable = true;
        buildWindowButton.interactable = true;
    }

    public void OnAnyButtonClicked()
    {
        //Set all pages to inactive
        craftWindow.gameObject.SetActive(false);
        buildWindow.gameObject.SetActive(false);
    }

    public static void SetFocusAtWindow(RectTransform window)
    {
        ins.focusWindow = window;
    }
}
