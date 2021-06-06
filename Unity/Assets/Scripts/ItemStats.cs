using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class ItemStats : ScriptableObject
{
    public enum Condition
    {
        atNight,
        lowHealth,
        none
    }

    public ItemStats[] itemsToCraft;
    public int amountProduced;
    public int itemType;
    public int damageVal;
    public int healVal;
    public bool placeable;
    public int maxStack;
    public int value;
    //public ItemStats tilesNearbyToBeCraftable;
}
