using UnityEngine;
using static Weapon;
[CreateAssetMenu(menuName = "Effects/Burn")]
public class Burn : AbilityEffect
{
    public float burnTime;

    public override void Apply(GameObject user, AbilityConfig config)
    {
        Stat stat = user.GetComponent<Stat>();
        if (stat != null && !stat.resistBurn)
        {
            stat.ApplyBurn(burnTime);
        }
    }
}
