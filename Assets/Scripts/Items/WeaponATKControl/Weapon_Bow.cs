using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bow", menuName = "Items/Weapon/Bow")]
public class Weapon_Bow : WeaponItem
{
    public GameObject defaultProjectile;
    public float projectileSpeed;

    public override void ATK(GameObject user, GameObject mainHand)
    {
        Player player = user.GetComponent<Player>();
        if (player == null) return;

        Inventory inventory = player.inventory;
        if (inventory == null) return;

        // Tìm đạn trong inventory
        ProjectileItem projectile = null;
        List<InventoryItem> items = inventory.items;

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item != null && item.itemData is ProjectileItem proj && item.quantity > 0)
            {
                projectile = proj;
                item.quantity--;
                if (item.quantity <= 0) items[i] = null;
                break;
            }
        }

        if (projectile == null)
        {
            Debug.Log("Out of arrows.");
            return;
        }

        //player.Shoot(projectile.prefabs, projectileSpeed, damage + projectile.damage);
        // Gọi Shoot riêng
        Shoot(user.transform.position, GetMouseWorldPosition(), projectile.prefabs, projectileSpeed, damage + projectile.damage);
    }

    private void Shoot(Vector3 origin, Vector3 target, GameObject prefab, float speed, float finalDamage)
    {
        Vector3 dir = (target - origin).normalized;

        GameObject projectile = GameObject.Instantiate(prefab, origin, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = dir * speed;

        projectile.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
            projScript.damage = finalDamage;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // Đảm bảo không bị z=0 khi dùng perspective camera
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}