using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        localScaleX = rb.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        rb.linearVelocity = new Vector2(moveX * moveSpeed, moveY * moveSpeed);
        rb.transform.position = new Vector3(rb.transform.position.x, rb.transform.position.y, rb.transform.position.y);
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

        if (mainHand != null)
        {
            atkSpeedTime -= Time.deltaTime;
            if (Input.GetMouseButtonDown(0) && atkSpeedTime < 0)
            {
                if(mainHand is Tools)
                {
                    Tools hold = (Tools)mainHand;
                    atkSpeedTime = hold.speed;
                    StartCoroutine(SwingTool(hold.speed / 2f, hold.speed / 2f));

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
    public void ChangeItem(int index)
    {
        List<InventoryItem> inventoryPlayer = new List<InventoryItem>();
        inventoryPlayer = inventory.GetComponent<Inventory>().items;
        if (inventoryPlayer[index - 1] != null && inventoryPlayer[index - 1].itemData != null && inventoryPlayer[index - 1].itemData is Tools)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (child.CompareTag("Tool"))  // hoặc bạn có thể đặt tag riêng cho weapon
                {
                    GameObject.Destroy(child.gameObject); // hoặc DestroyImmediate trong editor
                }
            }
            GameObject tool = Instantiate(inventoryPlayer[index - 1].itemData.prefabs, transform.position, Quaternion.identity);
            if (transform.localScale.x < 0)
            {
                tool.transform.localScale = new Vector2(-tool.transform.localScale.x, tool.transform.localScale.y);
                tool.transform.position += new Vector3(-0.7f, 0.5f, -1f);
            }
            else
            {
                tool.transform.position += new Vector3(0.7f, 0.5f, -1f);
            }
            mainHand = inventoryPlayer[index - 1].itemData;
            MainHand = tool;
            tool.transform.parent = transform;
        }
    }
}
