using UnityEngine;
using static Weapon;
[CreateAssetMenu(menuName = "Effects/Poison")]
public class Poison : AbilityEffect
{
    public float poisonTime;
    public override void Apply(GameObject user, AbilityConfig config)
    {
        Stat stat = user.GetComponent<Stat>();
        if (stat != null && !stat.resistPoison)
        {
            stat.ApplyPoison(poisonTime);
        }
    }
}
