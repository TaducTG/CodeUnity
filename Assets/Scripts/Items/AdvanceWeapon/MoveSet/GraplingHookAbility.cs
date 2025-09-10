using System.Collections;
using UnityEngine;
using static Weapon;

[CreateAssetMenu(menuName = "Abilities/GrapplingHook")]
public class WeaponAbility_GrapplingHook : WeaponAbility
{
    public float hookSpeed = 15f;
    public float pullSpeed = 10f;
    public float maxDistance = 12f;
    public GameObject hookPrefab;

    public override void Activate(GameObject user, GameObject mainHand, AbilityConfig config)
    {
        Player player = user.GetComponent<Player>();
        if (player == null) return;
        // spawn hook
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        Vector3 dir = (mouseWorld - user.transform.position).normalized;

        GameObject hook = GameObject.Instantiate(hookPrefab, user.transform.position, Quaternion.identity);

        GrapplingHookProjectile hookScript = hook.GetComponent<GrapplingHookProjectile>();
        hookScript.Init(user, dir, hookSpeed, pullSpeed, maxDistance);
    }
}
