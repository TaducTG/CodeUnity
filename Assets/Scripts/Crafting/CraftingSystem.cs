using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public Inventory inventory;
    private bool hasCrafted = false;
    private void Start()
    {
        inventory = FindAnyObjectByType<Inventory>();
    }
    // Kiểm tra có đủ nguyên liệu không
    public bool CanCraft(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            int total = inventory.GetTotalItem(ingredient.item);

            if (total < ingredient.quantity)
                return false;
        }
        return true;
    }

    // Thực hiện chế tạo
    public bool Craft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe))
        {
            Debug.Log("Không đủ nguyên liệu để chế!");
            return false;
        }
        if (hasCrafted)
        {
            return false;
        }
        // Trừ nguyên liệu
        foreach (var ingredient in recipe.ingredients)
        {
            inventory.RemoveItem(ingredient.item, ingredient.quantity);
        }
        hasCrafted = true;
        // Thêm vật phẩm tạo ra
        inventory.AddItems(recipe.outputItem, recipe.outputAmount);
        Debug.Log("Đã chế tạo " + recipe.outputItem.name);
        hasCrafted = false;
        return true;
    }
}
