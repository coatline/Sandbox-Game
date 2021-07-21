using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "New Effect", menuName = "Effect")]

public class EffectData : ScriptableObject
{
    public Sprite icon;
    public string effectName;
    public float length;
}
