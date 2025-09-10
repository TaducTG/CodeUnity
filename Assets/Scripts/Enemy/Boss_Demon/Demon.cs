using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demon : MonoBehaviour
{
    private Animator animator;
    private Transform player;
    private Rigidbody2D rb;

    private bool isMoving;
    private Vector2 moveDir;

    [Header("Movement")]
    public float idleTime = 3f;
    public float walkTime = 4f;
    public float walkSpeed = 2f;

    [Header("Stat")]
    public Stat enemyStat;

    private float baseDamage;

    private float baseDefend;

    public float rageTime = 20f;
    public float rageStatBuff = 1.5f;
    private bool rage;
    private float rageTimer;
    [Header("Melee Settings")]
    public float meleeRange = 2f;
    public float meleeCooldown = 2f;
    public GameObject meleeHitbox;
    private float meleeTimer = 0f;

    [Header("Skill Cooldowns")]
    public float shootCooldown = 5f;
    public float shootTime = 3f;
    public GameObject projectilePrefab;   // prefab đạn
    public float projectileSpeed = 8f;    // tốc độ bay của đạn
    public Transform shootPoint;          // vị trí bắn ra (empty object đặt trước mặt Golem)

    public float jumpCooldown = 16f;
    public float jumpDuration = 0.9f;          // Thời gian thực hiện cú nhảy
    public float speed = 5f;                   // Vận tốc nhảy (distance/time)
    public float jumpHeight = 0.5f;
    public GameObject jumpHitbox;
    private Vector2 targetPos;

    private float shootTimer = 0f;
    private float jumpTimer = 0f;

    private bool die = false;

    void Start()
    {
        enemyStat = GetComponent<Stat>();

        enemyStat.health = enemyStat.maxHealth;
        baseDamage = enemyStat.damage;
        baseDefend = enemyStat.defense;
        shootTimer = shootCooldown;

        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(BehaviorLoop()); // chạy Idle/Run
    }

    void Update()
    {
        if (die) return;

        shootTimer -= Time.deltaTime;
        jumpTimer -= Time.deltaTime;
        meleeTimer -= Time.deltaTime;

        // kiểm tra Melee
        if (player != null && meleeTimer <= 0f)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= meleeRange && !animator.GetBool("Skill_2") && !animator.GetBool("Skill_3"))
            {
                StartCoroutine(DoMelee());

                meleeTimer = meleeCooldown;
            }
        }
        // kiểm tra Skill theo cooldown
        if (shootTimer <= 0f)
        {
            StartCoroutine(DoShoot());
            shootTimer = shootCooldown;
        }

        if (jumpTimer <= 0f)
        {
            StartCoroutine(DoJump());
            jumpTimer = jumpCooldown;
        }

        if (enemyStat.health <= 0)
        {
            animator.SetBool("Death", true);
            die = true;
            StartCoroutine(Die());
        }
    }

    void FixedUpdate()
    {
        if (isMoving && !die)
        {
            rb.MovePosition(rb.position + moveDir * walkSpeed * Time.fixedDeltaTime);
        }
    }

    IEnumerator BehaviorLoop()
    {
        while (!die)
        {
            // Idle
            yield return StartCoroutine(DoIdle());

            // Run
            if (!animator.GetBool("Skill_2"))
            {
                yield return StartCoroutine(DoRun());
            }
            
        }
    }

    // ----------- Hành vi chính -----------

    IEnumerator DoIdle()
    {
        animator.SetBool("Run", false);
        isMoving = false;
        yield return new WaitForSeconds(idleTime);
    }

    IEnumerator DoRun()
    {
        animator.SetBool("Run", true);
        isMoving = true;

        float timer = 0f;
        while (timer < walkTime && !die)
        {
            if (player != null)
            {
                moveDir = (player.position - transform.position).normalized;

                // flip sprite
                if (moveDir.x != 0)
                {
                    float distToPlayer = Vector2.Distance(transform.position, player.position);

                    // Chỉ flip nếu còn cách xa player
                    if (distToPlayer > 0.5f)
                    {
                        Vector3 LocalScale = transform.localScale;
                        LocalScale.x = moveDir.x > 0 ? -Mathf.Abs(LocalScale.x) : Mathf.Abs(LocalScale.x);
                        transform.localScale = LocalScale;
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isMoving = false;
        animator.SetBool("Run", false);
    }

    IEnumerator DoShoot()
    {
        animator.SetBool("Skill_2", true);

        if (projectilePrefab != null && player != null)
        {
            // Spawn tại shootPoint (nếu có) hoặc tại vị trí Golem
            Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            // Hướng bay về phía Player
            Vector2 dir = (player.position - spawnPos).normalized;

            // --- XOAY SPRITE THEO HƯỚNG PLAYER ---
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            proj.transform.rotation = Quaternion.Euler(0, 0, angle);



            Vector3 scale = proj.transform.localScale;
            scale.x *= -1;   // hoặc scale.x, tùy sprite gốc
            proj.transform.localScale = scale;


            // Gán vận tốc
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * projectileSpeed;
            }
        }

        yield return new WaitForSeconds(shootTime); // thời gian animation
        animator.SetBool("Skill_2", false);
    }

    IEnumerator DoJump()
    {

        animator.SetBool("Skill_3", true);
        // Tìm Player
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        float maxJumpDistance = speed * jumpDuration;


        if (distToPlayer <= maxJumpDistance)
            targetPos = player.position;
        else
        {
            Vector2 dir = (player.position - transform.position).normalized;
            targetPos = (Vector2)transform.position + dir * maxJumpDistance;
        }
        //Delay
        yield return new WaitForSeconds(0.5f);
        // Thực hiện jump
        Vector2 start = transform.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            // Di chuyển theo tuyến tính XY
            transform.position = Vector2.Lerp(start, targetPos, t);

            // Thêm offset parabol (fake chiều cao nhảy)
            float heightOffset = 4 * jumpHeight * t * (1 - t);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + heightOffset, transform.localPosition.z);

            yield return null;
        }

        // Reset vị trí về đúng điểm đến (bỏ offset)
        jumpHitbox.SetActive(true);

        WeaponHitbox hb = jumpHitbox.GetComponent<WeaponHitbox>();

        hb.damage = enemyStat.damage;

        transform.position = targetPos;

        yield return new WaitForSeconds(0.1f);

        animator.SetBool("Skill_3", false);
        jumpHitbox.SetActive(false);
    }

    

    IEnumerator DoMelee()
    {
        animator.SetBool("Skill_1", true);
        yield return new WaitForSeconds(0.4f);

        meleeHitbox.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        animator.SetBool("Skill_1", false);
        meleeHitbox.SetActive(false);
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(1f);
        animator.SetBool("Death", false);
        gameObject.SetActive(false);
        ResetStat();
    }

    public void ResetStat()
    {
        enemyStat.health = enemyStat.maxHealth;
        enemyStat.defense = baseDefend;
        rage = false;
        die = false;
    }
}
