using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stat : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseHealthRegenrate;
    public float baseManaRegenrate;

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

    public float equipPercentHealth;
    public float equipPercentMana;
    public float equipPercentDamage;
    public float equipPercentDefense;
    public float equipPercentSpeed;

    public float equipHealthRegenerate;
    public float equipManaRegenerate;

    [Header("Buff Bonus")]
    public float buffHealth;
    public float buffMana;
    public float buffDamage;
    public float buffPercentDamage;
    public float buffDefense;
    public float buffSpeed;
    public float buffPercentSpeed;

    public bool rage;
    private Coroutine rageCoroutine;
    [Header("Debuff")]
    // Stun
    public bool stun;
    private Coroutine stunCoroutine;
    private float stunRemainingTime;
    // Burn
    private Coroutine burnCoroutine;
    private float burnRemainingTime;
    // Poison
    private Coroutine poisonCoroutine;
    private float poisonRemainingTime;
    // Defense Reduce
    private Coroutine defenseReduceCoroutine;
    private float defenseReduceRemainingTime;
    // Freeze
    private Coroutine freezeCoroutine;
    private float freezeRemainingTime;
    // Speed Reduce
    private Coroutine speedReduceCoroutine;
    private float speedReduceRemainingTime;
    // Reduce Stat

    private List<float> speedReduceList = new List<float>();
    public float speedReducePercent;
    private List<float> defenseReduceList = new List<float>();
    private float defenseReducePercent;

    [Header("Resist")]
    public bool resistBurn;
    public bool resistPoison;
    public bool resistStun;
    public bool resistDefenseReduce;
    public bool resistFreeze;
    public bool resistSpeedReduce;

    [Header("Final Stats (calculated)")]
    public float maxHealth;
    public float health;
    public float maxMana;
    public float mana;

    public float healthRegenerate;
    public float manaRegenerate;
    public float damage;
    public float defense;
    public float speed;

    private void Start()
    {
        RecalculateStats();

        health = maxHealth;
        mana = maxMana;

        StartCoroutine(Regenerate());
    }

    private void Update()
    {
        // Nếu bạn muốn chỉ tính lại khi có thay đổi thì bỏ Update đi,
        // và gọi RecalculateStats() mỗi khi thay đổi stat
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        maxHealth = (baseHealth + equipHealth + buffHealth) * (1 + equipPercentHealth);
        maxMana = (baseMana + equipMana + buffMana) * (1 + equipPercentMana);

        healthRegenerate = baseHealthRegenrate + equipHealthRegenerate;

        manaRegenerate = baseManaRegenrate + equipManaRegenerate;

        // Damage có cộng thêm % từ buff
        damage = (baseDamage + equipDamage + buffDamage) * (1 + buffPercentDamage + equipPercentDamage);

        // Defense
        defense = (baseDefense + equipDefense + buffDefense) * (1 - defenseReducePercent + equipPercentDefense);

        // Speed có cả % từ equip và buff
        speed = (baseSpeed + equipSpeed + buffSpeed) * (1 + equipPercentSpeed + buffPercentSpeed - speedReducePercent);
    }
    IEnumerator Regenerate()
    {
        while (true)
        {
            health = Mathf.Min(maxHealth,health + healthRegenerate);
            mana = Mathf.Min(maxMana,mana + manaRegenerate);
            yield return new WaitForSeconds(5f);
        }
    }
    public void ApplyStun(float duration)
    {
        stunRemainingTime = duration;
        stun = true;
        if (stunCoroutine == null)
        {
            stunCoroutine = StartCoroutine(Stun());
        }
    }
    IEnumerator Stun()
    {
        while(stunRemainingTime > 0)
        {
            stunRemainingTime -= Time.deltaTime;
            yield return null;
        }
        stun = false;
        stunCoroutine = null;
    }
    public void ApplyBurn(float duration)
    {
        burnRemainingTime = duration;

        if (burnCoroutine == null) // chỉ start nếu chưa có coroutine
        {
            burnCoroutine = StartCoroutine(Burn());
        }
    }
    IEnumerator Burn()
    {
        while (burnRemainingTime > 0)
        {
            health -= maxHealth * 0.01f; // trừ 1% máu tối đa
            yield return new WaitForSeconds(1f);
            burnRemainingTime -= 1f;
        }

        burnCoroutine = null; // reset để lần sau có thể start lại
    }
    public void ApplyPoison(float duration)
    {
        poisonRemainingTime = duration;

        if (poisonCoroutine == null)
        {
            poisonCoroutine = StartCoroutine(Poison());
        }
    }
    IEnumerator Poison()
    {
        while (poisonRemainingTime > 0)
        {
            // ✅ giảm máu theo % máu hiện tại
            health -= health * 0.02f;

            yield return new WaitForSeconds(1f); // tick mỗi giây
            poisonRemainingTime -= 1f;
        }

        poisonCoroutine = null; // reset để lần sau có thể Apply lại
    }
    public void ApplyDefenseReduce(float duration,float percent) {
        defenseReduceRemainingTime = duration;
        defenseReduceList.Add(percent);
        defenseReducePercent = defenseReduceList.Max();
        if (defenseReduceCoroutine == null)
        {
            defenseReduceCoroutine = StartCoroutine(DefenseReduce(percent));
        }
    }
    IEnumerator DefenseReduce(float percent)
    {
        while (defenseReduceRemainingTime > 0)
        {
            defenseReduceRemainingTime -= Time.deltaTime;
            yield return null; // chạy mỗi frame
        }
        defenseReduceList.Remove(percent);
        defenseReducePercent = defenseReduceList.Count > 0 ? defenseReduceList.Max() : 0f;
        defenseReduceCoroutine = null; // reset để lần sau có thể start lại
    }
    public void ApplyFreeze(float duration,float percent)
    {
        freezeRemainingTime = duration;
        speedReduceList.Add(percent);
        speedReducePercent = speedReduceList.Max();
        if (freezeCoroutine == null)
        {
            freezeCoroutine = StartCoroutine(Freeze(percent));
        }
    }
    IEnumerator Freeze(float percent)
    {
        while(freezeRemainingTime > 0)
        {
            health -= maxHealth * 0.02f;
            freezeRemainingTime -= 1f;
            yield return new WaitForSeconds(1f);
        }
        speedReduceList.Remove(percent);
        speedReducePercent = speedReduceList.Count > 0 ? speedReduceList.Max() : 0f;
        freezeCoroutine = null;
    }

    public void ApplySpeedReduce(float duration,float percent)
    {
        speedReduceRemainingTime = duration;
        speedReduceList.Add(percent);
        speedReducePercent = speedReduceList.Max();
        if(speedReduceCoroutine == null)
        {
            speedReduceCoroutine = StartCoroutine(SpeedReduce(percent));
        }
    }
    IEnumerator SpeedReduce(float percent)
    {
        while (speedReduceRemainingTime > 0)
        {
            speedReduceRemainingTime -= 1f;
            yield return new WaitForSeconds(1f);
        }
        speedReduceList.Remove(percent);
        speedReducePercent = speedReduceList.Count > 0 ? speedReduceList.Max() : 0f;
        speedReduceCoroutine = null;
    }
    public void ApplyRage()
    {
        if (!rage)
        {
            rage = true;
            rageCoroutine = StartCoroutine(Rage());
        }
        else
        {
            rage = false;
        }
    }
    IEnumerator Rage()
    {
        float amount = 0;
        while (rage && health > 0.2*maxHealth)
        {
            health -= maxHealth * 0.01f;
            buffPercentDamage += 0.01f;
            amount += 0.01f;
            yield return new WaitForSeconds(1f); // tick mỗi giây
        }
        buffPercentDamage -= amount;
    }

}
