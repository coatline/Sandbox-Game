using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk
{
    public Vector2Int topLeftBlock;
    public Vector3Int[] positions;
    public TileBase[] nullTiles;
    public TileBase[] tiles;
    public bool inUnloadQueue;
    public bool loaded;

    public Chunk(Vector2Int topLeftBlock, Vector3Int[] positions, TileBase[] tiles, TileBase[] nullTiles)
    {
        this.topLeftBlock = topLeftBlock;
        this.positions = positions;
        this.nullTiles = nullTiles;
        this.tiles = tiles;
    }
}
