using UnityEngine;
public enum EquipmentSlotType
{
    None,
    Helmet,
    Chestplate,
    Boots,
    Accessory
}

[CreateAssetMenu(fileName = "New Equipment Item", menuName = "Items/Equipment")]
public class EquipmentItem : Items
{
    public EquipmentSlotType slotType;

    // Các chỉ số cộng thêm khi trang bị
    public int bonusHealth;
    public int bonusDefense;
    public int bonusMana;

    public float bonusMoveSpeed;
    public float bonusDamage;
    [Header("Accessory")]
    public float bonusHealthRegenerate;
    public float bonusManaRegenerate;

    public float bonusPercentHealth;
    public float bonusPercentDefense;
    public float bonusPercentMana;
    public float bonusPercentDamage;
    public float bonusPercentMoveSpeed;
    public void ApplyStats(Player player)
    {
        player.playerStat.equipHealth += bonusHealth;
        player.playerStat.equipDefense += bonusDefense;
        player.playerStat.equipMana += bonusMana;
        player.playerStat.equipPercentSpeed += bonusMoveSpeed;
        player.playerStat.equipDamage += bonusDamage;

        player.playerStat.equipPercentHealth += bonusPercentHealth;
        player.playerStat.equipPercentMana += bonusPercentMana;
        player.playerStat.equipPercentDamage += bonusPercentDamage;
        player.playerStat.equipPercentDefense += bonusPercentDefense;
        player.playerStat.equipPercentSpeed += bonusPercentMoveSpeed;

        player.playerStat.equipHealthRegenerate += bonusHealthRegenerate;
        player.playerStat.equipManaRegenerate += bonusManaRegenerate;
    }

    // Trừ chỉ số khi bỏ trang bị
    public void RemoveStats(Player player)
    {
        player.playerStat.equipHealth -= bonusHealth;
        player.playerStat.equipDefense -= bonusDefense;
        player.playerStat.equipMana -= bonusMana;
        player.playerStat.equipPercentSpeed -= bonusMoveSpeed;
        player.playerStat.equipDamage -= bonusDamage;

        player.playerStat.equipPercentHealth -= bonusPercentHealth;
        player.playerStat.equipPercentMana -= bonusPercentMana;
        player.playerStat.equipPercentDamage -= bonusPercentDamage;
        player.playerStat.equipPercentDefense -= bonusPercentDefense;
        player.playerStat.equipPercentSpeed -= bonusPercentMoveSpeed;

        player.playerStat.equipHealthRegenerate -= bonusHealthRegenerate;
        player.playerStat.equipManaRegenerate -= bonusManaRegenerate;
    }
}
