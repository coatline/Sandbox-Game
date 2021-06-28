using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Condition
{
    atNight,
    lowHealth,
    none
}

public enum DamageType
{
    Melee,
    Ranged,
    Magic,
    Potion,
    Pet
}

public enum PlayerAction
{
    none,
    swing,
    shoot,
    spear,
    drink
}

public enum Buff
{
    movementSpeed,
    defense,
    criticalStrikeChance,
    regeneration,
    maxHealth,
    attackSpeed,
    strength
}

public enum ItemType
{
    rangedWeapon,
    projectile,
    tool,
    consumable,
    armor,
    block,
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]

public class ItemDataContainer : ScriptableObject
{
    public ItemType itemType;
    public TileData tileData;
    public WeaponData weaponData;
    public PlayerAction actionOnUse;
    public Sprite itemSprite;
    public bool emitsLight;
    public Color emitColor;

    public string itemName;
    public string description;
    public string rarity;

    public float useSpeed;

    public short maxStack;
    public short value;
    public short id;
}
