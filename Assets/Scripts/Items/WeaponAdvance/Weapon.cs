using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon")]
public class Weapon : Items
{
    public enum InputKey { LeftClick, RightClick, Q, E, R }

    [System.Serializable]
    public class AbilityConfig
    {
        public int damage;
        public float attackSpeed;
        public float lifetime;
        public float range;
        public GameObject projectilePrefab;

        // List effect kèm theo
        public List<AbilityEffect> effects;
    }

    [System.Serializable]
    public class WeaponAbilitySlot
    {
        public InputKey triggerKey;        // Phím để kích hoạt
        public WeaponAbility ability;      // ScriptableObject logic
        public AbilityConfig config;       // Config tham số riêng
    }

    public List<WeaponAbilitySlot> abilities = new List<WeaponAbilitySlot>();
}
