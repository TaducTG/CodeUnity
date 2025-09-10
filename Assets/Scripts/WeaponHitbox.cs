using System.Collections.Generic;
using UnityEngine;
using static AbilityEffect;

public class WeaponHitbox : MonoBehaviour
{
    public GameObject damagePopupPrefab;
    [SerializeField] private Canvas canvas;
    public float damage;
    public bool taggetPlayer;

    public List<AbilityEffect> onHitEffects = new List<AbilityEffect>();

    void Start()
    {
        // Tìm Canvas
        canvas = FindObjectOfType<Canvas>();
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
        }

        // 👇 Sau khi gây damage xong, apply thêm effect
        if (hitSomething)
        {
            foreach (var effect in onHitEffects)
            {
                if (effect.targetType == EffectTargetType.OnHit)
                {
                    effect.Apply(collision.gameObject, null);
                    // ⚠️ chỗ config có thể truyền null hoặc ref nếu bạn muốn giữ damage/attackSpeed
                }
            }
        }
    }
    public void SetEffects(List<AbilityEffect> effects)
    {
        onHitEffects = effects;
    }
    public void ShowDamagePopup(int amount, Vector3 worldPosition)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition + new Vector3(0, 1f, 0));
        GameObject popup = Instantiate(damagePopupPrefab, screenPosition, Quaternion.identity, canvas.transform);


        popup.GetComponent<DamagePopup>().Setup(amount);
    }
}
