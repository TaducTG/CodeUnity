using UnityEngine;

public class Boss_Skill : MonoBehaviour
{
    public GameObject hit_range;
    public float idle = 2f;
    public float atk = 1.6f;
    Rigidbody2D rb;
    Animator animator;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        idle -= Time.deltaTime;
        if(idle < 0)
        {
            animator.SetBool("Attack", true);
            hit_range.SetActive(true);
            atk -= Time.deltaTime;

            if (atk < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
