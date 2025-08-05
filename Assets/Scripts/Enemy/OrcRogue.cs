using UnityEngine;
using Pathfinding;

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

    public float damage;
    public float maxHealth;
    public float health;
    public float atkSpeed;
    private float atkSpeedTime;

    public GameObject player;
    public GameObject atkProjectile;    // Prefab đạn
    public Transform firePoint;         // Vị trí bắn (nếu để trống sẽ dùng vị trí Enemy)
    public float projectileSpeed = 8f;  // Tốc độ đạn
    public float projectileLifetime = 4f; // Thời gian tự hủy đạn
    public float range;


    private bool die = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        aiPath = GetComponent<AIPath>();
        atkSpeedTime = 0;
        health = maxHealth;
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
            pj.damage = damage;
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

        if (health <= 0)
        {
            animator.SetBool("Death", true);
        }
    }
}
