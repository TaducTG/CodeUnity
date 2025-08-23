using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance;

    [Header("UI References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;

    public Vector2 off = new Vector2(0,0);

    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        rectTransform = tooltipPanel.GetComponent<RectTransform>();
        Hide();
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            // Tooltip đi theo chuột
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform,
                Input.mousePosition,
                null,
                out pos);
            rectTransform.anchoredPosition = pos + off; // lệch chút để không đè chuột
        }
    }

    public void Show(InventoryItem item)
    {
        if (item == null || item.itemData == null) return;

        tooltipPanel.SetActive(true);
        itemNameText.text = item.itemData.itemName;
        iconImage.sprite = item.itemData.icon;

        // Ghép các dòng description
        descriptionText.text = string.Join("\n", item.itemData.description);
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }
}
