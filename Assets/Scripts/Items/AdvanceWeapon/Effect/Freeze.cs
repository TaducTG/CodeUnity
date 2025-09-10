using UnityEngine;
using static Weapon;
[CreateAssetMenu(menuName = "Effects/Freeze")]
public class Freeze : AbilityEffect
{
    public float freezeTime;
    public float speedReducePercent;
    public override void Apply(GameObject user, AbilityConfig config)
    {
        Stat stat = user.GetComponent<Stat>();
        if (stat != null && !stat.resistFreeze)
        {
            stat.ApplyFreeze(freezeTime,speedReducePercent);
        }
    }
}
