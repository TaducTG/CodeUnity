using NUnit.Framework.Interfaces;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public Items outputItem;
    public int outputAmount = 1;

    [System.Serializable]
    public class Ingredient
    {
        public Items item;
        public int quantity;
    }

    public Ingredient[] ingredients;
}
