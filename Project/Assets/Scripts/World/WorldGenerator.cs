using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class WorldGenerator : MonoBehaviour
{
    [SerializeField] bool inMenu;

    [Header("World Size")]
    public int worldWidth;
    public int worldHeight;

    [Header("Underground")]
    [SerializeField] int stoneStartOffset;
    [SerializeField] int sprinkleStoneStartOffset;
    [SerializeField] bool usePerlinCaves;
    public int caveStartingOffset;

    [Header("Cellular Automata Caves")]
    [SerializeField] int smoothIterations;

    [Header("Perlin Caves")]
    [Range(.01f, .8f)]
    [SerializeField] float caveSize;
    [Range(.1f, 2)]
    [SerializeField] float perlinCaveScale;

    [Header("Terrain Settings")]
    [Range(.1f, 5)]
    [SerializeField] float terrainScale;
    [SerializeField] float flowerScale;
    [SerializeField] float maxTerrainHeight;
    int terrainStartingHeight;

    [Header("References")]
    [SerializeField] ChestManager cm;
    [SerializeField] WorldLoader wl;
    [SerializeField] DataManager dm;

    [Header("Settings")]
    [Range(0, 100f)]
    [SerializeField] float chanceForTreeToSpawn;
    [Range(6, 20f)]
    [SerializeField] int maxTreeHeight;

    bool savable;

    void Awake()
    {
        if (inMenu) { return; }

        savable = (GD.currentWorld != null);

        if (savable)
        {
            Load();
        }
        else
        {
            GenerateWorldValues();
        }
    }

    void Load()
    {
        worldWidth = GD.wd.worldWidth;
        worldHeight = GD.wd.worldHeight;
    }

    void GenerateValues()
    {
        GenerateInitalTerrain();
        GenerateTrees();
        GenerateCaves();
        AddCaveDetail();
    }

    void GenerateWorldValues()
    {
        terrainStartingHeight = worldHeight / 2;

        GD.wd = new WorldData(worldWidth, worldHeight);

        GenerateValues();
    }

    public short[] GenerateNewBlockData(short worldWidth, short worldHeight)
    {
        this.worldWidth = worldWidth;
        this.worldHeight = worldHeight;

        GenerateWorldValues();

        short[] data = new short[worldWidth * worldHeight * 3];

        for (int x = 0; x < worldWidth; x++)
            for (int y = 0; y < worldHeight; y++)
                for (int z = 0; z < 3; z++)
                {
                    data[(x + (y * worldWidth) + (z * (worldHeight * worldWidth)))] = GD.wd.blockMap[x, y, z];
                }

        return data;
    }

    void GenerateInitalTerrain()
    {
        float xOffset = Random.Range(0f, 99999f);

        short dirt = dm.GetItemID("Dirt");
        short stone = dm.GetItemID("Stone");

        for (int x = 0; x < worldWidth; x++)
        {
            float xVal = ((x * (terrainScale / 100)) + xOffset);
            float normalnoise = Mathf.PerlinNoise(xVal, xOffset);

            float yVal = ((normalnoise) * (maxTerrainHeight));

            int surfaceTerrainHeight = Mathf.FloorToInt(yVal);

            GD.wd.highestTiles.Add((short)(surfaceTerrainHeight + terrainStartingHeight));

            for (int y = surfaceTerrainHeight + terrainStartingHeight; y >= 0; y--)
            {
                if (usePerlinCaves && (surfaceTerrainHeight + terrainStartingHeight) - y >= stoneStartOffset)
                {
                    GD.wd.blockMap[x, y, 0] = stone;
                }
                else
                {
                    if ((surfaceTerrainHeight + terrainStartingHeight) - y > sprinkleStoneStartOffset && Random.Range(0, 3) == 0)
                    {
                        GD.wd.blockMap[x, y, 0] = stone;
                    }
                    else
                    {
                        if (y == surfaceTerrainHeight + terrainStartingHeight)
                        {
                            GD.wd.blockMap[x, y, 0] = dm.GetItemID("Grass");
                        }
                        else
                        {
                            GD.wd.blockMap[x, y, 0] = dirt;
                        }
                    }
                }
            }
        }
    }

    public void GenerateStructure(Structure structure, int x, int y, bool duringRuntime = false)
    {
        for (int rx = 0; rx < structure.width; rx++)
            for (int ry = 0; ry < structure.height; ry++)
            {
                var index = rx + (ry * structure.width);
                var tile = structure.tiles[structure.structureData[index]];

                Vector2Int worldPosition = new Vector2Int((x + rx), y + ry);

                GD.wd.blockMap[worldPosition.x, worldPosition.y, tile.tileData.placeOnLayer] = tile.itemData.id;

                if (duringRuntime)
                {
                    wl.SetBlockInTilemap(worldPosition.x, worldPosition.y, tile.tileData.placeOnLayer, tile.tileData.tile);
                }
                else if (tile.tileData.isChest)
                {
                    cm.CreateNewChestDataAt(worldPosition.x, worldPosition.y, true);
                }
            }
    }

    public ItemDataContainer ItemAtPosition(int x, int y, byte layer)
    {
        return dm.GetItem(GD.wd.blockMap[x, y, layer]);
    }

    void GenerateTrees()
    {
        for (int x = 2; x < worldWidth - 3; x += 3)
        {
            if (Random.Range(0f, 100f) <= chanceForTreeToSpawn)
            {
                int treeHeight = Random.Range(3, maxTreeHeight + 1);
                bool spawnLeftStump = true;
                bool spawnRightStump = true;

                if (GD.wd.blockMap[x - 2, GD.wd.highestTiles[x], 0] == 0)
                {
                    spawnLeftStump = false;
                }
                if (GD.wd.blockMap[x + 2, GD.wd.highestTiles[x], 0] == 0)
                {
                    spawnRightStump = false;
                }

                var rand = Random.Range(0, 4);
                switch (rand)
                {
                    case 0: spawnLeftStump = false; break;
                    case 1: spawnRightStump = false; break;
                    case 2: spawnRightStump = false; spawnLeftStump = false; break;
                }

                Vector2Int startingPosition = new Vector2Int(x, GD.wd.highestTiles[x] + 1);
                GenerateTree(treeHeight, spawnLeftStump, spawnRightStump, startingPosition);
            }
        }
    }

    void GenerateTree(int treeHeight, bool spawnLeftStump, bool spawnRightStump, Vector2Int startingPosition)
    {
        TreeData newTree = new TreeData(startingPosition, spawnLeftStump, spawnRightStump, treeHeight);
        //trees.Add(startingPosition, newTree);

        for (int i = 0; i < treeHeight; i++)
        {
            if (i == 0)
            {
                if (!spawnLeftStump && !spawnRightStump)
                {
                    GD.wd.blockMap[startingPosition.x, startingPosition.y, 1] = dm.GetItemID("TreeTrunk");
                    continue;
                }

                GD.wd.blockMap[startingPosition.x, startingPosition.y, 1] = dm.GetItemID("TreeStumpMiddle");

                if (spawnLeftStump)
                {
                    GD.wd.blockMap[startingPosition.x - 1, startingPosition.y, 1] = dm.GetItemID("TreeStumpLeft");
                }

                if (spawnRightStump)
                {
                    GD.wd.blockMap[startingPosition.x + 1, startingPosition.y, 1] = dm.GetItemID("TreeStumpRight");
                }
            }
            else if (treeHeight - i > 1)
            {
                GD.wd.blockMap[startingPosition.x, startingPosition.y + i, 1] = dm.GetItemID("TreeTrunk");
            }
            else
            {
                GD.wd.blockMap[startingPosition.x, startingPosition.y + i, 1] = dm.GetItemID("TreeTop");
            }

        }
    }

    void GenerateFlowers()
    {
        float xOffset = Random.Range(0f, 99999f);

        for (int x = 0; x < worldWidth; x++)
        {
            //if (foregroundTilemap.GetTile(new Vector3Int(x + 1, GD.wd.highestTiles[x], 0)) == null || foregroundTilemap.GetTile(new Vector3Int(x - 1, GD.wd.highestTiles[x], 0)) == null)
            //{
            //    continue;
            //}

            float xVal = ((x * (terrainScale / 100)) + xOffset);
            float normalnoise = Mathf.PerlinNoise(xVal, xOffset);

            if (Random.Range(0f, normalnoise) <= .02f)
            {
                //if (Random.Range(0, 2) == 0)
                //{
                //    midgroundTilemap.SetTile(new Vector3Int(x, highestTiles[x] + 1, 0), redFlowerTile);
                //}
                //else
                //{
                //    midgroundTilemap.SetTile(new Vector3Int(x, highestTiles[x] + 1, 0), yellowFlowerTile);
                //}
            }
        }
    }

    void AddCaveDetail()
    {
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < GD.wd.highestTiles[x] - 1; y++)
            {
                GD.wd.blockMap[x, y, 2] = dm.GetItemID("Dirt Wall");
            }
        }

        var stoneId = dm.GetItemID("Stone");
        var dirtId = dm.GetItemID("Dirt");

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < GD.wd.highestTiles[x]; y++)
            {
                if (Random.Range(0, 100) > 10) { continue; }

                if (GD.wd.blockMap[x, y, 0] == stoneId || GD.wd.blockMap[x, y, 0] == dirtId)
                {
                    if (GD.wd.blockMap[x, y + 1, 0] == 0)
                    {
                        GD.wd.blockMap[x, y + 1, 0] = dm.GetItemID("Wood");
                    }
                }
            }
        }

        GenerateStructure(dm.GetStructure("TestStructure"), worldWidth / 2, worldHeight / 2);
    }

    void GenerateCaves()
    {
        if (usePerlinCaves)
        {
            var offset = new Vector2(Random.Range(0f, 99999f), Random.Range(0f, 99999f));

            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < GD.wd.highestTiles[x] - caveStartingOffset; y++)
                {
                    float xVal = ((x * (perlinCaveScale / 10)) + offset.x);
                    float yVal = ((y * (perlinCaveScale / 10)) + offset.y);

                    var noise = Mathf.PerlinNoise(xVal, yVal);

                    if (noise < caveSize)
                    {
                        GD.wd.blockMap[x, y, 0] = 0;
                    }
                }
            }
        }
        else
        {
            short stone = dm.GetItemID("Stone");

            for (int i = 0; i < smoothIterations; i++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    for (int y = 0; y < GD.wd.highestTiles[x] - caveStartingOffset; y++)
                    {
                        //Randomly set tiles
                        if (i == 0)
                        {
                            if (Random.Range(0, 2) == 0)
                            {
                                GD.wd.blockMap[x, y, 0] = stone;
                            }
                            else
                            {
                                GD.wd.blockMap[x, y, 0] = 0;
                            }
                        }
                        //Create order from madness
                        else
                        {
                            if (x == 0 || y == 0 || y >= worldHeight - 1 || x >= worldWidth - 1)
                            {
                                GD.wd.blockMap[x, y, 0] = stone;
                                continue;
                            }

                            int faces = 0;

                            if (GD.wd.blockMap[x - 1, y, 0] != 0)
                            {
                                faces++;
                            }
                            if (GD.wd.blockMap[x + 1, y, 0] != 0)
                            {
                                faces++;
                            }
                            if (GD.wd.blockMap[x, y + 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (GD.wd.blockMap[x, y - 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (GD.wd.blockMap[x - 1, y - 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (GD.wd.blockMap[x + 1, y + 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (GD.wd.blockMap[x - 1, y + 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (GD.wd.blockMap[x + 1, y - 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (faces < 4)
                            {
                                GD.wd.blockMap[x, y, 0] = 0;
                            }
                            else if (faces > 4)
                            {
                                //if (Random.Range(0, 5) == 0)
                                //{
                                //    blockMap[x, y] = BlockType.dirt;
                                //}
                                //else
                                {
                                    GD.wd.blockMap[x, y, 0] = stone;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void GenerateOres()
    {
        var offset = new Vector2(Random.Range(0f, 99999999f), Random.Range(0f, 99999999f));

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight - caveStartingOffset; y++)
            {
                var noise = Mathf.PerlinNoise((float)((x + offset.x) * perlinCaveScale / terrainStartingHeight - caveStartingOffset), (y + offset.y) * perlinCaveScale / (terrainStartingHeight - caveStartingOffset));

                if (noise < .4f)
                {
                    GD.wd.blockMap[x, y, 0] = dm.GetItemID("Stone");
                }
                else if (noise < .1f)
                {

                }
            }
        }
    }
}


