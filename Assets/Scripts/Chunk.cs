using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Vector3Int topLeftBlock;
    public Vector3Int bottomRightBlock;

    //one value that value being the center of the chunk

    public Chunk(Vector3Int topLeftBlock, Vector3Int bottomRightBlock)
    {
        this.topLeftBlock = topLeftBlock;
        this.bottomRightBlock = bottomRightBlock;
    }
}
