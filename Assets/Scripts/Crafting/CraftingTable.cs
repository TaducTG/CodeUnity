using UnityEngine;

public class CraftingTable : MonoBehaviour
{
    public GameObject craftingUIPanel;
    public CraftingUIManager craftingUIManager;
    private bool playerInRange = false;
    void Awake()
    {
        // Tìm các thành phần UI theo đúng cấu trúc trong Canvas
        Transform canvas = GameObject.Find("Canvas")?.transform;

        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        // Tìm root Crafting trong Canvas
        Transform craftingRoot = canvas.Find("Crafting");

        if (craftingRoot == null)
        {
            Debug.LogError("Không tìm thấy GameObject 'Crafting' trong Canvas!");
            return;
        }
        craftingUIPanel = craftingRoot.Find("Crafting_recipe_panel")?.gameObject;
    }
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleCraftingUI();
        }
    }

    private void ToggleCraftingUI()
    {
        bool isActive = craftingUIPanel.activeSelf;
        craftingUIPanel.SetActive(!isActive);
        if (!isActive)
        {
            craftingUIManager.PopulateRecipeButtons();
        }
        Time.timeScale = isActive ? 1f : 0f; // Dừng thời gian khi mở UI (tùy chọn)
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = false;
    }
}
