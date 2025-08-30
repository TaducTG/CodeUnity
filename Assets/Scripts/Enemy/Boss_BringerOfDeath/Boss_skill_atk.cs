using UnityEngine;

public class Boss_skill_atk : MonoBehaviour
{
    public float damage;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player p = collision.GetComponent<Player>();

                p.playerStat.health -= damage;

        }
    }
}
