using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public float health;
    public float maxHealth;

    public float moveSpeed;
    public float damage;

    public float idle = 10f;
    public float idleTime;

    public float normalATK = 12f;
    public float normalATKTime;
    private bool atk;
    public float atkTime = 0.9f;
    public GameObject atk_range;

    public float skillATK_1 = 20f;
    public float skillATK_1Time;
    public float skill_duration;
    public float skill_durationTime;
    public float skill_reactive;
    public float skill_reactiveTime;
    public GameObject skill_projectile;
    private bool skill;
    private bool die;

    public float rage;
    public float rageTime;
    private bool raged;
    private Vector2 loc;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer sr;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = maxHealth;

        idleTime = idle;
        normalATKTime = normalATK;
        skillATK_1Time = skillATK_1;
        skill_durationTime = skill_duration;
        skill_reactiveTime = skill_reactive;
        loc = (Vector2)transform.position;
        rageTime = rage;
    }

    // Update is called once per frame
    void Update()
    {
        if(health / maxHealth < 0.5f)
        {
            rageTime -= Time.deltaTime;
            if (!raged)
            {
                skillATK_1Time = 0;
                skill_duration = 2 * skill_duration;
                skill_reactive = skill_reactive / 1.5f;
                normalATK = normalATK / 2;
                raged = true;
            }
        }
        if(rageTime < 0)
        {
            skill_duration /= 2;
            skill_reactive *= 1.5f;
            normalATK *= 2;
            rageTime = 1000 * rage;
        }
        if (die)
        {
            return;
        }
        // Đánh thường
        normalATKTime -= Time.deltaTime;
        if(atk == true)
        {
            if(normalATKTime < normalATK - 0.9)
            {
                atk = false;
                animator.SetBool("Attack", false);
                atk_range.SetActive(false);
            }
            return;
        }
        idleTime -= Time.deltaTime;
        
        // Skill - call
        skillATK_1Time -= Time.deltaTime;
        if (skillATK_1Time < 0)
        {
            skillATK_1Time = skillATK_1;
            SkillATK_1();
        }
        if (skillATK_1 - skillATK_1Time > 0.9f)
        {
            animator.SetBool("Skill", false);
        }

        // TÌm player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 direction = player.transform.position - transform.position;

            // Flip theo trục X (nếu đang nhìn trái và player bên phải, thì quay)
            if (direction.x < 0)
            {
                Vector3 newScale = transform.localScale;
                newScale.x = 1;
                transform.localScale = newScale;
            }
            else if (direction.x > 0)
            {
                Vector3 newScale = transform.localScale;
                newScale.x = -1;
                transform.localScale = newScale;
            }
        }

        // di chuyển
        if(idleTime < 0)
        {
            animator.SetBool("Walk", true);
            // Tính hướng tới player
            Vector3 direction = player.transform.position - transform.position;
            direction.z = 0f; // tránh lỗi trong 2D
            direction.Normalize();

            // Di chuyển về phía player
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        // idle
        if(idleTime < -2 * idle)
        {
            animator.SetBool("Walk", false);
            idleTime = idle;
        }

        // Skill - active
        if (skill)
        {
            skill_durationTime -= Time.deltaTime;
            if(skill_durationTime < 0)
            {
                skill_durationTime = skill_duration;
                skill = false;
            }
            skill_reactiveTime -= Time.deltaTime;
            if(skill_reactiveTime < 0)
            {
                skill_reactiveTime = skill_reactive;
                float x = gameObject.transform.position.x + Random.Range(-60f, -20f);
                float y = gameObject.transform.position.y + Random.Range(-20f, 20f);
                GameObject projectitle = Instantiate(skill_projectile, new Vector3(x, y,-1) + (Vector3)loc, Quaternion.identity);
            }
            
        }

        //Die
        if(health <= 0)
        {
            animator.SetBool("Death", true);
            die = true;
            StartCoroutine(Die());
        }

    }
    IEnumerator Die()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
    IEnumerator ATK()
    {
        yield return new WaitForSeconds(0.4f);
        atk_range.SetActive(true);
    }
    public void SkillATK_1()
    {
        animator.SetBool("Skill", true);
        skill= true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(normalATKTime < 0)
            {
                normalATKTime = normalATK;
                atk = true;
                animator.SetBool("Idle", false);
                animator.SetBool("Attack", true);

                StartCoroutine(ATK());
                
            }
            
        }
    }
}
