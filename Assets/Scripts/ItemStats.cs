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
    Pet
}

public enum Action
{
    none,
    swing
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class ItemStats : ScriptableObject
{
    public ItemStats[] itemsToCraft;
    public EffectData givenEffect;
    public DamageType damageType;
    public Action actionOnUse;

    public int amountPerCraft;

    public string _name;
    public string description;
    public float speed;

    public bool consumable;

    public int healthOnUse;
    public int itemType;
    public int damageVal;
    public bool placeable;
    public int maxStack;
    public int value;
    //public ItemStats tilesNearbyToBeCraftable;
}
