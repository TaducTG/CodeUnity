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
    private bool isDamageBuffed = false;
    private float damageBuffAmount = 0f;
    private int damageOverride = 0;

    private bool isSpeedBuffed = false;
    private float speedBuffAmount = 0f;
    private int speedOverride = 0;

    private bool isDefBuffed = false;
    private float defBuffAmount = 0f;
    private int defOverride = 0;

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
        player.health = Mathf.Min(player.health + amount, player.maxHealth);
        Debug.Log($"[Item] {itemName} hồi {amount} máu cho {player.name}");
    }

    private void ApplyMana(Player player, float amount)
    {
        player.mana = Mathf.Min(player.mana + amount, player.maxMana);
        Debug.Log($"[Item] {itemName} hồi {amount} mana cho {player.name}");
    }

    private void ApplyBuff(Player player, BuffType buffType, float amount, float duration)
    {
        player.StartCoroutine(BuffCoroutine(player, buffType, amount, duration));
    }

    

    private IEnumerator BuffCoroutine(Player player, BuffType buffType, float amount, float duration)
    {
        Debug.Log($"[Item] {itemName} buff {buffType} +{amount} cho {player.name} trong {duration} giây");

        // Lưu giá trị ban đầu
        switch (buffType)
        {
            
            case BuffType.MoveSpeed:

                // Xử lý cho vấn đề dùng nhiều potion cùng lúc sẽ không stack mà lấy effect cuối cùng được apply
                if (!isSpeedBuffed)
                {
                    isSpeedBuffed = true;
                    speedBuffAmount = amount;
                    player.moveSpeed += amount;
                }
                else
                {
                    player.moveSpeed -= speedBuffAmount;
                    player.moveSpeed += amount;
                    speedBuffAmount = amount;
                    speedOverride += 1;
                }
                    break;
            case BuffType.Defense:
                if (!isDefBuffed)
                {
                    isDefBuffed = true;
                    defBuffAmount = amount;
                    player.def += amount;
                }
                else
                {
                    player.def -= defBuffAmount;
                    player.def += amount;
                    defBuffAmount = amount;
                    defOverride += 1;
                }
                    break;
        }

        yield return new WaitForSeconds(duration);

        // Hết buff → trả lại giá trị
        switch (buffType)
        {
            
            case BuffType.MoveSpeed:
                if(speedOverride > 0)
                {
                    speedOverride -= 1;
                    break;
                }

                isSpeedBuffed = false;
                player.moveSpeed = player.baseSpeed;
                break;
            case BuffType.Defense:
                if(defOverride > 0)
                {
                    defOverride -= 1;
                    break;
                }
                player.def = player.baseDef;
                break;
        }

        Debug.Log($"Buff {buffType} từ {itemName} đã hết hiệu lực");
    }
}
