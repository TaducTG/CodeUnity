using static Weapon;
using UnityEngine;

public abstract class AbilityEffect : ScriptableObject
{
    public enum EffectTargetType
    {
        Self,       // áp dụng ngay khi activate
        OnHit       // áp dụng khi va chạm với entity khác
    }
    public EffectTargetType targetType = EffectTargetType.Self;
    public abstract void Apply(GameObject user, AbilityConfig config);
}
