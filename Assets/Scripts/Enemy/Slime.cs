using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Slime : MonoBehaviour
{
    [Header("Player Detection")]
    public float detectionRange = 5f;          // Khoảng phát hiện Player

    [Header("Jump Settings")]
    public float jumpCooldown = 1.5f;          // Thời gian nghỉ giữa các lần nhảy
    public float jumpDuration = 0.5f;          // Thời gian thực hiện cú nhảy
    public float speed = 5f;                   // Vận tốc nhảy (distance/time)
    public float jumpHeight = 0.5f;            // Độ cao tối đa (parabol giả)

    [Header("Stat")]
    public Stat enemyStat;

    public float hurtTime = 0.2f;
    public float isHurt = 0;

    private Animator animator;
    private Transform player;
    private Vector2 targetPos;
    private bool isJumping = false;

    public GameObject hitboxPrefabs;
    public float hitboxTime = 0.05f;
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
    void Awake()
    {
        enemyStat = GetComponent<Stat>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        enemyStat.health = enemyStat.maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(BehaviorLoop());
    }
    private void Update()
    {
        hitboxTime -= Time.deltaTime;
        if (hitboxTime < 0)
        {
            hitboxPrefabs.SetActive(false);
        }
        isHurt -= Time.deltaTime;
        if(isHurt > 0)
        {
            animator.SetBool("Hurt", true);
        }
        else
        {
            animator.SetBool("Hurt", false);
        }

        if(enemyStat.health <= 0)
        {
            die = true;
            animator.SetBool("Death", true);
            StartCoroutine(Die());
        }
    }
    IEnumerator BehaviorLoop()
    {
        if (die)
        {
            yield break;
        }
        while (true)
        {
            
            if (!isJumping)
            {
                float distToPlayer = Vector2.Distance(transform.position, player.position);
                float maxJumpDistance = speed * jumpDuration;

                if (distToPlayer <= detectionRange)
                {
                    if (distToPlayer <= maxJumpDistance)
                        targetPos = player.position;
                    else
                    {
                        Vector2 dir = (player.position - transform.position).normalized;
                        targetPos = (Vector2)transform.position + dir * maxJumpDistance;
                    }
                }
                else
                {
                    Vector2 randomDir = Random.insideUnitCircle.normalized;
                    targetPos = (Vector2)transform.position + randomDir * maxJumpDistance;
                }
                yield return StartCoroutine(JumpTo(targetPos));
                yield return new WaitForSeconds(jumpCooldown);
            }
            yield return null;
        }
    }

    IEnumerator JumpTo(Vector2 destination)
    {
        isJumping = true;
        Vector2 start = transform.position;
        float elapsed = 0f;

        // Kích hoạt animation Jump
        if (animator != null)
            animator.SetBool("Run", true);

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            // Di chuyển theo tuyến tính XY
            transform.position = Vector2.Lerp(start, destination, t);

            // Thêm offset parabol (fake chiều cao nhảy)
            float heightOffset = 4 * jumpHeight * t * (1 - t);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + heightOffset, transform.localPosition.z);

            yield return null;
        }

        // Reset vị trí về đúng điểm đến (bỏ offset)
        hitboxPrefabs.SetActive(true);

        Transform hitbox = transform.Find("hitbox");
        if (hitbox != null)
        {
            WeaponHitbox hb = hitbox.GetComponent<WeaponHitbox>();

            hb.damage = enemyStat.damage;
        }

        hitboxTime = 0.05f;
        transform.position = destination;
        animator.SetBool("Run", false);
        isJumping = false;
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(1f);
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, speed * jumpDuration);
    }
}
