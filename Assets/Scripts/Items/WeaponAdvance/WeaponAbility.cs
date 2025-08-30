using static Weapon;
using UnityEngine;

public abstract class WeaponAbility : ScriptableObject
{
    public string abilityName;

    public abstract void Activate(GameObject user, AbilityConfig config);
}