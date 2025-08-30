using UnityEngine;

public class RoomControl : MonoBehaviour
{
    public GameObject boss;
    Boss b;
    Vector2 locate;
    void Start()
    {
        locate = boss.transform.localPosition;
        b = boss.GetComponent<Boss>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            boss.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            boss.transform.localPosition = locate;
            boss.SetActive(false);
            b.ResetStat();
            
        }
    }
}
