using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk
{
    public Vector2Int bottomLeftBlock;
    public Vector3Int[] positions;
    public TileBase[] nullTiles;
    public TileBase[] fgtiles;
    public TileBase[] bgtiles;
    public TileBase[] mgtiles;
    public bool inUnloadQueue;
    public bool loaded;

    public Chunk(Vector2Int topLeftBlock, Vector3Int[] positions, TileBase[] fgtiles, TileBase[] mgtiles, TileBase[] bgtiles, TileBase[] nullTiles)
    {
        this.bottomLeftBlock = topLeftBlock;
        this.positions = positions;
        this.nullTiles = nullTiles;
        this.fgtiles = fgtiles;
        this.mgtiles = mgtiles;
        this.bgtiles = bgtiles;
    }
}
