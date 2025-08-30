using System.Collections;
using UnityEngine;

public class Stat : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseHealth;
    public float baseMana;
    public float baseDamage;
    public float baseDefense;
    public float baseSpeed;
    public float atackSpeed;

    [Header("Equipment Bonus")]
    public float equipHealth;
    public float equipMana;
    public float equipDamage;
    public float equipDefense;
    public float equipSpeed;
    public float equipPercentSpeed;

    [Header("Buff Bonus")]
    public float buffHealth;
    public float buffMana;
    public float buffDamage;
    public float buffPercentDamage;
    public float buffDefense;
    public float buffSpeed;
    public float buffPercentSpeed;

    [Header("Debuff")]
    public bool stun;
    public bool burn;
    public float burnTime;
    public bool posion;

    public float defenseReducePercent;
    public float speedReducePercent;

    [Header("Final Stats (calculated)")]
    public float maxHealth;
    public float health;
    public float maxMana;
    public float mana;
    public float damage;
    public float defense;
    public float speed;

    private void Start()
    {
        RecalculateStats();

        health = maxHealth;
        mana = maxMana;
    }

    private void Update()
    {
        // Nếu bạn muốn chỉ tính lại khi có thay đổi thì bỏ Update đi,
        // và gọi RecalculateStats() mỗi khi thay đổi stat
        RecalculateStats();

        if (burn)
        {
            burn = false; // Ngăn gọi nhiều lần
            StartCoroutine(Burn());
        }
    }

    public void RecalculateStats()
    {
        maxHealth = baseHealth + equipHealth + buffHealth;
        maxMana = baseMana + equipMana + buffMana;

        // Damage có cộng thêm % từ buff
        damage = (baseDamage + equipDamage + buffDamage) * (1 + buffPercentDamage);

        // Defense
        defense = baseDefense + equipDefense + buffDefense;

        // Speed có cả % từ equip và buff
        speed = (baseSpeed + equipSpeed + buffSpeed) * (1 + equipPercentSpeed + buffPercentSpeed);
    }

    IEnumerator Burn()
    {
        float elapsed = 0f;
        while (elapsed < burnTime)
        {
            health -= maxHealth * 0.01f; // Trừ 1% máu tối đa
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        burn = false; // Kết thúc hiệu ứng
    }
}
