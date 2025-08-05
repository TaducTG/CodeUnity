using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    public float health;
    public float maxHealth;
    public float tier;

    public bool dropItem;

    public GameObject[] dropItems;
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);

            for(int i=0; i < dropItems.Length; i++)
            {
                GameObject drop = Instantiate(dropItems[i], transform.position, Quaternion.identity);
                foreach (MonoBehaviour script in drop.GetComponents<MonoBehaviour>())
                {
                    script.enabled = true;
                }
                drop.GetComponent<Collider2D>().enabled = true;
                if (drop.GetComponent<DropItem>() != null)
                {
                    drop.GetComponent<DropItem>().dropItem = true;
                    drop.transform.localScale *= 0.5f;
                }
                drop.GetComponent<PickUpItem>().block = false;
                Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 force = new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f));
                    rb.AddForce(force, ForceMode2D.Impulse);
                }

            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Tool_hitbox") && !dropItem)
        {
            Transform parent = collision.transform.parent;
            Tools tool = (Tools)parent.GetComponent<PickUpItem>().itemData;
            if(tool.tier >= tier)
            {
                health -= tool.damage;

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
