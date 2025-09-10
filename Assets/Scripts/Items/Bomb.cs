using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    public float explodeDelay = 2f;        // Thời gian chờ phát nổ
    public float explosionRadius = 3f;     // Bán kính nổ
    public float maxDamage = 100f;         // Sát thương tối đa
    public Sprite explosionSprite;         // Ảnh vụ nổ

    private bool exploded = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (transform.parent != null && transform.parent.CompareTag("Player"))
        {
            return;
        }

        Invoke(nameof(Explode), explodeDelay);
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        // Đổi sang ảnh nổ
        if (explosionSprite != null && sr != null)
            sr.sprite = explosionSprite;

        // Tìm tất cả collider trong phạm vi nổ
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            // Thử lấy script Health (bạn đổi bằng script stat của nhân vật/ quái)
            Stat target = hit.GetComponent<Stat>();
            if (target != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                float dmg = Mathf.Lerp(maxDamage, 0, dist / explosionRadius); // giảm dần theo khoảng cách
                target.health -= dmg;
            }
            if(hit.tag == "MapObject")
            {
                DropItem dr = hit.GetComponent<DropItem>();
                if(dr != null)
                {
                    float dist = Vector2.Distance(transform.position, hit.transform.position);
                    float dmg = Mathf.Lerp(maxDamage, 0, dist / explosionRadius); // giảm dần theo khoảng cách
                    dr.health -= dmg/(dr.tier+0.5f);
                }
            }
        }

        // Xóa object sau khi nổ (chờ 0.5s để thấy hiệu ứng)
        Destroy(gameObject, 0.2f);
    }

    void OnDrawGizmosSelected()
    {
        // Vẽ phạm vi nổ trong editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
