using UnityEngine;

public class MeleeATKRange : MonoBehaviour
{
    public float damage;

    private void Start()
    {
        Orc orc = transform.parent.GetComponent<Orc>();
        if(orc != null)
        {
            damage = orc.damage;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player p = collision.GetComponent<Player>();
            p.health -= damage;
        }
    }
}
