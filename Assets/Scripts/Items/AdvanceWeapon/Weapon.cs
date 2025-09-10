using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapons")]
public class Weapon : Items
{
    public enum InputKey { LeftClick, RightClick, Q, E, R }

    [System.Serializable]
    public class AbilityConfig
    {
        [Header("Consumed")]
        public float manaUsed;
        public float healthUsed;

        [Header("Normal stat")]
        public int damage;
        public float attackSpeed;

        [Header("For swing type")]
        public float angle;

        [Header("For fire projectile type")]
        public float projectileSpeed;
        public GameObject projectilePrefab;

        [Header("For graplinghook type")]
        public float maxDistance;
        public float hookSpeed;

        [Header("Effect")]
        public List<AbilityEffect> effects;// List effect kèm theo
    }

    [System.Serializable]
    public class WeaponAbilitySlot
    {
        public InputKey triggerKey;        // Phím để kích hoạt
        public WeaponAbility ability;      // ScriptableObject logic
        public AbilityConfig config;       // Config tham số riêng


        [System.NonSerialized]
        public float nextAvailableTime = 0f;


    }

    public List<WeaponAbilitySlot> abilities = new List<WeaponAbilitySlot>();
}
