using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WorldData
{
    public Dictionary<Vector2Int, ItemDataContainer> multiTileItems;
    public Dictionary<Vector2Int, ChestData> chests;

    public int worldWidth;
    public int worldHeight;
    
    public float elapsedTime;
    public TimeData td;

    public short[,,] blockMap;

    public List<short> highestTiles;

    public WorldData(int worldWidth, int worldHeight)
    {
        multiTileItems = new Dictionary<Vector2Int, ItemDataContainer>();
        chests = new Dictionary<Vector2Int, ChestData>();
        blockMap = new short[worldWidth, worldHeight, 3];
        highestTiles = new List<short>();

        elapsedTime = 0;

        td = new TimeData(1, 8);

        this.worldWidth = worldWidth;
        this.worldHeight = worldHeight;
    }
}
