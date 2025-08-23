using UnityEngine;
public enum EquipmentSlotType
{
    None,
    Helmet,
    Chestplate,
    Boots
}

[CreateAssetMenu(fileName = "New Equipment Item", menuName = "Items/Equipment")]
public class EquipmentItem : Items
{
    public EquipmentSlotType slotType;

    // Các chỉ số cộng thêm khi trang bị
    public int bonusHealth;
    public int bonusDefense;
    public int bonusMana;
    public void ApplyStats(Player player)
    {
        player.maxHealth += bonusHealth;
        player.baseDef += bonusDefense;
        player.maxMana += bonusMana;
    }

    // Trừ chỉ số khi bỏ trang bị
    public void RemoveStats(Player player)
    {
        player.maxHealth -= bonusHealth;
        player.baseDef -= bonusDefense;
        player.maxMana -= bonusMana;
    }
}
