using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    [Header("Stats")]
    public float health;
    public float maxHealth;
    public float tier; // Độ cứng của block/enemy

    [Header("SpawnObject")]
    public GameObject spawnPrefabs;

    [Header("Drop Config")]
    public bool dropItem; // Dùng để đánh dấu item rơi ra, tránh bị phá tiếp khi spawn

    [System.Serializable]
    public class DropData
    {
        public GameObject itemPrefab; // Prefab item sẽ rơi
        public int quantity = 1;      // Số lượng item spawn
        [Range(0f, 100f)]
        public float dropChance = 100f; // % tỉ lệ spawn
    }

    public List<DropData> dropTable = new List<DropData>();

    private bool isDead = false; // Chặn xử lý drop nhiều lần

    void Start()
    {
        health = maxHealth;
    }

    void Update()
    {
        // Nếu máu <= 0 và chưa xử lý chết => drop
        if (health <= 0 && !isDead)
        {
            isDead = true;
            DropAllItems();

            if (gameObject.GetComponent<ConnectableBlock>())
            {
                ConnectableBlock cn = gameObject.GetComponent<ConnectableBlock>();
                PickUpItem item = gameObject.GetComponent<PickUpItem>();
                cn.UpdateNeighbors(transform.position, item.itemData, true);
            }
            else
            {
                Destroy(gameObject);
            }
            if(spawnPrefabs != null)
            {
                Instantiate(spawnPrefabs, transform.position, Quaternion.identity);
            }
        }
    }

    private void DropAllItems()
    {
        foreach (DropData data in dropTable)
        {
            // Quay số random xem có rơi hay không
            float roll = Random.Range(0f, 100f);
            if (roll <= data.dropChance && data.itemPrefab != null)
            {
                // Spawn số lượng item mong muốn
                for (int j = 0; j < data.quantity; j++)
                {
                    GameObject drop = Instantiate(data.itemPrefab, transform.position, Quaternion.identity);

                    // Bật tất cả script trên item (nếu có tắt sẵn)
                    foreach (MonoBehaviour script in drop.GetComponents<MonoBehaviour>())
                    {
                        script.enabled = true;
                    }

                    // Bật Collider2D nếu có
                    var col = drop.GetComponent<Collider2D>();
                    if (col) col.enabled = true;

                    // Nếu item này cũng có DropItem => đánh dấu là item rơi và scale nhỏ lại
                    var dropScript = drop.GetComponent<DropItem>();
                    if (dropScript != null)
                    {
                        dropScript.dropItem = true;
                        drop.transform.localScale *= 0.5f;
                    }

                    // Nếu có PickUpItem => bỏ trạng thái block
                    var pickup = drop.GetComponent<PickUpItem>();
                    if (pickup) pickup.block = false;

                    // Thêm lực ngẫu nhiên để item bắn ra
                    var rb = drop.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 force = new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f));
                        rb.AddForce(force, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu va chạm với hitbox của tool và không phải là item vừa rơi
        if (collision.CompareTag("Tool_hitbox") && !dropItem)
        {
            Hitbox h = collision.GetComponent<Hitbox>();
            if (h != null && h.tier >= tier) // Chỉ bị phá nếu tool tier >= tier của block
            {
                health -= h.damage * (1 + (tier - h.tier)*0.2f);
                Shake(0.1f, 0.05f);
            }
        }
    }

    // Hàm tạo hiệu ứng rung khi bị đánh
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
