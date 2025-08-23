using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CraftingUIManager : MonoBehaviour
{
    public GameObject recipeButtonPrefab;
    public Transform recipeListParent;
    public GameObject ingredientPanel;
    public GameObject ingredientSlotPrefab;
    public Transform ingredientListParent;

    public List<CraftingRecipe> availableRecipes;
    public CraftingSystem craftingSystem;

    public GameObject Craftbutton;
    public GameObject closeButton;

    [SerializeField] private string craftingTableName; // Nhập tên bàn chế tạo

    void Awake()
    {
        // Tìm các thành phần UI theo đúng cấu trúc trong Canvas
        Transform canvas = GameObject.Find("Canvas")?.transform;

        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        // Tìm root Crafting trong Canvas
        Transform craftingRoot = canvas.Find("Crafting");

        if (craftingRoot == null)
        {
            Debug.LogError("Không tìm thấy GameObject 'Crafting' trong Canvas!");
            return;
        }

        Craftbutton = craftingRoot.Find("Crafting_detail/Craftbutton")?.gameObject;
        closeButton = craftingRoot.Find("Crafting_detail/exit")?.gameObject;
        ingredientPanel = craftingRoot.Find("Crafting_detail")?.gameObject;
        recipeListParent = craftingRoot.Find("Crafting_recipe_panel/Show_recipe_panel");
        ingredientListParent = craftingRoot.Find("Crafting_detail/Show_ingredient_panel");

        // Load recipe theo thư mục dựa trên tên bàn chế tạo
        availableRecipes = new List<CraftingRecipe>(Resources.LoadAll<CraftingRecipe>($"Recipe/Crafting/{craftingTableName}/"));

        if (availableRecipes.Count == 0)
        {
            Debug.LogWarning($"Không tìm thấy recipe nào trong Recipe/Crafting/{craftingTableName}/");
        }
    }

    void Start()
    {
        PopulateRecipeButtons();
    }

    // Hàm xử lý in các recipe dưới dạng các button lên UI
    public void PopulateRecipeButtons()
    {
        foreach (Transform child in recipeListParent)
        {
            Destroy(child.gameObject);
        }
        foreach (CraftingRecipe recipe in availableRecipes)
        {
            GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeListParent);

            Image iconImage = buttonObj.GetComponentInChildren<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = recipe.outputItem.icon;
            }

            buttonObj.GetComponent<Button>().onClick.AddListener(() => ShowIngredients(recipe));
        }
    }

    void ShowIngredients(CraftingRecipe recipe)
    {
        foreach (Transform child in ingredientListParent)
        {
            Destroy(child.gameObject);
        }

        ingredientPanel.SetActive(true);

        foreach (var ingredient in recipe.ingredients)
        {
            GameObject slot = Instantiate(ingredientSlotPrefab, ingredientListParent);

            Image iconImage = slot.GetComponentInChildren<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = ingredient.item.icon;
            }

            TextMeshProUGUI quantityText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (quantityText != null)
            {
                quantityText.text = ingredient.quantity.ToString();
            }
        }

        Craftbutton.GetComponent<Button>().onClick.RemoveAllListeners();
        Craftbutton.GetComponent<Button>().onClick.AddListener(() => craftingSystem.Craft(recipe));

        closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        closeButton.GetComponent<Button>().onClick.AddListener(() => ingredientPanel.SetActive(false));
    }
}
