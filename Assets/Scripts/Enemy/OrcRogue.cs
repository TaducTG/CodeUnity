using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AIPath))]
public class OrcRogue : MonoBehaviour
{
    [Header("Sprite")]
    [Tooltip("Để trống thì script sẽ tự tìm SpriteRenderer ở GameObject hiện tại hoặc con.")]
    public SpriteRenderer spriteRenderer;

    [Header("Flip Settings")]
    [Tooltip("Flip bằng SpriteRenderer.flipX (khuyến nghị cho 2D).")]
    public bool useSpriteRendererFlip = true;

    [Tooltip("Flip bằng localScale.x (dùng khi không dùng SpriteRenderer hoặc là prefab đặc thù).")]
    public bool useLocalScaleFlip = false;

    [Tooltip("Đảo ngược hướng flip nếu sprite gốc quay ngược.")]
    public bool invertFlip = false;

    [Tooltip("Ngưỡng vận tốc để bắt đầu flip, tránh rung khi gần như đứng yên.")]
    [Min(0f)] public float flipSpeedDeadzone = 0.05f;

    [Header("Velocity Source")]
    [Tooltip("Dùng desiredVelocity (hướng mong muốn) thay vì velocity (vận tốc thực) để flip sớm hơn.")]
    public bool useDesiredVelocity = false;

    private AIPath aiPath;
    Animator animator;
    [Header("Stat")]

    public Stat enemyStat;

    public float atkSpeed;


    private float atkSpeedTime;

    public GameObject atkProjectile;    // Prefab đạn
    public Transform firePoint;         // Vị trí bắn (nếu để trống sẽ dùng vị trí Enemy)
    public float projectileSpeed = 8f;  // Tốc độ đạn
    public float projectileLifetime = 4f; // Thời gian tự hủy đạn
    public float range;


    [Header("Drop items")]
    private bool die = false;
    public class DropData
    {
        public GameObject itemPrefab; // Prefab item sẽ rơi
        public int quantity = 1;      // Số lượng item spawn
        [Range(0f, 100f)]
        public float dropChance = 100f; // % tỉ lệ spawn
    }

    public List<DropData> dropTable = new List<DropData>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyStat = GetComponent<Stat>();
        animator = GetComponent<Animator>();
        aiPath = GetComponent<AIPath>();
        atkSpeedTime = 0;
        enemyStat.health = enemyStat.maxHealth;
        if (spriteRenderer == null)
        {
            // Thử lấy ở chính GameObject
            spriteRenderer = GetComponent<SpriteRenderer>();
            // Nếu chưa có, thử tìm ở con
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Gợi ý: tắt rotation tự động để không xoay sprite 2D.
        if (aiPath != null)
            aiPath.enableRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        atkSpeedTime -= Time.deltaTime;
        if (aiPath == null)
        {

            return;
        }

        // Chỉ xử lý khi AI path đang bật và được phép di chuyển
        if (!aiPath.enabled || !aiPath.canMove || die)
        {
            animator.SetBool("Run", false);
            return;
        }

        // Lấy vận tốc dùng để xác định hướng
        Vector3 v = useDesiredVelocity ? (Vector3)aiPath.desiredVelocity : (Vector3)aiPath.velocity;
        animator.SetBool("Run", true);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        float sqrDist = (player.transform.position - transform.position).sqrMagnitude;

        if (sqrDist < range * range && atkSpeedTime <= 0f)
        {
            atkSpeedTime = atkSpeed; // reset hồi chiêu
            // --- Tạo projectile hướng về Player ---
            Vector2 origin = firePoint ? (Vector2)firePoint.position : (Vector2)transform.position;
            Vector2 dir = ((Vector2)player.transform.position - origin).normalized;

            // Tạo đạn
            GameObject proj = Instantiate(atkProjectile, origin, Quaternion.identity);

            // Xoay đạn theo hướng bắn (trục Z cho 2D)
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            proj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Đẩy đạn đi: ưu tiên dùng Rigidbody2D nếu có
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * projectileSpeed;
            }

            Projectile pj = proj.GetComponent<Projectile>();
            pj.damage = enemyStat.damage;
            pj.taggetPlayer = true; 
            // Tự hủy sau N giây để tránh rác
            Destroy(proj, projectileLifetime);
        }

