using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewGameWindow : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameWorldInputField;
    [SerializeField] private TMP_InputField seedInputField;

    [Header("References")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject contentWindow;
    
    private List<GameObject> pages = new List<GameObject>();
    private int currentPageIndex = 0;

    void Start()
    {
        if(nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }
        if(backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // Initialize pages list based on contentWindow's children
        if(contentWindow != null)
        {
            foreach(Transform child in contentWindow.transform)
            {
                pages.Add(child.gameObject);
                child.gameObject.SetActive(false); // Hide all pages initially
            }
            if(pages.Count > 0)
            {
                pages[0].SetActive(true); // Show the first page
            }
        }
    }

    public void CreateNewGame()
    {
        string worldName = nameWorldInputField.text;
        string seed = seedInputField.text;
        
        int seedValue = string.IsNullOrEmpty(seed) ? Random.Range(int.MinValue, int.MaxValue) : seed.GetHashCode();

        Debug.Log($"Creating new world with name: {worldName} and seed: {seedValue}");
        

        World newWorld = new World(worldName, seedValue);
        World.RegisterNewWorldToCurrentWorld(newWorld);

        SceneHandler.LoadPlayScene();
    }

    private void OnNextButtonClicked()
    {
        if(currentPageIndex < pages.Count - 1)
        {
            pages[currentPageIndex].SetActive(false);
            currentPageIndex++;
            pages[currentPageIndex].SetActive(true);
        }else
        {
            EventQueue.Enqueue("CreateNewGame", CreateNewGame);
            EventQueue.Enqueue("CloseNewGameWindow", () =>
            {
                DefineThing def = ThingHandler.CreateThingById("HumanDef");
                def.transform.position = new Vector3(0, 0, -0.1f);

                GameObject render = new GameObject("render");
                render.transform.SetParent(def.gameObject.transform);
                render.transform.localPosition = Vector3.zero;
                SpriteRenderer spriteRenderer = render.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = LocalRefDefaultRS.GetSpriteByName("Circle");
                render.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

                GameService.Ins.SetFocusObject(def.gameObject);
            });
            Reset();
        }
    }

    private void OnBackButtonClicked()
    {
        if(currentPageIndex > 0)
        {
            pages[currentPageIndex].SetActive(false);
            currentPageIndex--;
            pages[currentPageIndex].SetActive(true);
        }else
        {
            // Optionally, you can close the new game window or go back to the main menu
            this.gameObject.SetActive(false);
            Reset();
        }
    }

    void Reset()
    {
        currentPageIndex = 0;
        nameWorldInputField.text = string.Empty;
        seedInputField.text = string.Empty;
    }
}
