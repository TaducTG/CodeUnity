using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static DropItem;

public class Animal : MonoBehaviour
{
    [Header("General Settings")]
    public Stat enemtStat;

    public float walkSpeed = 2f;              // tốc độ đi bộ bình thường
    public float runSpeed = 5f;               // tốc độ khi bỏ chạy
    public float idleTime = 2f;               // thời gian đứng chờ
    public float walkTime = 3f;               // thời gian đi bộ
    public float changeRunDirTime = 1.5f;     // đổi hướng chạy sau mỗi khoảng thời gian


    [Header("Flee Settings")]
    public bool isScared = false;             // bật khi bị đánh
    public float fleeDuration = 5f;           // chạy bao lâu thì dừng

    public float hitTime;
    public float hit = 0.4f;
    public float death = 0.8f;
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDir;
    private bool isMoving = false;
    private bool die = false;

    [Header("Drop Items")]
    public bool dropItems;
    [System.Serializable]
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
        hitTime = 0;
        enemtStat = GetComponent<Stat>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemtStat.health = enemtStat.maxHealth;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(BehaviorLoop());
    }

    IEnumerator BehaviorLoop()
    {
        while (true)
        {
            if (!isScared)
            {
                // -------- Idle --------
                animator.SetBool("Run", false);
                isMoving = false;
                yield return new WaitForSeconds(idleTime);

                // -------- Walk --------
                animator.SetBool("Run", true);
                isMoving = true;
                moveDir = Random.insideUnitCircle.normalized;
                yield return new WaitForSeconds(walkTime);
            }
            else
            {
                animator.SetBool("Run", true);
                // -------- Flee --------
                float fleeTimer = 0f;
                while (fleeTimer < fleeDuration)
                {
                    isMoving = true;
                    Vector2 dirAway = ((Vector2)transform.position - (Vector2)player.position).normalized;
                    moveDir = dirAway;

                    // chạy 1 đoạn rồi đổi hướng chút cho tự nhiên
                    yield return new WaitForSeconds(changeRunDirTime);
                    fleeTimer += changeRunDirTime;

                    // thêm một chút random lệch hướng
                    moveDir = (dirAway + Random.insideUnitCircle * 0.3f).normalized;
                }

                // Sau khi hết thời gian bỏ chạy → quay lại loop thường
                isScared = false;
                animator.SetBool("Run", false);
            }
        }
    }

    void Update()
    {
        if (isMoving)
        {
            float currentSpeed = isScared ? runSpeed : walkSpeed;
            rb.MovePosition(rb.position + moveDir * currentSpeed * Time.deltaTime);

            // ---- Flip hướng theo moveDir ----
            if (moveDir.x != 0)
            {
                spriteRenderer.flipX = moveDir.x < 0;
            }
        }

        hitTime -= Time.deltaTime;
        if (hitTime < 0)
        {
            animator.SetBool("Hurt", false);
        }
    }

    // Hàm gây sát thương cho Animal
    public void TakeDamage()
    {
        animator.SetBool("Hurt", true);
        hitTime = hit;
        if (enemtStat.health <= 0 && !die)
        {
            animator.SetBool("Death", true);
            die = true;
            StartCoroutine(Die());
        }
        else
        {
            isScared = true; // chuyển sang trạng thái bỏ chạy
        }
    }

    IEnumerator Die()
    {
        // TODO: Thêm animation chết hoặc rơi item
        yield return new WaitForSeconds(death);
        enemtStat.health = enemtStat.maxHealth;
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
