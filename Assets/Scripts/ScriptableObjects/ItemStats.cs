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

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class ItemStats : ScriptableObject
{
    public ItemStats[] itemsToCraft;
    public EffectData givenEffect;
    public WeaponData weaponData;
    public PlayerAction actionOnUse;
    public int tilemapLayer;

    public int amountPerCraft;

    public string _name;
    public string description;
    public float useSpeed;

    public bool consumable;

    public int healthOnUse;
    public int itemType;
    public bool placeable;
    public int maxStack;
    public int value;
    //public ItemStats tilesNearbyToBeCraftable;
}
