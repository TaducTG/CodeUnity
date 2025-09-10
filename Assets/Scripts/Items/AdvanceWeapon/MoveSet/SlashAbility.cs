using UnityEngine;
using System.Collections;
using static Weapon;
using static AbilityEffect;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Abilities/Slash")]
public class SlashAbility : WeaponAbility
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
        if (player != null)
        {
            player.StartCoroutine(Swing(mainHand, config, config.effects));
        }
    }

    private IEnumerator Swing(GameObject mainHand, AbilityConfig config, List<AbilityEffect> effects)
    {
        Transform hitbox = mainHand.transform.Find("hitbox");
        if (hitbox == null) yield break;

        WeaponHitbox hb = hitbox.GetComponent<WeaponHitbox>();

        // gán effect
        List<AbilityEffect> onHitEffects = new List<AbilityEffect>();
        foreach (var e in effects)
        {
            if (e.targetType == EffectTargetType.OnHit)
            {
                onHitEffects.Add(e);
            }
        }
        hb.SetEffects(onHitEffects);

        hitbox.gameObject.SetActive(true);
        Quaternion originalRotation = mainHand.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(mainHand.transform.localEulerAngles + new Vector3(0, 0, -config.angle));

        float duration = config.attackSpeed / 2f;
        float returnDuration = config.attackSpeed / 2f;

        float time = 0f;
        while (time < duration)
        {
            mainHand.transform.localRotation = Quaternion.Lerp(originalRotation, targetRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        mainHand.transform.localRotation = targetRotation;

        time = 0f;
        while (time < returnDuration)
        {
            mainHand.transform.localRotation = Quaternion.Lerp(targetRotation, originalRotation, time / returnDuration);
            time += Time.deltaTime;
            yield return null;
        }

        hitbox.gameObject.SetActive(false);
        mainHand.transform.localRotation = originalRotation;
    }
}
