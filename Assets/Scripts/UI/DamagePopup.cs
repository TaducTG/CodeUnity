using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI text; // ← nếu prefab nằm trong Canvas dạng UI
    public float moveSpeed = 1f;
    public float disappearTime = 0.3f;

    private float timer;
    private Vector3 moveDir;
    private Color textColor;

    public void Setup(int damageAmount)
    {
        text.text = damageAmount.ToString();
        textColor = text.color;
        moveDir = new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0f);
    }

    private void Update()
    {
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= disappearTime)
        {
            textColor.a -= 2f * Time.deltaTime;
            text.color = textColor;
            if (textColor.a <= 0f)
                Destroy(gameObject);
        }
    }
}
