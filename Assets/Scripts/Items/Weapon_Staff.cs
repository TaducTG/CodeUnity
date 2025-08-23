using UnityEngine;

[CreateAssetMenu(fileName = "New Staff", menuName = "Items/Weapon/Staff")]
public class Weapon_Staff : WeaponItem
{
    public GameObject defaultProjectile;
    public float projectileSpeed;

    public override void ATK(GameObject user, GameObject mainHand)
    {
        if (defaultProjectile == null)
        {
            Debug.LogWarning("Staff: defaultProjectile is not assigned.");
            return;
        }

        Vector3 origin = user.transform.position;
        Vector3 target = GetMouseWorldPosition();

        Shoot(origin, target, defaultProjectile, projectileSpeed, damage);
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
        mousePos.z = 10f; // Đảm bảo camera có chiều sâu đủ
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
