using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AbilityEffect;
using static Weapon;
[CreateAssetMenu(menuName = "Abilities/Thrust")]
public class ThrustAbility : WeaponAbility
{
    // 👉 khoảng thrust (có thể config)
    public float thrustDistance = 2.0f; // hoặc config.thrustDistance
    public float thrustDuration = 0.15f; // thời gian đi tới
    public float returnDuration = 0.1f; // thời gian quay về

    public float x;

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
        if (player != null)
        {
            player.StartCoroutine(Thrust(user,mainHand,user.transform.position,GetMouseWorldPosition() ,config, config.effects));
        }


    }
    private IEnumerator Thrust(GameObject user,GameObject mainHand, Vector3 origin, Vector3 target, AbilityConfig config, List<AbilityEffect> effects)
    {
        Transform hitbox = mainHand.transform.Find("hitbox");
        if (hitbox == null) yield break;

        WeaponHitbox hb = hitbox.GetComponent<WeaponHitbox>();

        // gán effect OnHit
        List<AbilityEffect> onHitEffects = new List<AbilityEffect>();
        foreach (var e in effects)
        {
            if (e.targetType == EffectTargetType.OnHit)
            {
                onHitEffects.Add(e);
            }
        }
        hb.SetEffects(onHitEffects);

        // 👉 hướng thrust
        Vector3 dir = (target - origin).normalized;

        if (user.transform.localScale.x < 0)
        {
            dir.x = -dir.x;
        }

        Vector3 startPos = mainHand.transform.localPosition;
        Vector3 endPos = startPos + dir * thrustDistance;

        Quaternion startRot = mainHand.transform.localRotation;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        angle = angle - 90;

        Quaternion targetRot = Quaternion.Euler(0, 0, angle);

        hitbox.gameObject.SetActive(true);

        // thrust tới trước
        float t = 0f;
        while (t < thrustDuration)
        {
            t += Time.deltaTime;
            float ratio = t / thrustDuration;

            // move + rotate
            mainHand.transform.localPosition = Vector3.Lerp(startPos, endPos, ratio);
            mainHand.transform.localRotation = Quaternion.Lerp(startRot, targetRot, ratio);

            yield return null;
        }

        // giữ một chút
        yield return new WaitForSeconds(0.05f);

        // trả lại vị trí & rotation
        t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float ratio = t / returnDuration;

            mainHand.transform.localPosition = Vector3.Lerp(endPos, startPos, ratio);
            mainHand.transform.localRotation = Quaternion.Lerp(targetRot, startRot, ratio);

            yield return null;
        }

        mainHand.transform.localPosition = startPos;
        mainHand.transform.localRotation = startRot;
        hitbox.gameObject.SetActive(false);
    }
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // Đảm bảo không bị z=0 khi dùng perspective camera
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