        // Nếu gần như đứng yên thì không đổi hướng để tránh nhấp nháy
        if (v.sqrMagnitude < (flipSpeedDeadzone * flipSpeedDeadzone)) return;

        // Quy ước: x < 0 => nhìn/trái, x > 0 => nhìn/phải (bạn có thể đảo bằng invertFlip)
        float x = v.x;
        bool flipToLeft = x < 0f;
        if (invertFlip) flipToLeft = !flipToLeft;

        // Thực hiện flip theo chế độ bạn chọn
        if (useSpriteRendererFlip && spriteRenderer != null)
        {
            // Với sprite mặc định nhìn sang phải: flipX = true khi đi sang trái
            spriteRenderer.flipX = flipToLeft;
        }

        if (useLocalScaleFlip)
        {
            Vector3 ls = transform.localScale;
            float desiredSign = flipToLeft ? -1f : 1f;
            // Nếu invertFlip bật thì đảo dấu
            if (invertFlip) desiredSign = -desiredSign;

            // Chỉ cập nhật khi cần (tránh set lại liên tục)
            float absX = Mathf.Abs(ls.x);
            if (Mathf.Sign(ls.x) != Mathf.Sign(desiredSign))
            {
                ls.x = absX * desiredSign;
                transform.localScale = ls;
            }
        }

        if (enemyStat.health <= 0 && !die)
        {
            animator.SetBool("Death", true);
            die = true;
            StartCoroutine(Die());
        }
    }

    public void TakeDamage(float damage)
    {
        animator.SetBool("Hurt", true);
        enemyStat.health -= Mathf.Max(1, damage - enemyStat.defense);
        if (enemyStat.health <= 0 && !die)
        {
            animator.SetBool("Death", true);
            die = true;
            StartCoroutine(Die());
        }
    }
    IEnumerator Die()
    {
        yield return new WaitForSeconds(0.6f);
        enemyStat.health = enemyStat.maxHealth;
        die = false;
        DropAllItems();
        EnemyPoolManager.Instance.ReturnToPool(gameObject);
    }

    private void DropAllItems()
    {
        foreach (DropData data in dropTable)
        {
            // Quay số random xem có rơi hay không
            float roll = Random.Range(0f, 100f);
            if (roll <= data.dropChance && data.itemPrefab != null)
            {
                // Spawn số lượng item mong muốn
                for (int j = 0; j < data.quantity; j++)
                {
                    GameObject drop = Instantiate(data.itemPrefab, transform.position, Quaternion.identity);

                    // Bật tất cả script trên item (nếu có tắt sẵn)
                    foreach (MonoBehaviour script in drop.GetComponents<MonoBehaviour>())
                    {
                        script.enabled = true;
                    }

                    // Bật Collider2D nếu có
                    var col = drop.GetComponent<Collider2D>();
                    if (col) col.enabled = true;

                    // Nếu item này cũng có DropItem => đánh dấu là item rơi và scale nhỏ lại
                    var dropScript = drop.GetComponent<DropItem>();
                    if (dropScript != null)
                    {
                        dropScript.dropItem = true;
                        drop.transform.localScale *= 0.5f;
                    }

                    // Nếu có PickUpItem => bỏ trạng thái block
                    var pickup = drop.GetComponent<PickUpItem>();
                    if (pickup) pickup.block = false;

                    // Thêm lực ngẫu nhiên để item bắn ra
                    var rb = drop.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 force = new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f));
                        rb.AddForce(force, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
}
