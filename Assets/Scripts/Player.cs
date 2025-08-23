using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Player : MonoBehaviour
{
    [Header("Setting")]
    public float atkSpeedTime;
    private float localScaleX;
    public float angle = 45f;
    Rigidbody2D rb;
    Animator animator;
    public GameObject hitboxPrefab;
    public Inventory inventory;
    public Items mainHand;
    public GameObject MainHand;
    public GameObject[] panelsToBlockAttack;
    public MapGenerator map;

    [Header("Stat")]
    public float maxHealth;
    public float baseHealth;
    public float health;
    public float maxMana;
    public float baseMana;
    public float mana;
    public float def;
    public float baseDef;
    public float moveSpeed;
    public float baseSpeed;

    public float maxDistance = 2f; // khoảng cách tối đa

    void Start()
    {
        baseSpeed = moveSpeed;
        baseDef = def;

        health = maxHealth;
        mana = maxMana;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        localScaleX = rb.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        rb.linearVelocity = new Vector2(moveX * moveSpeed, moveY * moveSpeed);
        if(moveX != 0 || moveY != 0)
        {
            animator.SetBool("Run", true);
        }
        else
        {
            animator.SetBool("Run", false);
        }
        if (moveX < 0)
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
            if (mainHand is Tools farmTool && MainHand.tag == "FarmTools")
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                farmTool.UseFarmTool(mouseWorldPos,map);
            }
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
                    StartCoroutine(SwingTool(hold.speed / 2f, hold.speed / 2f));
                }
                if (mainHand is WeaponItem weapon)
                {
                    atkSpeedTime = weapon.attackSpeed;
                    weapon.ATK(gameObject, MainHand);

                    if (MainHand.tag == "Weapon")
                    {
                        Transform hitbox = MainHand.transform.Find("hitbox");
                        if (hitbox != null)
                        {
                            WeaponHitbox hb = hitbox.GetComponent<WeaponHitbox>();
                            WeaponItem wp = (WeaponItem)mainHand;
                            hb.damage = wp.damage;
                        }
                        
                    }
                }
                if(mainHand is ConsumableItem consumable)
                {
                    consumable.ApplyEffect(gameObject);
                    inventory.RemoveItem(mainHand, 1);

                    if(inventory.GetTotalItem(mainHand) == 0)
                    {
                        foreach (Transform child in gameObject.transform)
                        {
                            if (child.CompareTag("Tool") || child.CompareTag("Item") || child.CompareTag("Placeable"))  // hoặc bạn có thể đặt tag riêng cho weapon
                            {
                                GameObject.Destroy(child.gameObject); // hoặc DestroyImmediate trong editor
                            }
                        }
                        mainHand = null;
                    }
                }
            }
        }
    }
    IEnumerator SwingTool(float duration, float returnDuration)
    {
        // Lấy vị trí chuột
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        GameObject tempHitbox = null;

        // Nếu trong phạm vi cho phép → tạo hitbox
        if (Vector2.Distance(transform.position, mouseWorldPos) <= maxDistance)
        {
            tempHitbox = Instantiate(hitboxPrefab, mouseWorldPos, Quaternion.identity);
            Tools t = (Tools)MainHand.GetComponent<PickUpItem>().itemData;
            tempHitbox.GetComponent<Hitbox>().damage = t.damage;
            tempHitbox.GetComponent<Hitbox>().tier = t.tier;
        }

        // Lưu rotation ban đầu và target
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
        MainHand.transform.localRotation = originalRotation;

        // Xóa hitbox khi kết thúc swing
        if (tempHitbox != null)
            Destroy(tempHitbox);
    }
    public void ChangeItem(int index)
    {
        List<InventoryItem> inventoryPlayer = new List<InventoryItem>();
        inventoryPlayer = inventory.GetComponent<Inventory>().items;
        // Xóa vật phẩm hiện tại trên tay
        foreach (Transform child in gameObject.transform)
        {
            if (child.CompareTag("Tool") || child.CompareTag("Item") || child.CompareTag("Placeable")
                || child.CompareTag("Weapon") || child.CompareTag("Range_Weapon") || child.CompareTag("Spear")
                || child.CompareTag("FarmTools"))
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
        if (inventoryPlayer[index - 1].itemData is Tools
            || inventoryPlayer[index - 1].itemData is WeaponItem
            || inventoryPlayer[index - 1].itemData is Placeable
            || inventoryPlayer[index - 1].itemData is ConsumableItem
            || inventoryPlayer[index - 1].itemData is Crops)
        {
           
            mainHand = inventoryPlayer[index - 1].itemData;
            MainHand = tool;
        }
        else
        {
            mainHand = null;
        }
    }

    void TryPlaceBlock()
    {

        if (mainHand == null || (!(mainHand is Placeable) && !(mainHand is Crops))) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // Cố định Z-axis

        // Snap theo lưới
        float snappedX = Mathf.Floor(mouseWorldPos.x) + 0.5f;
        float snappedY = Mathf.Floor(mouseWorldPos.y) + 0.5f;
        Vector2 placementPos = new Vector2(snappedX, snappedY);

        // Kiểm tra xem có thể đặt được không
        if (!CanPlaceAt(placementPos)) return;

        GameObject spawned = new GameObject();
        if (!(mainHand is Crops))
        {
            GameObject prefab = mainHand.prefabs; // Lưu ý bạn đang lưu prefab trong itemData
            if (prefab == null)
            {
                Debug.LogWarning("Placeable item has no prefab assigned!");
                return;
            }

            spawned = Instantiate(mainHand.prefabs, placementPos, Quaternion.identity);
            spawned.GetComponent<DropItem>().dropItem = false;
            spawned.GetComponent<PickUpItem>().block = true;
        }
        else
        {
            Crops c = (Crops)mainHand;
            GameObject prefab = c.placePrefabs;
            if (prefab == null)
            {
                Debug.LogWarning("Placeable item has no prefab assigned!");
                return;
            }
            spawned = Instantiate(prefab, placementPos, Quaternion.identity);
        }
        if (!spawned.GetComponent<TopDownDepthSort>())
        {
            Vector3 pos = spawned.transform.position;
            pos.z = 9f; // đặt z cố định
            spawned.transform.position = pos;
        }




        // Trừ 1 item khỏi inventory nếu cần
        inventory.RemoveItem(mainHand, 1);

        var connectScript = spawned.GetComponent<ConnectableBlock>();
        if (connectScript != null)
        {
            connectScript.UpdateConnection(mainHand);         // Cập nhật chính nó
            connectScript.UpdateNeighbors(placementPos,mainHand);
        }
        if (MainHand.layer == LayerMask.NameToLayer("HardObject"))
        {
            Collider2D[] softHits = Physics2D.OverlapCircleAll(placementPos, 0.4f, LayerMask.GetMask("SoftObject"));
            foreach (var soft in softHits)
            {
                Destroy(soft.gameObject); // Phá object mềm bị đè
            }
        }
        if (inventory.GetTotalItem(mainHand) == 0)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (child.CompareTag("Tool") || child.CompareTag("Item") || child.CompareTag("Placeable"))  // hoặc bạn có thể đặt tag riêng cho weapon
                {
                    GameObject.Destroy(child.gameObject); // hoặc DestroyImmediate trong editor
                }
            }
            mainHand = null;
        }
    }

    bool CanPlaceAt(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, 0.4f);

        bool hasFarmLand = false;

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            // ❌ Không đặt nếu có cùng loại block
            var pickUp = hit.GetComponent<PickUpItem>();
            if (pickUp != null && pickUp.itemData == mainHand)
            {
                return false;
            }

            // Check nếu có farm_land
            if (hit.CompareTag("Farm_land"))
            {
                hasFarmLand = true;
            }

            // Nếu collider không phải trigger và không phải farmland → cản đặt
            if (!hit.isTrigger && !hit.CompareTag("Farm_land"))
            {
                return false;
            }
        }

        // Nếu mainHand là Crops thì chỉ đặt được trên farmland
        if (mainHand is Crops && !hasFarmLand)
        {
            return false;
        }

        return true;
    }

}
