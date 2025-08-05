using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float timeExist = 5f;
    public float damage;
    public bool taggetPlayer;

    public GameObject damagePopupPrefab;
    [SerializeField] private Canvas canvas;

    void Start()
    {
        // Tìm Canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas trong scene!");
            return;
        }

        // Tải prefab từ thư mục Resources
        damagePopupPrefab = Resources.Load<GameObject>("UI/DamagePopup");
        if (damagePopupPrefab == null)
        {
            Debug.LogError("Không tìm thấy prefab DamagePopupUIPrefab trong Resources/UI/");
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeExist -= Time.deltaTime;
        if(timeExist < 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && taggetPlayer)
        {
            Player p = collision.GetComponent<Player>();
            p.health -= damage;
            Destroy(gameObject);
        }
        if (collision.CompareTag("Enemy") && !taggetPlayer)
        {
            Orc enemy = collision.GetComponent<Orc>();
            if (enemy != null)
            {
                enemy.health -= damage;
                ShowDamagePopup((int)damage, transform.position);
            }
            OrcRogue enemy2 = collision.GetComponent<OrcRogue>();
            if (enemy2 != null)
            {
                enemy2.health -= damage;
                ShowDamagePopup((int)damage, transform.position);
            }
            Boss boss = collision.GetComponentInParent<Boss>();
            if (boss != null)
            {
                boss.health -= damage;
                ShowDamagePopup((int)damage,transform.position);
            }
            Destroy(gameObject);
        }
    }
    public void ShowDamagePopup(int amount, Vector3 worldPosition)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition + new Vector3(0, 1f, 0));
        GameObject popup = Instantiate(damagePopupPrefab, screenPosition, Quaternion.identity, canvas.transform);


        popup.GetComponent<DamagePopup>().Setup(amount);
    }

}
