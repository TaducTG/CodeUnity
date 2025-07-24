using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public Inventory inventory;

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

        // Trừ nguyên liệu
        foreach (var ingredient in recipe.ingredients)
        {
            inventory.RemoveItem(ingredient.item, ingredient.quantity);
        }

        // Thêm vật phẩm tạo ra
        inventory.AddItems(recipe.outputItem, recipe.outputAmount);
        Debug.Log("Đã chế tạo " + recipe.outputItem.name);
        return true;
    }
}
