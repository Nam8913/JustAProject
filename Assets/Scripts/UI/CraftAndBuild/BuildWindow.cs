#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BuildWindow : UIWindow
{
    [Header("References")]
    [SerializeField] private GameObject content_categoryContent;
    [SerializeField] private GameObject content_structureRecipesContent;
    [SerializeField] private Button buildButton;
    [SerializeField] private TMPro.TMP_Text infor;
    [SerializeField] private GameObject holderListBuildModeButtons;
    [SerializeField] private TMPro.TMP_Text labelBuildMode;

    [Header("Prefabs")]
    [SerializeField] private GameObject categoryPrefab;
    [SerializeField] private GameObject recipePrefab;

    [Header("Temp")]
    private RectTransform holderInforText;
    private List<StructureRecipe> structureRecipes = new List<StructureRecipe>();
    private string currentSelectedRecipeId = string.Empty;
    private string currentSelectedCategory = string.Empty;

    public void ToggleSelf()
    {
        if (gameObject.activeSelf)
        {
            GameUI.Instance.CloseWindow("mainPanel");
        }
        else
        {
            GameUI.Instance.OpenWindow<BuildWindow>("mainPanel", true);
        }
    }

    private void Awake()
    {
        // Load recipes from the database
        LoadRecipes();
    }

    private void Start()
    {
        buildButton.interactable = false; // Disable build button initially
        
        //Setup cateogory UI
        holderInforText = infor.transform.parent.GetComponent<RectTransform>();
        float currentWidthCategory = this.transform.Find("CategoryScrollView").GetComponent<RectTransform>().rect.width;
        GridLayoutGroup gridLayout = content_categoryContent.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(currentWidthCategory, gridLayout.cellSize.y);
        List<string> categories = structureRecipes.Select(r => r.category).Distinct().ToList();
        foreach(var category in categories)
        {
            GameObject categoryGO = Instantiate(categoryPrefab, content_categoryContent.transform);
            categoryGO.GetComponentInChildren<TMPro.TMP_Text>().text = category;
            Button button = categoryGO.GetComponent<Button>();
            button.onClick.AddListener(() => OnCategoryClicked(category));
        }

        List<GameObject> buildModeButtons = new List<GameObject>();
        foreach(Transform child in holderListBuildModeButtons.transform)
        {
            buildModeButtons.Add(child.gameObject);
            #if DEBUG_LOG_FLAG && false && false
            Debug.Log($"Found build mode button: {child.gameObject.name}");
            #endif
            Button button = child.GetComponent<Button>();
            if(button != null)
            {
                button.onClick.AddListener(() => OnBuildModeButtonClicked(button));
            }
            button.interactable = false; // Disable all buttons initially
        }


        
        // Set default build mode to Single
        SetSingleBuildMode();
    }

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        
    }

    private void OnCategoryClicked(string category)
    {
        if(currentSelectedCategory == category)
        {
            return;
        }
        currentSelectedCategory = category;
        foreach(Transform child in content_structureRecipesContent.transform)
        {
            Destroy(child.gameObject);
        }

        List<StructureRecipe> structureRecipesInCategory = structureRecipes.Where(r => r.category == category).ToList();
        foreach(var recipe in structureRecipesInCategory)
        {
            GameObject recipeGO = Instantiate(recipePrefab, content_structureRecipesContent.transform);
            recipeGO.GetComponentInChildren<TMPro.TMP_Text>().text = recipe.Id;
            Button button = recipeGO.GetComponent<Button>();
            button.onClick.AddListener(() => OnRecipeClicked(recipe));
        }


        foreach(Transform child in holderListBuildModeButtons.transform)
        {
            Button button = child.GetComponent<Button>();
            if(button != null)
            {
                button.interactable = false; // Disable all buttons when a new category is selected
            }
        }
    }

    private void OnRecipeClicked(StructureRecipe recipe)
    {
        if(currentSelectedRecipeId == recipe.Id)
        {
            return;
        }
        currentSelectedRecipeId = recipe.Id;
        string inforText = GetInforTextForRecipe(recipe);
        DrawInforText(inforText);

        if(IsEnoughResourcesForRecipe(recipe))
        {
            buildButton.interactable = true;
            buildButton.onClick.RemoveAllListeners();
            buildButton.onClick.AddListener(() => {
                this.Hide(); // Close the build window before triggering build mode
                BuildUtility.TriggerBuildMode();
                Define selectedStructure = GetDefineStructureRecipe(recipe.result);
                if(selectedStructure == null)
                {
                    Debug.LogError($"No Define found for recipe {recipe.result}. Cannot set selected structure.");
                    return;
                }
                BuildUtility.SetSelectedStructure(selectedStructure);
            });
        }
        else
        {
            buildButton.interactable = false;
            buildButton.onClick.RemoveAllListeners();
        }

        // Enable all build mode buttons when a recipe is selected
        foreach(Transform child in holderListBuildModeButtons.transform)
        {
            Button button = child.GetComponent<Button>();
            if(button != null)
            {
                button.interactable = true; // Enable all buttons when a recipe is selected
            }
        }

        // Set build mode to single by default when a recipe is selected
        SetSingleBuildMode();
    }

    private void DrawInforText(string text)
    {
        infor.text = text;
        Vector2 getPreferredSize = infor.GetPreferredValues(text,infor.GetComponent<RectTransform>().rect.width,0);
        holderInforText.sizeDelta = new Vector2(holderInforText.sizeDelta.x, getPreferredSize.y + 20); // Add some padding
    }

    private void ClearInforText()
    {
        infor.text = string.Empty;
        holderInforText.sizeDelta = new Vector2(holderInforText.sizeDelta.x, holderInforText.sizeDelta.y); // Reset to default height
    }

    private string GetInforTextForRecipe(StructureRecipe recipe)
    {
        string text = $"<b>{recipe.Id}</b>\n\n";
        text += $"Category: {recipe.category}\n\n";
        
        text += "Ingredients:\n";
        foreach(var component in recipe.components)
        {
            text += "- ";
            List<string> options = new List<string>();
            foreach(var option in component.Options)
            {
                options.Add($"{option.quantity}x {option.thingId}");
            }
            text += string.Join(" OR ", options);
            text += "\n";
        }
        
        text += "\nRequired Tool Qualities:\n";
        foreach(var quality in recipe.qualities)
        {
            text += $"- {quality.id} (Level {quality.level})\n";
        }
        return text;
    }

    public bool IsEnoughResourcesForRecipe(StructureRecipe recipe)
    {
        // DEV: For now, we will just return true. In the future, we will check the player's inventory to see if they have enough resources for the recipe.
        return true;
    }

    public void LoadRecipes()
    {
        structureRecipes = DatabaseThing.Store[typeof(StructureRecipe)].Values.Cast<StructureRecipe>().ToList();
        foreach(var recipe in structureRecipes)
        {
            #if DEBUG_LOG_FLAG && false
            Debug.Log($"Loaded structure recipe: {recipe.Id}");
            #endif
        }
    }

    void OnBuildModeButtonClicked(Button button)
    {
        lock (buildModeLock)
        {
            string buttonName = button.gameObject.name;
            if(System.Enum.TryParse(buttonName, out BuildUtility.BuildMode parsedMode))
            {
                #if DEBUG_LOG_FLAG && false
                Debug.Log($"Current Build Mode: {BuildUtility.CurrentBuildMode}, Selected Build Mode: {parsedMode}");
                #endif
                if(BuildUtility.CurrentBuildMode == parsedMode)
                {
                    SetSingleBuildMode();
                }else
                {
                    if (labelBuildMode != null)
                    {
                        labelBuildMode.text = $"Mode: {parsedMode}";
                    }
                    BuildUtility.SetBuildMode(parsedMode);
                }
                #if DEBUG_LOG_FLAG && false
                Debug.Log($"Build Mode set to: {BuildUtility.CurrentBuildMode}");
                #endif
            }
            
        }
    }

    void SetSingleBuildMode()
    {
        if (labelBuildMode != null)
        {
            labelBuildMode.text = "Mode: Single";
        }
        BuildUtility.SetBuildMode(BuildUtility.BuildMode.Single);
    }

    Define GetDefineStructureRecipe(string recipeId)
    {
        if(DatabaseThing.Store.TryGetValue(typeof(Define), out var defineDict))
        {
            if(defineDict.TryGetValue(recipeId, out var recipeObj))
            {
                return recipeObj as Define;
            }
        }else
        {
            Debug.LogError($"No recipes found in DatabaseThing.Store for type {typeof(Define)}");
        }
        return null;
    }

    readonly object buildModeLock = new object();
}
