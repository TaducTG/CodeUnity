using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements.Experimental;
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

        // KHÔNG tìm Crafting trong chính nó nữa!
        Craftbutton = craftingRoot.Find("Crafting_detail/Craftbutton")?.gameObject;
        closeButton = craftingRoot.Find("Crafting_detail/exit")?.gameObject;
        ingredientPanel = craftingRoot.Find("Crafting_detail")?.gameObject;
        recipeListParent = craftingRoot.Find("Crafting_recipe_panel/Show_recipe_panel");
        ingredientListParent = craftingRoot.Find("Crafting_detail/Show_ingredient_panel");

        // Debug kiểm tra
        if (Craftbutton == null) Debug.LogError("Không tìm thấy Craftbutton!");
        if (closeButton == null) Debug.LogError("Không tìm thấy exit (CloseButton)!");
        if (ingredientPanel == null) Debug.LogError("Không tìm thấy Show_ingredient_panel!");
        if (recipeListParent == null) Debug.LogError("Không tìm thấy Show_recipe_panel!");
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

            // Tìm Image trong prefab (dùng để hiển thị icon)
            Image iconImage = buttonObj.GetComponentInChildren<Image>();

            if (iconImage != null)
            {
                iconImage.sprite = recipe.outputItem.icon; // icon từ item output
            }
            else
            {
                Debug.LogWarning("Không tìm thấy Image trong recipeButtonPrefab");
            }

            // Gán sự kiện khi click
            buttonObj.GetComponent<Button>().onClick.AddListener(() => ShowIngredients(recipe));
        }
    }

    void ShowIngredients(CraftingRecipe recipe)
    {
        // Xoá các nguyên liệu cũ
        foreach (Transform child in ingredientListParent)
        {
            Destroy(child.gameObject);
        }

        ingredientPanel.SetActive(true);

        // Tạo UI cho từng nguyên liệu

        foreach (var ingredient in recipe.ingredients)
        {
            GameObject slot = Instantiate(ingredientSlotPrefab, ingredientListParent);

            // Gán icon của nguyên liệu
            Image iconImage = slot.GetComponentInChildren<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = ingredient.item.icon;
            }
            else
            {
                Debug.LogWarning("Không tìm thấy Image trong ingredientSlotPrefab.");
            }

            // Gán số lượng nguyên liệu bằng TextMeshPro
            TextMeshProUGUI quantityText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (quantityText != null)
            {
                quantityText.text = ingredient.quantity.ToString();
            }
            else
            {
                Debug.LogWarning("Không tìm thấy TextMeshProUGUI trong ingredientSlotPrefab.");
            }
        }



        // Tuỳ chọn: thêm nút Craft
        Craftbutton.GetComponent<Button>().onClick.RemoveAllListeners();
        Craftbutton.GetComponent<Button>().onClick.AddListener(() => craftingSystem.Craft(recipe));

        //Exit
        closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        closeButton.GetComponent<Button>().onClick.AddListener(() => ingredientPanel.SetActive(false));
    }
}
