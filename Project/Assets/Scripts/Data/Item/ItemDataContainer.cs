using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    none,
    rangedWeapon,
    meleeWeapon,
    projectile,
    tool,
    consumable,
    armor,
    block,
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
[System.Serializable]

public class ItemDataContainer : ScriptableObject
{
    public ItemData itemData;
    public TileData tileData;
    public WeaponData weaponData;
    public ItemType itemType;
    public PlayerAction actionOnUse;

    private void OnValidate()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
}

[System.Serializable]

public class ItemData
{
    [Header("Data")]
    public SoundData useSound;
    public float animationTime;
    public float useTime;
    public bool generateCollider;
    public short maxStack;
    public short value;
    [Header("Light")]
    public bool emitsLight;
    public Color emitColor;
    [Header("Visuals")]
    public Sprite itemSprite;
    public Sprite heldSprite;
    public bool showPlacement;
    public bool showOnSelect;
    public bool hideOnUse;
    [Header("Text")]
    public string description;
    public string rarity;

    public short id;
}
