using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldGenerator : MonoBehaviour
{
    Camera mainCamera;
    Player player;

    [Header("World Height")]
    public int worldWidth;
    public int worldHeight;
    [SerializeField] int chunkSize;

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
    [SerializeField] bool displayChunks;
    [SerializeField] bool fillInWorld;
    int terrainStartingHeight;

    [Header("References")]
    [SerializeField] Tilemap foregroundTilemap;
    [SerializeField] Tilemap midgroundTilemap;
    [SerializeField] Tilemap backgroundTilemap;
    [SerializeField] Player playerPrefab;
    [SerializeField] List<Structure> structures;
    [SerializeField] Pickup pickupPrefab;
    public List<ItemDataContainer> itemData;

    [Header("Settings")]
    [SerializeField] int excessChunksToLoad;
    [Range(0, 100f)]
    [SerializeField] float chanceForTreeToSpawn;
    [Range(6, 20f)]
    [SerializeField] int maxTreeHeight;

    Vector3 viewDistance;
    Dictionary<Vector2Int, Chunk> chunks;
    Dictionary<Vector2Int, TreeData> trees;
    Dictionary<string, short> itemIDfromName;
    Dictionary<string, Structure> structureFromName;
    [HideInInspector]
    public List<int> highestTiles;
    [HideInInspector]
    public short[,] fgblockMap;
    [HideInInspector]
    public short[,] mgblockMap;
    [HideInInspector]
    public short[,] bgblockMap;
    int chunksOnX;
    int chunksOnY;

    void Awake()
    {
        chunksOnX = Mathf.CeilToInt(((float)worldWidth / chunkSize));
        chunksOnY = Mathf.CeilToInt(((float)worldHeight / chunkSize));

        viewDistance.x += chunkSize / 2;
        viewDistance.y += chunkSize / 2;

        terrainStartingHeight = worldHeight / 2;

        mainCamera = Camera.main;

        viewDistance = new Vector3((mainCamera.orthographicSize * mainCamera.aspect), mainCamera.orthographicSize, 0);
        fgblockMap = new short[worldWidth, worldHeight];
        mgblockMap = new short[worldWidth, worldHeight];
        bgblockMap = new short[worldWidth, worldHeight];

        trees = new Dictionary<Vector2Int, TreeData>();
        chunks = new Dictionary<Vector2Int, Chunk>();
        chunksToUnload = new List<Vector2Int>();
        highestTiles = new List<int>();

        var cb = GetComponent<CreateBarrier>();
        cb.offset = new Vector2(worldWidth / 2, worldHeight / 2);
        cb.mapSize = new Vector2(worldWidth / 2, worldHeight / 2);

        InitializeData();

        GenerateInitalTerrain();

        GenerateStructure("TestStructure", worldWidth / 2, highestTiles[worldWidth / 2]);
        GenerateTrees();

        //GenerateFlowers();

        //GenerateOres();

        GenerateCaves();

        AddCaveDetail();

        AddChunks();

        SpawnPlayer();

        InitializeWorld();
    }

    void AddBarrier()
    {
        CreateBarrier barrier = GetComponent<CreateBarrier>();
        barrier.mapSize = new Vector2(worldWidth, worldHeight);
        barrier.offset = new Vector2(worldWidth, worldHeight);
    }

    void InitializeData()
    {
        structureFromName = new Dictionary<string, Structure>();
        itemIDfromName = new Dictionary<string, short>();

        for (short i = 0; i < itemData.Count; i++)
        {
            itemData[i].id = i;

            if (itemData[i].itemName == "")
            {
                Debug.LogError($"item id of {itemData[i].id}'s name needs to be assigned!");
            }

            itemIDfromName.Add(itemData[i].itemName, i);
        }

        for (int j = 0; j < structures.Count; j++)
        {
            if (structures[j]._name == "")
            {
                Debug.LogError($"structure of {j}'s name needs to be assigned!");
            }

            structureFromName.Add(structures[j]._name, structures[j]);
        }
    }

    void GenerateInitalTerrain()
    {
        float xOffset = Random.Range(0f, 99999f);

        for (int x = 0; x < worldWidth; x++)
        {
            float xVal = ((x * (terrainScale / 100)) + xOffset);
            float normalnoise = Mathf.PerlinNoise(xVal, xOffset);

            float yVal = ((normalnoise) * (maxTerrainHeight));

            int surfaceTerrainHeight = Mathf.FloorToInt(yVal);

            highestTiles.Add(surfaceTerrainHeight + terrainStartingHeight);

            for (int y = surfaceTerrainHeight + terrainStartingHeight; y >= 0; y--)
            {
                //MAKE FASTER BY GENERATING 1s ONLY IN PLACES WE KNOW THAT CAVES WILL NOT BE

                //if (y < 0 || y >= worldHeight || x < 0 || x >= terrainWidth) { print("INVALIDE POSITION"); continue; }

                if (usePerlinCaves && (surfaceTerrainHeight + terrainStartingHeight) - y >= stoneStartOffset)
                {
                    fgblockMap[x, y] = itemIDfromName["Stone"];
                }
                else
                {
                    if ((surfaceTerrainHeight + terrainStartingHeight) - y > sprinkleStoneStartOffset && Random.Range(0, 3) == 0)
                    {
                        fgblockMap[x, y] = itemIDfromName["Stone"];
                    }
                    else
                    {
                        if (y == surfaceTerrainHeight + terrainStartingHeight)
                        {
                            fgblockMap[x, y] = itemIDfromName["Grass"];
                        }
                        else
                        {
                            fgblockMap[x, y] = itemIDfromName["Dirt"];
                        }
                    }
                }
            }
        }
    }

    public Vector2Int blockModifiedAt = -Vector2Int.one;

    void GenerateStructure(string name, int x, int y)
    {
        Structure structure = structureFromName[name];

        for (int rx = 0; rx < structure.width; rx++)
            for (int ry = 0; ry < structure.height; ry++)
            {
                var index = rx + (ry * structure.width);
                var tile = structure.tiles[structure.structureData[index]];
                Vector2Int worldPosition = new Vector2Int((x + rx), y + ry);

                switch (tile.tileData.layer)
                {
                    case WorldLayer.foreground:
                        fgblockMap[worldPosition.x, worldPosition.y] = tile.id;
                        break;
                    case WorldLayer.midground:
                        mgblockMap[worldPosition.x, worldPosition.y] = tile.id;
                        break;
                    case WorldLayer.background:
                        bgblockMap[worldPosition.x, worldPosition.y] = tile.id;
                        break;
                }
            }
    }

    // Pretty much a Queue
    List<Vector2Int> treeBlocksToChop = new List<Vector2Int>();

    IEnumerator ChopTree()
    {
        while (treeBlocksToChop.Count > 0)
        {
            Vector2Int pos = treeBlocksToChop[0];
            BreakBlock(pos.x, pos.y, WorldLayer.midground, true);
            treeBlocksToChop.RemoveAt(0);
            yield return new WaitForSeconds(.02f);
        }
    }

    public void BreakBlock(int x, int y, WorldLayer layer, bool isTreeBroken = false)
    {
        var chunkCoordinates = WorldCoordinatesToNearestChunkCoordinates(new Vector2(x, y));
        var chunkWorldCoordinates = chunkCoordinates * new Vector2Int(chunkSize, chunkSize);

        ItemDataContainer itemBroken = null;

        switch (layer)
        {
            case WorldLayer.foreground:

                if (fgblockMap[x, y] == 0)
                {
                    return;
                }

                blockModifiedAt = new Vector2Int(x, y);
                itemBroken = itemData[fgblockMap[x, y]];
                fgblockMap[x, y] = 0;
                foregroundTilemap.SetTile(new Vector3Int(x, y, 0), null);
                chunks[chunkCoordinates].fgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = null;

                break;
            case WorldLayer.midground:

                if (mgblockMap[x, y] == 0)
                {
                    return;
                }

                itemBroken = itemData[mgblockMap[x, y]];

                if (!isTreeBroken && itemBroken.tileData.treeTile)
                {
                    for (int ty = 0; ty < maxTreeHeight; ty++)
                    {
                        TreeData tree;

                        int treeStumpWorldPositionY = y - ty;

                        // ty moves down the tree to find the root
                        // When it finds it it will start to break from the point it was broken
                        if (trees.TryGetValue(new Vector2Int(x, treeStumpWorldPositionY), out tree))
                        {
                            for (int by = y; by < ((tree.height) + y); by++)
                            {
                                treeBlocksToChop.Add(new Vector2Int(x, by));
                                //BreakBlock(x, by, WorldLayer.midground, true);
                            }

                            // If the stump was broken then break the side stumps too
                            if (treeStumpWorldPositionY == y)
                            {
                                if (tree.leftStump)
                                {
                                    BreakBlock(x - 1, treeStumpWorldPositionY, WorldLayer.midground, true);
                                }
                                if (tree.rightStump)
                                {
                                    BreakBlock(x + 1, treeStumpWorldPositionY, WorldLayer.midground, true);
                                }

                                trees.Remove(new Vector2Int(x, treeStumpWorldPositionY));
                            }

                            StartCoroutine(ChopTree());

                            break;
                        }
                    }
                }
                else if(isTreeBroken && !itemBroken.tileData.treeTile) { /* Just another Midground block that should not be broken */ return; }

                mgblockMap[x, y] = 0;
                midgroundTilemap.SetTile(new Vector3Int(x, y, 0), null);
                chunks[chunkCoordinates].mgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = null;

                break;
            case WorldLayer.background:

                if (bgblockMap[x, y] == 0)
                {
                    return;
                }

                itemBroken = itemData[bgblockMap[x, y]];
                bgblockMap[x, y] = 0;
                backgroundTilemap.SetTile(new Vector3Int(x, y, 0), null);
                chunks[chunkCoordinates].bgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = null;

                break;
        }

        var rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

        var pickup = Instantiate(pickupPrefab, new Vector3(x + Random.Range(-.25f, .25f), y + Random.Range(-.25f, .25f), 0) + Vector3.one / 2, rotation);
        //pickup.GetComponent<Pickup>()
        pickup.SetItem(itemBroken.tileData.dropOnBreak, itemBroken.tileData.amountDropped);
    }

    public void PlaceBlock(int x, int y, ItemDataContainer tileData)
    {
        blockModifiedAt = new Vector2Int(x, y);

        var chunkCoordinates = WorldCoordinatesToNearestChunkCoordinates(new Vector2(x, y));
        var chunkWorldCoordinates = chunkCoordinates * new Vector2Int(chunkSize, chunkSize);

        switch (tileData.tileData.layer)
        {
            case WorldLayer.foreground:

                fgblockMap[x, y] = tileData.id;
                foregroundTilemap.SetTile(new Vector3Int(x, y, 0), tileData.tileData.tile);
                chunks[chunkCoordinates].fgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = tileData.tileData.tile;

                break;
            case WorldLayer.midground:

                mgblockMap[x, y] = tileData.id;
                midgroundTilemap.SetTile(new Vector3Int(x, y, 0), tileData.tileData.tile);
                chunks[chunkCoordinates].mgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = tileData.tileData.tile;

                break;
            case WorldLayer.background:

                bgblockMap[x, y] = tileData.id;
                backgroundTilemap.SetTile(new Vector3Int(x, y, 0), tileData.tileData.tile);
                chunks[chunkCoordinates].bgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = tileData.tileData.tile;

                break;
        }
    }

    void GenerateTrees()
    {
        for (int x = 2; x < worldWidth - 3; x += 3)
        {
            //do not put on uneven block
            if (fgblockMap[x - 1, highestTiles[x]] == 0 || fgblockMap[x + 1, highestTiles[x]] == 0)
            {
                continue;
            }

            if (Random.Range(0f, 100f) <= chanceForTreeToSpawn)
            {
                int treeHeight = Random.Range(3, maxTreeHeight + 1);
                bool spawnLeftStump = true;
                bool spawnRightStump = true;

                if (fgblockMap[x - 2, highestTiles[x]] == 0)
                {
                    spawnLeftStump = false;
                }
                if (fgblockMap[x + 2, highestTiles[x]] == 0)
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

                Vector2Int startingPosition = new Vector2Int(x, highestTiles[x] + 1);
                GenerateTree(treeHeight, spawnLeftStump, spawnRightStump, startingPosition);
            }
        }
    }

    void GenerateTree(int treeHeight, bool spawnLeftStump, bool spawnRightStump, Vector2Int startingPosition)
    {
        TreeData newTree = new TreeData(startingPosition, spawnLeftStump, spawnRightStump, treeHeight);
        trees.Add(startingPosition, newTree);

        for (int i = 0; i < treeHeight; i++)
        {
            if (i == 0)
            {
                if (!spawnLeftStump && !spawnRightStump)
                {
                    mgblockMap[startingPosition.x, startingPosition.y] = itemIDfromName["TreeTrunk"];
                    continue;
                }

                mgblockMap[startingPosition.x, startingPosition.y] = itemIDfromName["TreeStumpMiddle"];

                if (spawnLeftStump)
                {
                    mgblockMap[startingPosition.x - 1, startingPosition.y] = itemIDfromName["TreeStumpLeft"];
                }

                if (spawnRightStump)
                {
                    mgblockMap[startingPosition.x + 1, startingPosition.y] = itemIDfromName["TreeStumpRight"];
                }
            }
            else if (treeHeight - i > 1)
            {
                mgblockMap[startingPosition.x, startingPosition.y + i] = itemIDfromName["TreeTrunk"];
            }
            else
            {
                mgblockMap[startingPosition.x, startingPosition.y + i] = itemIDfromName["TreeTop"];
            }

        }
    }

    void GenerateFlowers()
    {
        float xOffset = Random.Range(0f, 99999f);

        for (int x = 0; x < worldWidth; x++)
        {
            if (foregroundTilemap.GetTile(new Vector3Int(x + 1, highestTiles[x], 0)) == null || foregroundTilemap.GetTile(new Vector3Int(x - 1, highestTiles[x], 0)) == null)
            {
                continue;
            }

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

    //Spawn player and assign camera variables
    void SpawnPlayer()
    {
        var camerabarriers = new GameObject("Camera Barriers");

        var topright = new GameObject("Top Right Camera Barrier");
        topright.transform.position = new Vector3(worldWidth, worldHeight);
        var bottomleft = new GameObject("Bottom Left Camera Barrier");
        bottomleft.transform.position = new Vector3(0, 0);

        topright.transform.SetParent(camerabarriers.transform);
        bottomleft.transform.SetParent(camerabarriers.transform);

        player = Instantiate(playerPrefab, new Vector3(worldWidth / 2, highestTiles[worldWidth / 2] + 2, 0), Quaternion.identity);
        playerRb = player.GetComponent<Rigidbody2D>();

        player.blockTilemap = foregroundTilemap;
        player.backgroundTilemap = midgroundTilemap;

        var script = mainCamera.GetComponent<CameraFollowWithBarriers>();
        mainCamera.transform.position = player.transform.position - new Vector3(0, 0, 10);

        script.topRightBarrier = topright.transform;
        script.bottomLeftBarrier = bottomleft.transform;
        script.followObject = player.transform;
    }

    bool drawn = false;

    private void OnDrawGizmos()
    {
        if (!displayChunks || drawn || chunks == null) { return; }

        drawn = true;

        for (int x = 0; x < chunksOnX; x++)
        {
            for (int y = 0; y < chunksOnY; y++)
            {
                Color color = Color.red;

                Vector2Int chunkPos = new Vector2Int(x, y);
                if (!chunks.TryGetValue(chunkPos, out Chunk chunk)) { return; }
                Vector3Int topRightBlock = new Vector3Int(chunks[chunkPos].bottomLeftBlock.x + chunkSize, chunks[chunkPos].bottomLeftBlock.y + chunkSize, 0);
                Vector3Int bottomLeftBlock = new Vector3Int(chunks[chunkPos].bottomLeftBlock.x, chunks[chunkPos].bottomLeftBlock.y, 0);
                //Gizmos.DrawWireCube()
                Debug.DrawLine(bottomLeftBlock, topRightBlock - new Vector3Int(0, chunkSize, 0), color, Mathf.Infinity);
                Debug.DrawLine(bottomLeftBlock, topRightBlock - new Vector3Int(chunkSize, 0, 0), color, Mathf.Infinity);
                Debug.DrawLine(topRightBlock, bottomLeftBlock + new Vector3Int(0, chunkSize, 0), color, Mathf.Infinity);
                Debug.DrawLine(topRightBlock, bottomLeftBlock + new Vector3Int(chunkSize, 0, 0), color, Mathf.Infinity);
            }
        }
    }

    void AddCaveDetail()
    {
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0/*highestTiles[x] - 1 - caveStartingOffset*/; y < highestTiles[x] - 1; y++)
            {
                bgblockMap[x, y] = itemIDfromName["DirtWall"];
            }
        }

        var stoneId = itemIDfromName["Stone"];
        var dirtId = itemIDfromName["Dirt"];

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < highestTiles[x]; y++)
            {
                if (Random.Range(0, 100) > 10) { continue; }

                if (fgblockMap[x, y] == stoneId || fgblockMap[x, y] == dirtId)
                {
                    if (fgblockMap[x, y + 1] == 0)
                    {
                        fgblockMap[x, y + 1] = itemIDfromName["Grass"];
                    }
                }
            }
        }
    }

    void GenerateCaves()
    {
        if (usePerlinCaves)
        {
            var offset = new Vector2(Random.Range(0f, 99999f), Random.Range(0f, 99999f));

            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < highestTiles[x] - caveStartingOffset; y++)
                {
                    float xVal = ((x * (perlinCaveScale / 10)) + offset.x);
                    float yVal = ((y * (perlinCaveScale / 10)) + offset.y);

                    var noise = Mathf.PerlinNoise(xVal, yVal);

                    if (noise < caveSize)
                    {
                        fgblockMap[x, y] = 0;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < smoothIterations; i++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    for (int y = 0; y < highestTiles[x] - caveStartingOffset; y++)
                    {
                        //Randomly set tiles
                        if (i == 0)
                        {
                            if (Random.Range(0, 2) == 0)
                            {
                                fgblockMap[x, y] = 3;
                            }
                            else
                            {
                                fgblockMap[x, y] = 0;
                            }
                        }
                        //Create order from madness
                        else
                        {
                            if (x == 0 || y == 0 || y >= worldHeight - 1 || x >= worldWidth - 1)
                            {
                                fgblockMap[x, y] = 3;
                                continue;
                            }

                            int faces = 0;

                            if (fgblockMap[x - 1, y] != 0)
                            {
                                faces++;
                            }
                            if (fgblockMap[x + 1, y] != 0)
                            {
                                faces++;
                            }
                            if (fgblockMap[x, y + 1] != 0)
                            {
                                faces++;
                            }
                            if (fgblockMap[x, y - 1] != 0)
                            {
                                faces++;
                            }
                            if (fgblockMap[x - 1, y - 1] != 0)
                            {
                                faces++;
                            }
                            if (fgblockMap[x + 1, y + 1] != 0)
                            {
                                faces++;
                            }
                            if (fgblockMap[x - 1, y + 1] != 0)
                            {
                                faces++;
                            }
                            if (fgblockMap[x + 1, y - 1] != 0)
                            {
                                faces++;
                            }
                            if (faces < 4)
                            {
                                fgblockMap[x, y] = 0;
                            }
                            else if (faces > 4)
                            {
                                //if (Random.Range(0, 5) == 0)
                                //{
                                //    blockMap[x, y] = BlockType.dirt;
                                //}
                                //else
                                {
                                    fgblockMap[x, y] = 3;
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
                    fgblockMap[x, y] = 3;
                }
                else if (noise < .1f)
                {

                }
            }
        }
    }

    //Define chunks
    void AddChunks()
    {
        for (int x = 0; x < chunksOnX; x++)
            for (int y = 0; y < chunksOnY; y++)
            {
                var bottomLeftBlock = new Vector2Int((x * chunkSize), (y * chunkSize));

                Vector3Int[] positions = new Vector3Int[chunkSize * chunkSize];
                TileBase[] nullTiles = new TileBase[chunkSize * chunkSize];
                TileBase[] fgtiles = new TileBase[chunkSize * chunkSize];
                TileBase[] mgtiles = new TileBase[chunkSize * chunkSize];
                TileBase[] bgtiles = new TileBase[chunkSize * chunkSize];
                int iterations = 0;

                for (int wy = bottomLeftBlock.y; wy < bottomLeftBlock.y + chunkSize; wy++)
                    for (int wx = bottomLeftBlock.x; wx < bottomLeftBlock.x + chunkSize; wx++)
                    {
                        {
                            if (!WithinWorldBounds(wx, wy)) { continue; }

                            positions[iterations] = new Vector3Int(wx, wy, 0);
                            fgtiles[iterations] = itemData[(fgblockMap[wx, wy])].tileData.tile;
                            mgtiles[iterations] = itemData[(mgblockMap[wx, wy])].tileData.tile;
                            bgtiles[iterations] = itemData[(bgblockMap[wx, wy])].tileData.tile;
                            iterations++;
                        }
                    }

                Chunk chunk = new Chunk(bottomLeftBlock, positions, fgtiles, mgtiles, bgtiles, nullTiles);
                chunks.Add(new Vector2Int(x, y), chunk);
            }
    }

    Vector2Int WorldCoordinatesToNearestChunkCoordinates(Vector2 input)
    {
        Vector2Int chunkCoords = new Vector2Int((int)(input.x / chunkSize), (int)((int)input.y / chunkSize));
        return chunkCoords;
    }

    bool WithinWorldBounds(int x, int y)
    {
        return x >= 0 && x < worldWidth && y >= 0 && y < worldHeight;
    }

    void InitializeWorld()
    {
        Vector3 loadRange = new Vector2(viewDistance.x + (excessChunksToLoad * chunkSize), viewDistance.y + (excessChunksToLoad * chunkSize));
        Vector3 topRightLoadPosition = mainCamera.transform.position + loadRange;
        Vector3 bottomLeftLoadPosition = mainCamera.transform.position - loadRange;

        var topRightChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinates(topRightLoadPosition);
        var bottomLeftChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinates(bottomLeftLoadPosition);

        for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
        {
            for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
            {
                if (!WithinWorldBounds(x * chunkSize, y * chunkSize)) { continue; }
                Chunk chunk = chunks[new Vector2Int(x, y)];

                foregroundTilemap.SetTiles(chunk.positions, chunk.fgtiles);
                midgroundTilemap.SetTiles(chunk.positions, chunk.mgtiles);
                backgroundTilemap.SetTiles(chunk.positions, chunk.bgtiles);

                chunk.loaded = true;
            }
        }
    }

    List<Vector2Int> chunksToUnload;
    bool unloading;

    IEnumerator UnloadChunks()
    {
        unloading = true;
        yield return new WaitForEndOfFrame();

        while (chunksToUnload.Count > 0)
        {
            if (loadingNewChunks) { yield return new WaitForEndOfFrame(); }

            Chunk chunk = chunks[chunksToUnload[0]];

            if (!chunk.inUnloadQueue)
            {
                chunksToUnload.RemoveAt(0);
                continue;
            }

            if (chunk.loaded)
            {
                foregroundTilemap.SetTiles(chunk.positions, chunk.nullTiles);
                midgroundTilemap.SetTiles(chunk.positions, chunk.nullTiles);
                backgroundTilemap.SetTiles(chunk.positions, chunk.nullTiles);
            }

            chunk.loaded = false;
            chunk.inUnloadQueue = false;

            chunksToUnload.RemoveAt(0);

            //if (chunksToUnload.Count %1 == 0)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        unloading = false;
    }

    void LoadChunks()
    {
        if (!fillInWorld)
        {
            lastCameraPositionUpdate = mainCamera.transform.position;

            //Top right of player
            Vector3 loadRange = new Vector2(viewDistance.x + (excessChunksToLoad * chunkSize), viewDistance.y + (excessChunksToLoad * chunkSize));
            Vector3 topRightLoadPosition = mainCamera.transform.position + loadRange;
            Vector3 bottomLeftLoadPosition = mainCamera.transform.position - loadRange;

            var topRightChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinates(topRightLoadPosition);
            var bottomLeftChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinates(bottomLeftLoadPosition);

            //if (loadDirection.x == -1)
            {
                if (topRightChunkLoadCoordinate.x + 1 < chunksOnX)
                {
                    for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
                    {
                        //test if i need this heere
                        if (y < 0 || y >= chunksOnY) { continue; }

                        var key = new Vector2Int(topRightChunkLoadCoordinate.x + 1, y);

                        Chunk chunk = chunks[key];

                        if (!chunk.inUnloadQueue && chunk.loaded)
                        {
                            chunksToUnload.Add(key);
                            chunk.inUnloadQueue = true;
                        }
                    }
                }

                if (bottomLeftChunkLoadCoordinate.x >= 0)
                {
                    for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
                    {
                        //test if i need this heere
                        if (y < 0 || y >= chunksOnY) { continue; }

                        Chunk chunk = chunks[new Vector2Int(bottomLeftChunkLoadCoordinate.x, y)];

                        if (!chunk.loaded)
                        {
                            LoadChunk(chunk);
                        }
                        else
                        {
                            //it must be in the queue
                            chunk.inUnloadQueue = false;
                        }

                        chunk.loaded = true;
                    }
                }
                else
                {
                    //print("End of world not gonna generate (left)");
                }

            }
            //else
            {
                if (bottomLeftChunkLoadCoordinate.x - 1 >= 0)
                {
                    for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
                    {
                        if (y < 0 || y >= chunksOnY) { continue; }

                        var key = new Vector2Int(bottomLeftChunkLoadCoordinate.x - 1, y);

                        Chunk chunk = chunks[key];

                        if (!chunk.inUnloadQueue && chunk.loaded)
                        {
                            chunksToUnload.Add(key);
                            chunk.inUnloadQueue = true;
                        }
                    }
                }

                if (topRightChunkLoadCoordinate.x < chunksOnX)
                {
                    for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
                    {
                        if (y < 0 || y >= chunksOnY) { continue; }

                        Chunk chunk = chunks[new Vector2Int(topRightChunkLoadCoordinate.x, y)];

                        if (!chunk.loaded)
                        {
                            LoadChunk(chunk);
                        }
                        else
                        {
                            //it must be in the queue
                            chunk.inUnloadQueue = false;
                        }

                        chunk.loaded = true;
                    }
                }
                else
                {
                    //print("End of world not gonna generate (right)");
                }

            }

            //if (loadDirection.y == -1)
            {
                if (topRightChunkLoadCoordinate.y + 1 < chunksOnY)
                {
                    for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
                    {
                        if (x < 0 || x >= chunksOnX) { continue; }

                        var key = new Vector2Int(x, topRightChunkLoadCoordinate.y + 1);

                        Chunk chunk = chunks[key];

                        if (!chunk.inUnloadQueue && chunk.loaded)
                        {
                            chunksToUnload.Add(key);
                            chunk.inUnloadQueue = true;
                        }
                    }
                }

                if (bottomLeftChunkLoadCoordinate.y >= 0)
                {
                    for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
                    {
                        //test with and without this check
                        if (x < 0 || x >= chunksOnX) { continue; }

                        Chunk chunk = chunks[new Vector2Int(x, bottomLeftChunkLoadCoordinate.y)];

                        if (!chunk.loaded)
                        {
                            LoadChunk(chunk);
                        }
                        else
                        {
                            chunk.inUnloadQueue = false;
                        }

                        chunk.loaded = true;
                    }
                }
                else
                {
                    //print("End of world not gonna generate (bottom)");
                }

            }
            //else
            {
                if (bottomLeftChunkLoadCoordinate.y - 1 >= 0)
                {
                    for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
                    {
                        if (x < 0 || x >= chunksOnX) { continue; }

                        var key = new Vector2Int(x, bottomLeftChunkLoadCoordinate.y - 1);

                        Chunk chunk = chunks[key];

                        if (!chunk.inUnloadQueue && chunk.loaded)
                        {
                            chunksToUnload.Add(key);
                            chunk.inUnloadQueue = true;
                        }
                    }
                }

                if (topRightChunkLoadCoordinate.y < chunksOnY)
                {
                    for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
                    {
                        //test with and without this check
                        if (x < 0 || x >= chunksOnX) { continue; }

                        Chunk chunk = chunks[new Vector2Int(x, topRightChunkLoadCoordinate.y)];

                        if (!chunk.loaded)
                        {
                            LoadChunk(chunk);
                        }
                        else
                        {
                            chunk.inUnloadQueue = false;
                        }

                        chunk.loaded = true;
                    }
                }
                else
                {
                    //print("End of world not gonna generate (top)");
                }
            }

            //BoundsInt boundsInt = new BoundsInt();
        }
        //Show entire world for debugging
        else
        {
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    if (x >= worldWidth)
                    {
                        break;
                    }

                    foregroundTilemap.SetTile(new Vector3Int(x, y, 0), itemData[(fgblockMap[x, y])].tileData.tile);
                }
            }
        }

    }

    void LoadChunk(Chunk chunk)
    {
        foregroundTilemap.SetTiles(chunk.positions, chunk.fgtiles);
        midgroundTilemap.SetTiles(chunk.positions, chunk.mgtiles);
        backgroundTilemap.SetTiles(chunk.positions, chunk.bgtiles);
    }

    Vector3 lastCameraPositionUpdate;
    Vector2Int loadDirection;
    Rigidbody2D playerRb;
    bool loadingNewChunks;

    void Update()
    {
        loadingNewChunks = false;

        if (Vector3.Distance(mainCamera.transform.position, lastCameraPositionUpdate) >= chunkSize / 2f)
        {
            //var pos = mainCamera.transform.position;
            //loadDirection = Vector2Int.zero;
            //if (lastCameraPositionUpdate.x > pos.x)
            //{
            //    if (lastCameraPositionUpdate.y < pos.y)
            //    {
            //        loadDirection = new Vector2Int(-1, 1);
            //    }
            //    else if (lastCameraPositionUpdate.y > pos.y)
            //    {
            //        loadDirection = new Vector2Int(-1, -1);
            //    }
            //}
            //else if (lastCameraPositionUpdate.x < pos.x)
            //{
            //    if (lastCameraPositionUpdate.y > pos.y)
            //    {
            //        loadDirection = new Vector2Int(1, 1);
            //    }
            //    else if (lastCameraPositionUpdate.y > pos.y)
            //    {
            //        loadDirection = new Vector2Int(1, -1);
            //    }
            //}

            loadingNewChunks = true;
            LoadChunks();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }

        if (chunksToUnload.Count > 0 && !unloading)
        {
            StartCoroutine(UnloadChunks());
        }
    }
}
