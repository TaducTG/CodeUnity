using System.Collections.Generic;
using UnityEngine;
using static AbilityEffect;
using static Weapon;
[CreateAssetMenu(menuName = "Abilities/FireProjectile")]
public class FireProjectitleAbility : WeaponAbility
{
    public override void Activate(GameObject user, GameObject mainHand, AbilityConfig config)
    {
        // chạy effects kèm theo
        foreach (var effect in config.effects)
        {
            if (effect.targetType == EffectTargetType.Self)
            {
                effect.Apply(user, config);
            }
        }
        // tìm runner để chạy coroutine
        Player player = user.GetComponent<Player>();
        if (player == null) return;

        Inventory inventory = player.inventory;
        if (inventory == null) return;

        if (config.projectilePrefab == null)
        {
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
            Shoot(user.transform.position, GetMouseWorldPosition(), projectile.prefabs, config.projectileSpeed, config.damage + projectile.damage,config.effects);
        }
        else
        {
            ProjectileItem projectile =(ProjectileItem)config.projectilePrefab.GetComponent<PickUpItem>().itemData;

            Shoot(user.transform.position, GetMouseWorldPosition(), config.projectilePrefab, config.projectileSpeed, config.damage + projectile.damage, config.effects);
        }
        
    }
    private void Shoot(Vector3 origin, Vector3 target, GameObject prefab, float speed, float finalDamage, List<AbilityEffect> effects)
    {
        Vector3 dir = (target - origin).normalized;

        GameObject projectile = GameObject.Instantiate(prefab, origin, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = dir * speed;

        projectile.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.damage = finalDamage;

            List<AbilityEffect> onHitEffects = new List<AbilityEffect>();
            foreach (var e in effects)
            {
                if (e.targetType == EffectTargetType.OnHit)
                {
                    onHitEffects.Add(e);
                }
            }
            projScript.SetEffects(onHitEffects);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // Đảm bảo không bị z=0 khi dùng perspective camera
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
