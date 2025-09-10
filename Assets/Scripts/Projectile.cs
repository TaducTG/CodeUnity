using System.Collections.Generic;
using UnityEngine;
using static AbilityEffect;

public class Projectile : MonoBehaviour
{
    public float timeExist = 5f;
    public float damage;
    public bool taggetPlayer;
    [System.Serializable]
    public struct EffectChance
    {
        public AbilityEffect effect;
        [Range(0, 100)] public float chancePercent;
    }

    public List<EffectChance> onHitEffects = new List<EffectChance>();
    // 👇 Thêm danh sách effect
    private List<AbilityEffect> onHitWeaponEffect = new List<AbilityEffect>();

    public GameObject damagePopupPrefab;
    [SerializeField] private Canvas canvas;

    void Start()
    {
        // Tìm Canvas
        canvas = Object.FindFirstObjectByType<Canvas>();
        //canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas trong scene!");
            return;
        }

        // Tải prefab từ thư mục Resources
        damagePopupPrefab = Resources.Load<GameObject>("UI/DamagePopup");
        if (damagePopupPrefab == null)
        {
            Debug.LogError("Không tìm thấy prefab DamagePopupUIPrefab trong Resources/UI/");
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeExist -= Time.deltaTime;
        if(timeExist < 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool hitSomething = false;

        if (collision.CompareTag("Player") && taggetPlayer)
        {
            Player p = collision.GetComponent<Player>();
            if (p != null)
            {
                float finalDmg = Mathf.Max(damage - p.playerStat.defense, 0);
                p.playerStat.health -= finalDmg;
                hitSomething = true;
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Enemy") && !taggetPlayer)
        {
            Stat enemyStat = collision.GetComponent<Stat>();
            if (enemyStat != null)
            {
                float finalDmg = Mathf.Max(damage - enemyStat.defense, 1);
                enemyStat.health -= finalDmg;
                ShowDamagePopup((int)finalDmg, transform.position);
                hitSomething = true;
            }

            Animal animal = collision.GetComponent<Animal>();
            if (animal != null)
            {
                animal.TakeDamage();
                hitSomething = true;
            }

            Slime enemy3 = collision.GetComponent<Slime>();
            if (enemy3 != null)
            {
                enemy3.isHurt = 0.2f;
                hitSomething = true;
            }
            Destroy(gameObject);
        }

        // 👇 Sau khi gây damage xong, apply thêm effect
        if (hitSomething)
        {
            foreach (var effect in onHitWeaponEffect)
            {
                if (effect.targetType == EffectTargetType.OnHit)
                {
                    effect.Apply(collision.gameObject, null);
                    // ⚠️ chỗ config có thể truyền null hoặc ref nếu bạn muốn giữ damage/attackSpeed
                }
            }
            foreach (var wrapper in onHitEffects)
            {
                if (wrapper.effect.targetType == EffectTargetType.OnHit)
                {
                    float roll = Random.Range(0f, 100f);
                    if (roll <= wrapper.chancePercent)
                    {
                        wrapper.effect.Apply(collision.gameObject, null);
                    }
                }
            }
        }
    }
    public void SetEffects(List<AbilityEffect> effects)
    {
        onHitWeaponEffect = effects;
    }
    public void ShowDamagePopup(int amount, Vector3 worldPosition)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition + new Vector3(0, 1f, 0));
        GameObject popup = Instantiate(damagePopupPrefab, screenPosition, Quaternion.identity, canvas.transform);


        popup.GetComponent<DamagePopup>().Setup(amount);
    }

}
