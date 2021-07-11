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
    meleeWeapon,
    projectile,
    tool,
    consumable,
    armor,
    block,
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]

public class ItemDataContainer : ScriptableObject
{
    public ItemData itemData;
    public ItemType itemType;
    public TileData tileData;
    public WeaponData weaponData;
    public PlayerAction actionOnUse;
    
}

[System.Serializable]

public class ItemData
{
    [Header("Data")]
    public SoundData useSound;
    public float useTime;
    public bool generateCollider;
    public short maxStack;
    public short value;
    public short id;
    [Header("Light")]
    public bool emitsLight;
    public Color emitColor;
    [Header("Visuals")]
    public Sprite itemSprite;
    public Sprite heldSprite;
    public bool showOnSelect;
    public bool hideOnUse;
    [Header("Text")]
    public string itemName;
    public string description;
    public string rarity;
}