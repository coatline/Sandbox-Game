using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Structure", menuName = "Structure")]

[System.Serializable]
public class Structure : ScriptableObject
{
    public string _name;
    public byte width;
    public byte height;
    public List<RuleTile> tiles;
    public int[] structure;
}
