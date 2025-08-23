using System.Collections;
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
    public float maxHealth;
    public float health;
    public float damage;
    public float hurtTime = 0.2f;
    public float isHurt = 0;

    private Animator animator;
    private Transform player;
    private Vector2 targetPos;
    private bool isJumping = false;
    private bool die = false;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        health = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(BehaviorLoop());
    }
    private void Update()
    {
        isHurt -= Time.deltaTime;
        if(isHurt > 0)
        {
            animator.SetBool("Hurt", true);
        }
        else
        {
            animator.SetBool("Hurt", false);
        }

        if(health <= 0)
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
        transform.position = destination;
        animator.SetBool("Run", false);
        isJumping = false;
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(1f);
        health = maxHealth;
        die = false;
        EnemyPoolManager.Instance.ReturnToPool(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, speed * jumpDuration);
    }
}
