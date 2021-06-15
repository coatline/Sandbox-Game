using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeData
{
    public bool isActive = false;
    public int height;
    public Vector3Int trunkPosition;
    public bool spawnLeftTrunk;
    public bool spawnRightTrunk;

    public TreeData(int height, Vector3Int trunkPosition, bool spawnLeftTrunk, bool spawnRightTrunk, bool isActive = true)
    {
        this.height = height;
        this.trunkPosition = trunkPosition;
        this.spawnLeftTrunk = spawnLeftTrunk;
        this.spawnRightTrunk = spawnRightTrunk;
        this.isActive = isActive;
    }
}