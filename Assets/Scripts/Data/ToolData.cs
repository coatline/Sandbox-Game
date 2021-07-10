using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tool
{
    pickaxe,
    axe,
    hammer
}

[System.Serializable]
public class ToolData 
{
    public byte worldLayer;
    public Tool toolType;
    public int strength;
}
