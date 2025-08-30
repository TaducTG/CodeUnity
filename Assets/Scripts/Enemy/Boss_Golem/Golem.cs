using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Golem : MonoBehaviour
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
    public GameObject projectilePrefab;   // prefab đạn
    public float projectileSpeed = 8f;    // tốc độ bay của đạn
    public Transform shootPoint;          // vị trí bắn ra (empty object đặt trước mặt Golem)

    public float shieldCooldown = 16f;
    public float defendIncrease = 10f;
    public float shieldTime = 4f;

    public float laserCooldown = 12f;
    public GameObject laserPrefab;
    public float laserDuration = 5f;
    public float laserDamage = 5f;


    private float shootTimer = 0f;
    private float shieldTimer = 0f;
    private float laserTimer = 0f;

    private bool die = false;

    void Start()
    {
        enemyStat = GetComponent<Stat>();

        enemyStat.health = enemyStat.maxHealth;
        baseDamage = enemyStat.damage;
        baseDefend = enemyStat.defense;
        shootTimer = shootCooldown;
        shieldTimer = shieldCooldown;
        laserTimer = laserCooldown;

        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(BehaviorLoop()); // chạy Idle/Run
    }

    void Update()
    {
        if (die) return;

        if (rageTimer > 0)
        {
            // giảm cooldown
            rageTimer -= Time.deltaTime;
            //buffstat
            meleeTimer -= Time.deltaTime * rageStatBuff;
            shootTimer -= Time.deltaTime * rageStatBuff;
            shieldTimer -= Time.deltaTime;
            laserTimer -= Time.deltaTime * rageStatBuff;
            enemyStat.damage = baseDamage * rageStatBuff;
            shieldTime -= Time.deltaTime;
        }
        else
        {
            enemyStat.damage = baseDamage;
            // giảm cooldown
            meleeTimer -= Time.deltaTime;
            shootTimer -= Time.deltaTime;
            shieldTimer -= Time.deltaTime;
            laserTimer -= Time.deltaTime;

            shieldTime -= Time.deltaTime;
        }
        if(enemyStat.health < enemyStat.maxHealth / 2 && !rage)
        {
            rage = true;
            rageTime = rageTimer;
        }
        // kiểm tra Melee
        if (player != null && meleeTimer <= 0f)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= meleeRange)
            {
                StartCoroutine(DoMelee());
                
                meleeTimer = meleeCooldown;
            }
        }
        if(shieldTime > 0)
        {
            enemyStat.defense = baseDefend + defendIncrease;
        }
        else
        {
            enemyStat.defense = baseDefend;
        }
        // kiểm tra Skill theo cooldown
        if (shootTimer <= 0f)
        {
            StartCoroutine(DoShoot());
            shootTimer = shootCooldown;
        }

        if (shieldTimer <= 0f)
        {
            StartCoroutine(DoShield());
            shieldTimer = shieldCooldown;
        }

        if (laserTimer <= 0f)
        {
            StartCoroutine(DoLaser());
            laserTimer = laserCooldown;
        }
        if(enemyStat.health <= 0)
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
            yield return StartCoroutine(DoRun());
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
                        LocalScale.x = moveDir.x > 0 ? Mathf.Abs(LocalScale.x) : -Mathf.Abs(LocalScale.x);
                        transform.localScale = LocalScale;
                    }

                    Vector3 localScale = transform.localScale;
                    localScale.x = moveDir.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                    transform.localScale = localScale;


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
        animator.SetBool("Shoot", true);

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

        yield return new WaitForSeconds(1f); // thời gian animation
        animator.SetBool("Shoot", false);
    }

    IEnumerator DoShield()
    {
        animator.SetBool("Shield", true);
        yield return new WaitForSeconds(1f);
        animator.SetBool("Shield", false);
    }

    IEnumerator DoLaser()
    {
        animator.SetBool("Laser", true);
        // tạo laser
        GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);

        // hướng về player
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        laser.transform.rotation = Quaternion.Euler(0, 0, angle);

        LaserATK ls = laser.GetComponent<LaserATK>();

        if(ls != null)
        {
            ls.damage = laserDamage;
        }

        // xóa sau khi bắn xong
        yield return new WaitForSeconds(1f);
        animator.SetBool("Laser", false);
    }

    IEnumerator DoMelee()
    {
        meleeHitbox.SetActive(true);
        animator.SetBool("Melee", true);
        yield return new WaitForSeconds(0.7f);
        animator.SetBool("Melee", false);
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
