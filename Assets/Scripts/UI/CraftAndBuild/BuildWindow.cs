using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BuildWindow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject content_categoryContent;
    [SerializeField] private GameObject content_structureRecipesContent;
    [SerializeField] private Button buildButton;
    [SerializeField] private TMPro.TMP_Text infor;

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
        this.gameObject.SetActive(!this.gameObject.activeSelf);
        PlayUI.SetFocusAtWindow(this.GetComponent<RectTransform>());
    }

    private void Start()
    {
        //Setup cateogory UI
        holderInforText = infor.transform.parent.GetComponent<RectTransform>();
        LoadRecipes();
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

    public void LoadRecipes()
    {
        structureRecipes = DatabaseThing.Store[typeof(StructureRecipe)].Values.Cast<StructureRecipe>().ToList();
        foreach(var recipe in structureRecipes)
        {
            Debug.Log($"Loaded structure recipe: {recipe.Id}");
        }
    }
}
