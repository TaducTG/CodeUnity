using UnityEngine;

public class Boss_normalATK : MonoBehaviour
{
    public float damage = 12f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player p = collision.GetComponent<Player>();

                p.health -= damage;

        }
    }
}
