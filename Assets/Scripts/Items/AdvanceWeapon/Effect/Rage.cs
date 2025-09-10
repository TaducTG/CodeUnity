using UnityEngine;
using static Weapon;
[CreateAssetMenu(menuName = "Effects/Rage")]
public class Rage : AbilityEffect
{
    public override void Apply(GameObject user, AbilityConfig config)
    {
        Stat stat = user.GetComponent<Stat>();
        stat.ApplyRage();
    }
}