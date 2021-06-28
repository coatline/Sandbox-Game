using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeData
{
    public Vector2Int stumpPos;
    public bool leftStump;
    public bool rightStump;
    public int height;

    public TreeData(Vector2Int stumpPos, bool leftStump, bool rightStump, int height)
    {
        this.stumpPos = stumpPos;
        this.leftStump = leftStump;
        this.rightStump = rightStump;
        this.height = height;
    }
}