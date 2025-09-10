using UnityEngine;
using static Weapon;
[CreateAssetMenu(menuName = "Effects/DefenseReudce")]
public class DefenseReudce : AbilityEffect
{
    public float defenseReudceTime;
    public float percent;
    public override void Apply(GameObject user, AbilityConfig config)
    {
        Stat stat = user.GetComponent<Stat>();
        if (stat != null && !stat.resistDefenseReduce)
        {
            stat.ApplyDefenseReduce(defenseReudceTime,percent);
        }
    }
}