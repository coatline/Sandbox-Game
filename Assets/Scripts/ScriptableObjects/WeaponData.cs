using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]

public class WeaponData : ScriptableObject
{
    public DamageType damageType;
    public RangedData rangedData;
    
    public int criticalStrikeChance;
    public int damageVal;
}
