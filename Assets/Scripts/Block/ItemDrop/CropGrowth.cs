using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropGrowth : MonoBehaviour
{
    [Header("Growth Settings")]
    public Sprite[] growthStages;   // Các sprite theo từng giai đoạn
    public float timePerStage = 10f; // Thời gian mỗi giai đoạn
    private int currentStage = 0;
    private float growthTimer;

    private SpriteRenderer spriteRenderer;

    [Header("Stats")]
    public float health = 10f;
    public float maxHealth = 10f;
    public float tier = 0; // độ cứng, giống DropItem

    [Header("Drop Config")]
    public bool dropItem = false; // tránh bị phá khi mới spawn ra từ hạt giống
    public List<DropItem.DropData> dropTable = new List<DropItem.DropData>();

    private bool isDead = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = growthStages[0];
        growthTimer = timePerStage;
        health = maxHealth;
    }

    void Update()
    {
        // Xử lý phát triển qua thời gian
        growthTimer -= Time.deltaTime;
        if (growthTimer <= 0f && currentStage < growthStages.Length - 1)
        {
            currentStage++;
            spriteRenderer.sprite = growthStages[currentStage];
            growthTimer = timePerStage;
        }

        // Nếu máu <= 0 thì rơi đồ
        if (health <= 0 && !isDead)
        {
            isDead = true;
            Harvest();
            Destroy(gameObject);
        }
    }

    public void Harvest()
    {
        if (currentStage < growthStages.Length - 1)
        {
            // Chưa chín -> rơi lại chính crop (giả sử slot đầu tiên của dropTable là hạt)
            if (dropTable.Count > 0)
                SpawnItem(dropTable[0]);
        }
        else
        {
            // Đã chín -> rơi ra item khác
            DropAllItems();

            // Đồng thời phá Farmland bên dưới
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Farmland"));
            if (hit != null) Destroy(hit.gameObject);
        }
    }

    private void DropAllItems()
    {
        foreach (DropItem.DropData data in dropTable)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= data.dropChance && data.itemPrefab != null)
            {
                for (int j = 0; j < data.quantity; j++)
                    SpawnItem(data);
            }
        }
    }

    private void SpawnItem(DropItem.DropData data)
    {
        GameObject drop = Instantiate(data.itemPrefab, transform.position, Quaternion.identity);

        // Bật tất cả script
        foreach (MonoBehaviour script in drop.GetComponents<MonoBehaviour>())
            script.enabled = true;

        var col = drop.GetComponent<Collider2D>();
        if (col) col.enabled = true;

        var dropScript = drop.GetComponent<DropItem>();
        if (dropScript != null)
        {
            dropScript.dropItem = true;
            drop.transform.localScale *= 0.5f;
        }

        var pickup = drop.GetComponent<PickUpItem>();
        if (pickup) pickup.block = false;

        var rb = drop.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 force = new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f));
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(1);
        if (collision.CompareTag("Tool_hitbox") && !dropItem)
        {
            Debug.Log(1);
            Hitbox h = collision.GetComponent<Hitbox>();
            if (h != null && h.tier >= tier)
            {
                health -= h.damage;
                Shake(0.1f, 0.05f);
            }
        }
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
