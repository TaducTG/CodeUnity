using UnityEngine;
using UnityEngine.UI;

public class DragItemUI : MonoBehaviour
{
    public static DragItemUI Instance;

    public Image icon;
    private RectTransform rectTransform;

    void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();


        Hide();
    }

    void Update()
    {
        if (gameObject.activeSelf)
        {
            rectTransform.position = Input.mousePosition;
        }
    }

    public void Show(Sprite sprite)
    {
        icon.sprite = sprite;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
