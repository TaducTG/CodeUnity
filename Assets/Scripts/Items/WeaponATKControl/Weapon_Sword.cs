using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Sword", menuName = "Items/Weapon/Sword")]
public class Weapon_Sword : WeaponItem
{
    public float angle = 90f;

    public override void ATK(GameObject user, GameObject mainHand)
    {
        MonoBehaviour runner = user.GetComponent<MonoBehaviour>(); // ⚠ Không dùng thế này

        // Giải pháp đúng: tạo MonoBehaviour runner cụ thể, ví dụ:
        Player player = user.GetComponent<Player>();
        if (player != null)
        {
            player.StartCoroutine(Swing(mainHand, angle, attackSpeed / 2f, attackSpeed / 2f));
        }
    }

    IEnumerator Swing(GameObject mainHand, float angle, float duration, float returnDuration)
    {
        Transform hitbox = mainHand.transform.Find("hitbox");
        if (hitbox == null) yield break;

        hitbox.gameObject.SetActive(true);
        Quaternion originalRotation = mainHand.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(mainHand.transform.localEulerAngles + new Vector3(0, 0, -angle));

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