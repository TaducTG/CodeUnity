using System.Collections;
using System.Collections.Generic;
using JetBrains.Rider.Unity.Editor;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEditor.Progress;

public class Player : MonoBehaviour
{
    public float moveSpeed;
    public float atkSpeedTime;
    private float localScaleX;
    public float angle = 45f;
    Rigidbody2D rb;

    public Inventory inventory;
    public Items mainHand;
    public GameObject MainHand;
    public GameObject[] panelsToBlockAttack;

    public float maxHealth;
    public float health;
    public float maxMana;
    public float mana;


    void Start()
    {
        health = maxHealth;
        mana = maxMana;
        rb = GetComponent<Rigidbody2D>();
        localScaleX = rb.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        rb.linearVelocity = new Vector2(moveX * moveSpeed, moveY * moveSpeed);

        if(moveX < 0)
        {
            rb.transform.localScale = new Vector3(-localScaleX, rb.transform.localScale.y);
        }
        if (moveX > 0)
        {
            rb.transform.localScale = new Vector3(localScaleX, rb.transform.localScale.y);
        }



        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeItem(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeItem(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) ChangeItem(5);
        if (Input.GetKeyDown(KeyCode.Alpha6)) ChangeItem(6);
        if (Input.GetKeyDown(KeyCode.Alpha7)) ChangeItem(7);

        if (Input.GetMouseButtonDown(1)) // Chuột phải để đặt block
        {
            TryPlaceBlock();
        }

        if (mainHand != null)
        {
            atkSpeedTime -= Time.deltaTime;

            // ❌ Không tấn công nếu panel UI đang mở
            bool isUIPanelOpen = false;
            foreach (GameObject panel in panelsToBlockAttack)
            {
                if (panel.activeSelf)
                {
                    isUIPanelOpen = true;
                    break;
                }
            }

            if (!isUIPanelOpen && Input.GetMouseButtonDown(0) && atkSpeedTime < 0)
            {
                if(mainHand is Tools)
                {
                    Tools hold = (Tools)mainHand;
                    atkSpeedTime = hold.speed;
                    if(MainHand.tag == "Range_Weapon")
                    {
                        List<InventoryItem> inventoryPlayer = new List<InventoryItem>();
                        inventoryPlayer = inventory.GetComponent<Inventory>().items;

                        ProjectileItem projectile = ScriptableObject.CreateInstance<ProjectileItem>();
                        bool haveProjectile = false;
                        for (int i = 0; i < inventoryPlayer.Count; i++)
                        {
                            InventoryItem item = inventoryPlayer[i];
                            if (item == null) continue;

                            if (item.itemData is ProjectileItem proj && item.quantity > 0)
                            {
                                item.quantity -= 1;
                                projectile = proj;
                                haveProjectile = true;

                                if (item.quantity <= 0)
                                {
                                    // Xóa item khỏi slot (để trống slot)
                                    inventoryPlayer[i] = null;
                                    // Nếu có UI: cập nhật lại
                                    // uiManager?.RefreshInventoryUI(); // hoặc SyncSlotsToInventory()
                                }
                                break;
                            }
                        }
                        if (haveProjectile)
                        {
                            Shoot(projectile.prefabs, projectile.speed,hold.damage + projectile.damage);
                        }
                        else
                        {
                            Debug.Log("Out of arrow");
                        }
                        
                    }
                    else
                    {
                        if(MainHand.tag == "Spear")
                        {
                            angle = 180;
                        }
                        else if(MainHand.tag == "Weapon")
                        {
                            angle = 90;
                        }
                        else
                        {
                            angle = 45;
                        }
                            StartCoroutine(SwingTool(hold.speed / 2f, hold.speed / 2f));
                    }

                }
            }
        }
    }
    IEnumerator SwingTool(float duration,float returnDuration)
    {
        Transform hitbox = MainHand.transform.Find("hitbox");
        hitbox.gameObject.SetActive(true);


        Quaternion originalRotation = MainHand.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(MainHand.transform.localEulerAngles + new Vector3(0f, 0f, -angle));

        // Xoay xuống
        float time = 0f;
        while (time < duration)
        {
            MainHand.transform.localRotation = Quaternion.Lerp(originalRotation, targetRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        MainHand.transform.localRotation = targetRotation;

        // Quay về vị trí ban đầu
        time = 0f;
        while (time < returnDuration)
        {
            MainHand.transform.localRotation = Quaternion.Lerp(targetRotation, originalRotation, time / returnDuration);
            time += Time.deltaTime;
            yield return null;
        }
        hitbox.gameObject.SetActive(false);
        MainHand.transform.localRotation = originalRotation;
    }

    public void Shoot(GameObject bulletPrefab,float bulletSpeed,float damage)
    {
        // Lấy vị trí chuột trong thế giới
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // Đảm bảo không có sai lệch trục Z

        // Tính hướng từ nòng súng đến chuột
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        // Tạo đạn
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // Cho đạn bay theo hướng chuột
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * bulletSpeed;

        // (Tuỳ chọn) Xoay viên đạn theo hướng bay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        Projectile bl = bullet.GetComponent<Projectile>();
        bl.damage = damage;
    }
    public void ChangeItem(int index)
    {
        List<InventoryItem> inventoryPlayer = new List<InventoryItem>();
        inventoryPlayer = inventory.GetComponent<Inventory>().items;
        // Xóa vật phẩm hiện tại trên tay
        foreach (Transform child in gameObject.transform)
        {
            if (child.CompareTag("Tool") || child.CompareTag("Item") || child.CompareTag("Placeable")
                || child.CompareTag("Weapon") || child.CompareTag("Range_Weapon") || child.CompareTag("Spear"))
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        if(inventoryPlayer[index - 1] == null || inventoryPlayer[index - 1].itemData == null)
        {
            return;
        }

        GameObject tool = Instantiate(inventoryPlayer[index - 1].itemData.prefabs, transform.position, Quaternion.identity);

        var rb = tool.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;          // ngừng tham gia physics
            rb.linearVelocity = Vector2.zero;    // dừng chuyển động
            rb.angularVelocity = 0f;
        }

        // Đặt đúng vị trí so với Player
        if (transform.localScale.x < 0)
        {
            tool.transform.localScale = new Vector2(-tool.transform.localScale.x, tool.transform.localScale.y);
            tool.transform.position += new Vector3(-0.7f, 0.5f, -1f);
        }
        else
        {
            tool.transform.position += new Vector3(0.7f, 0.5f, -1f);
        }
        tool.transform.parent = transform;

        //Nếu là Tools thì có thể swing
        if (inventoryPlayer[index - 1].itemData is Tools)
        {
           
            mainHand = inventoryPlayer[index - 1].itemData;
            MainHand = tool;
        }

        //Nếu là Placeable thì có thể đặt 
        if(inventoryPlayer[index - 1].itemData is Placeable)
        {
            mainHand = inventoryPlayer[index - 1].itemData;
        }
    }

    void TryPlaceBlock()
    {

        if (mainHand == null || !(mainHand is Placeable)) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // Cố định Z-axis

        // Snap theo lưới
        float gridSize = 1f;
        float snappedX = Mathf.Floor(mouseWorldPos.x) + 0.5f;
        float snappedY = Mathf.Floor(mouseWorldPos.y) + 0.5f;
        Vector2 placementPos = new Vector2(snappedX, snappedY);

        // Kiểm tra xem có thể đặt được không
        if (!CanPlaceAt(placementPos)) return;

        GameObject prefab = mainHand.prefabs; // Lưu ý bạn đang lưu prefab trong itemData
        if (prefab == null)
        {
            Debug.LogWarning("Placeable item has no prefab assigned!");
            return;
        }

        GameObject spawned = Instantiate(mainHand.prefabs, placementPos, Quaternion.identity);


        spawned.GetComponent<DropItem>().dropItem = false;
        spawned.GetComponent<PickUpItem>().block = true;

        // Trừ 1 item khỏi inventory nếu cần
        inventory.RemoveItem(mainHand, 1);

        var connectScript = spawned.GetComponent<ConnectableBlock>();
        if (connectScript != null)
        {
            connectScript.UpdateConnection(mainHand);         // Cập nhật chính nó
            connectScript.UpdateNeighbors(placementPos,mainHand);
        }

        foreach (Transform child in gameObject.transform)
        {
            if (child.CompareTag("Tool") || child.CompareTag("Item") || child.CompareTag("Placeable"))  // hoặc bạn có thể đặt tag riêng cho weapon
            {
                GameObject.Destroy(child.gameObject); // hoặc DestroyImmediate trong editor
            }
        }
        mainHand = null;
    }

    bool CanPlaceAt(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position + Vector2.up*0.5f, 0.5f);

        foreach (var hit in hits)
        {
            if (!hit.isTrigger)
            {
                return false; // Có collider thật sự (không phải trigger) -> không được đặt
            }
        }

        return true; // Không có collider nào "vật lý cản trở" -> được đặt
    }

}
