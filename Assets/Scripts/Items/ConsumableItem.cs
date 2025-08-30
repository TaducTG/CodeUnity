using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Items/Consumable")]
public class ConsumableItem : Items
{
    public enum ConsumableEffectType
    {
        Heal,
        RestoreMana,
        Buff
    }

    public enum BuffType
    {
        Damage,
        MoveSpeed,
        Defense
    }

    [System.Serializable]
    public class EffectData
    {
        public ConsumableEffectType effectType;
        public float effectValue = 10f;
        public float duration = 0f;

        // Chỉ dùng nếu là Buff
        public BuffType buffTarget;
    }

    [Header("Consumable Settings")]
    public List<EffectData> effects = new List<EffectData>();

    // Quản lý coroutine đang chạy cho từng buff
    private Dictionary<BuffType, Coroutine> activeBuffs = new Dictionary<BuffType, Coroutine>();

    public void ApplyEffect(GameObject user)
    {
        Player player = user.GetComponent<Player>();

        if (player == null)
        {
            Debug.LogWarning($"Không tìm thấy Player trên {user.name}");
            return;
        }

        foreach (var effect in effects)
        {
            switch (effect.effectType)
            {
                case ConsumableEffectType.Heal:
                    ApplyHeal(player, effect.effectValue);
                    break;

                case ConsumableEffectType.RestoreMana:
                    ApplyMana(player, effect.effectValue);
                    break;

                case ConsumableEffectType.Buff:
                    ApplyBuff(player, effect.buffTarget, effect.effectValue, effect.duration);
                    break;
            }
        }
    }

    private void ApplyHeal(Player player, float amount)
    {
        player.playerStat.health = Mathf.Min(player.playerStat.health + amount, player.playerStat.maxHealth);
        Debug.Log($"[Item] {itemName} hồi {amount} máu cho {player.name}");
    }

    private void ApplyMana(Player player, float amount)
    {
        player.playerStat.mana = Mathf.Min(player.playerStat.mana + amount, player.playerStat.maxMana);
        Debug.Log($"[Item] {itemName} hồi {amount} mana cho {player.name}");
    }

    private void ApplyBuff(Player player, BuffType buffType, float amount, float duration)
    {
        // Nếu đã có buff cùng loại → dừng coroutine cũ và reset hiệu ứng
        if (activeBuffs.ContainsKey(buffType) && activeBuffs[buffType] != null)
        {
            player.StopCoroutine(activeBuffs[buffType]);
            ResetBuff(player, buffType);
        }

        // Bắt đầu buff mới
        Coroutine newBuff = player.StartCoroutine(BuffCoroutine(player, buffType, amount, duration));
        activeBuffs[buffType] = newBuff;
    }

    private IEnumerator BuffCoroutine(Player player, BuffType buffType, float amount, float duration)
    {
        Debug.Log($"[Item] {itemName} buff {buffType} +{amount} cho {player.name} trong {duration} giây");

        // Áp dụng buff
        switch (buffType)
        {
            case BuffType.MoveSpeed:
                player.playerStat.buffSpeed += amount;
                break;
            case BuffType.Defense:
                player.playerStat.buffDefense += amount;
                break;
            case BuffType.Damage:
                player.playerStat.buffDamage += amount;
                break;
        }

        yield return new WaitForSeconds(duration);

        // Hết buff → reset
        ResetBuff(player, buffType);

        Debug.Log($"Buff {buffType} từ {itemName} đã hết hiệu lực");
        activeBuffs[buffType] = null;
    }

    private void ResetBuff(Player player, BuffType buffType)
    {
        // Trả lại về giá trị gốc
        switch (buffType)
        {
            case BuffType.MoveSpeed:
                player.playerStat.buffSpeed = 0f;
                break;
            case BuffType.Defense:
                player.playerStat.buffDefense = 0f;
                break;
            case BuffType.Damage:
                player.playerStat.buffDamage = 0f;
                break;
        }
    }
}
