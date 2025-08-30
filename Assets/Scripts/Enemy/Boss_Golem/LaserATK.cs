using UnityEngine;

public class LaserATK : MonoBehaviour
{
    public float waitTimer = 1.5f;
    public float existTimer = 1f;
    public float hitDuration = 0.1f;
    public float hitTimer = 0.1f;

    public float damage;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hitTimer -= Time.deltaTime;
        waitTimer -= Time.deltaTime;
        if(waitTimer < 0)
        {
            existTimer -= Time.deltaTime;
            if(existTimer < 0)
            {
                Destroy(gameObject);
            }
            BoxCollider2D box = gameObject.GetComponent<BoxCollider2D>();
            box.enabled = true;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(hitTimer < 0 && collision.CompareTag("Player"))
        {
            hitTimer = hitDuration;

            Player p = collision.GetComponent<Player>();
            p.playerStat.health -= Mathf.Max(damage - p.playerStat.defense, 2);
        }
    }
}
