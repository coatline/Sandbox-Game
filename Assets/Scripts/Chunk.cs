using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Vector2Int topLeftBlock;

    public Chunk(Vector2Int topLeftBlock)
    {
        this.topLeftBlock = topLeftBlock;
    }
}
