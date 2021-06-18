using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Armor", menuName = "Armor")]

public class ArmorData : ScriptableObject
{
    public int defenseValue;
    public Buff fullSetBonus;
}
