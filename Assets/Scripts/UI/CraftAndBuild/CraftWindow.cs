using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CraftWindow : UIWindow
{
    [Header("References")]
    [SerializeField] private GameObject content_categoryContent;
    [SerializeField] private GameObject content_recipesContent;
    [SerializeField] private Button craftButton;
    [SerializeField] private TMPro.TMP_Text infor;

    [Header("Prefabs")]
    [SerializeField] private GameObject categoryPrefab;
    [SerializeField] private GameObject recipePrefab;

    [Header("Temp")]
    private DefineThing owner;
    private RectTransform holderInforText;
    private List<Recipe> recipes = new List<Recipe>();
    private string currentSelectedRecipeId = string.Empty;
    private string currentSelectedCategory = string.Empty;

    public void SetOwner(DefineThing owner)
    {
        this.owner = owner;
    }

    public void ToggleSelf()
    {
        if (gameObject.activeSelf)
        {
            GameUI.Instance.CloseWindow("mainPanel");
        }
        else
        {
            GameUI.Instance.OpenWindow<CraftWindow>("mainPanel", true);
        }
    }

    private void Awake()
    {
        // Load recipes from the database
        LoadRecipes();
    }

    private void Start()
    {
        //Setup cateogory UI
        holderInforText = infor.transform.parent.GetComponent<RectTransform>();
        float currentWidthCategory = this.transform.Find("CategoryScrollView").GetComponent<RectTransform>().rect.width;
        GridLayoutGroup gridLayout = content_categoryContent.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(currentWidthCategory, gridLayout.cellSize.y);
        List<string> categories = recipes.Select(r => r.category).Distinct().ToList();
        foreach(var category in categories)
        {
            GameObject categoryGO = Instantiate(categoryPrefab, content_categoryContent.transform);
            categoryGO.GetComponentInChildren<TMPro.TMP_Text>().text = category;
            Button button = categoryGO.GetComponent<Button>();
            button.onClick.AddListener(() => OnCategoryClicked(category));
        }
    }

    private void OnCategoryClicked(string category)
    {
        if(currentSelectedCategory == category)
        {
            return;
        }
        currentSelectedCategory = category;
        foreach(Transform child in content_recipesContent.transform)
        {
            Destroy(child.gameObject);
        }

        List<Recipe> recipesInCategory = recipes.Where(r => r.category == category).ToList();
        foreach(var recipe in recipesInCategory)
        {
            GameObject recipeGO = Instantiate(recipePrefab, content_recipesContent.transform);
            recipeGO.GetComponentInChildren<TMPro.TMP_Text>().text = recipe.Id;
            Button button = recipeGO.GetComponent<Button>();
            button.onClick.AddListener(() => OnRecipeClicked(recipe));
        }
    }

    private void OnRecipeClicked(Recipe recipe)
    {
        if(currentSelectedRecipeId == recipe.Id)
        {
            return;
        }
        currentSelectedRecipeId = recipe.Id;
        string inforText = GetInforTextForRecipe(recipe);
        DrawInforText(inforText);
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

    private string GetInforTextForRecipe(Recipe recipe)
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

    public void LoadRecipes()
    {
        recipes = DatabaseThing.Store[typeof(Recipe)].Values.Cast<Recipe>().ToList();
        foreach(var recipe in recipes)
        {
            Debug.Log($"Loaded recipe: {recipe.Id}");
        }
    }

    private bool isRecipeCraftable(Recipe recipe)
    {
        ShowInventoryGUI showInventoryGUI = ShowInventoryGUI.Instance;
        if(showInventoryGUI == null)
        {
            return false; // Can't determine if recipe is craftable without access to player's inventory
        }

        ProvideContainer_Comp holder = owner.GetComp<ProvideContainer_Comp>();
        if(holder == null)        {
            return false; // Owner does not have a container to hold crafted item
        }



        // Check if player has required components
        foreach(var component in recipe.components)
        {
            bool hasAtLeastOneOption = false;
            foreach(var option in component.Options)
            {
                if(HasItem(holder.OwnedContainer, option.thingId, option.quantity))
                {
                    hasAtLeastOneOption = true;
                    break;
                }
            }
            if(!hasAtLeastOneOption)
            {
                return false; // Player does not have any of the options for this component
            }
        }

        // // Check if player has required tool qualities
        // foreach(var quality in recipe.qualities)
        // {
        //     if(!GameService.PlayerToolManager.HasToolWithQuality(quality.id, quality.level))
        //     {
        //         return false; // Player does not have a tool with the required quality
        //     }
        // }

        return true; // Player meets all requirements to craft this recipe

    }

    private bool HasItem(Container root,string itemId,int quantity)
    {
        if(root == null)
        {
            return false;
        }

        int get = root.GetItemQuantity(itemId);
        if(get <= 0 || get < quantity)
        {
            return false;
        }

        foreach(Container subContainer in root.children)
        {
            bool any = HasItem(subContainer, itemId, quantity);
            if(any)
            {
                return true;
            }
        }

        return true;

    }
}
