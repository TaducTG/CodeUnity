using static Weapon;
using UnityEngine;

public abstract class AbilityEffect : ScriptableObject
{
    public abstract void Apply(GameObject user, AbilityConfig config);
}
