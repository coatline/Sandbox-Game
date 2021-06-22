using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public DamageType damageType;
    public RangedData rangedData;
    
    public int criticalStrikeChance;
    public int damageVal;
}
