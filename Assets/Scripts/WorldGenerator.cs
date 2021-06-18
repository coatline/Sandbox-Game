using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;

//public enum BlockType
//{
//    air,
//    grass,
//    dirt,
//    stone,
//    wood,
//    iron,
//    redtorch,
//    greentorch,
//    bluetorch,
//    torch
//}

public class WorldGenerator : MonoBehaviour
{
    [Header("Tree Tile References")]
    [SerializeField] RuleTile treeStumpBL;
    [SerializeField] RuleTile treeStumpBR;
    [SerializeField] RuleTile treeStumpM;
    [SerializeField] RuleTile treeTrunk;
    [SerializeField] RuleTile treeLeaves;
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
    [SerializeField] Tilemap blockTilemap;
    [SerializeField] Tilemap midgroundTilemap;
    [SerializeField] Tilemap backgroundTilemap;
    [SerializeField] RuleTile grassTile;
    [SerializeField] RuleTile dirtTile;
    [SerializeField] RuleTile stoneTile;
    [SerializeField] RuleTile torch;
    [SerializeField] RuleTile blueTorch;
    [SerializeField] RuleTile redTorch;
    [SerializeField] RuleTile greenTorch;
    [SerializeField] RuleTile dirtWall;
    [SerializeField] RuleTile redFlowerTile;
    [SerializeField] RuleTile yellowFlowerTile;
    [SerializeField] Player playerPrefab;

    [Header("Settings")]
    [SerializeField] int excessChunksToLoad;
    [Range(0, 100f)]
    [SerializeField] float chanceForTreeToSpawn;
    [Range(6, 20f)]
    [SerializeField] int maxTreeHeight;
    int chunksOnX;
    int chunksOnY;

    Vector3 viewDistance;
    Dictionary<Vector2Int, Chunk> chunks;
    public List<int> highestTiles;
    //0air1grass2dirt3stone4wood5iron6redtorch7greentorch8bluetorch9torch
    public byte[,] fgblockMap;
    public byte[,] mgblockMap;
    public byte[,] bgblockMap;

    void Awake()
    {
        chunksOnX = Mathf.CeilToInt(((float)worldWidth / chunkSize));
        chunksOnY = Mathf.CeilToInt(((float)worldHeight / chunkSize));

        viewDistance.x += chunkSize / 2;
        viewDistance.y += chunkSize / 2;

        terrainStartingHeight = worldHeight / 2;

        mainCamera = Camera.main;

        viewDistance = new Vector3((mainCamera.orthographicSize * mainCamera.aspect), mainCamera.orthographicSize, 0);
        fgblockMap = new byte[worldWidth, worldHeight];
        mgblockMap = new byte[worldWidth, worldHeight];
        bgblockMap = new byte[worldWidth, worldHeight];

        chunks = new Dictionary<Vector2Int, Chunk>();
        chunksToUnload = new List<Vector2Int>();
        highestTiles = new List<int>();

        GenerateInitalTerrain();

        GenerateTrees();

        //GenerateFlowers();

        //GenerateOres();

        GenerateCaves();

        AddCaveDetail();

        AddChunks();

        SpawnPlayer();

        InitializeWorld();
    }

    //Generate height map for terrain
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
                    fgblockMap[x, y] = 3;
                }
                else
                {
                    if ((surfaceTerrainHeight + terrainStartingHeight) - y > sprinkleStoneStartOffset && Random.Range(0, 3) == 0)
                    {
                        fgblockMap[x, y] = 3;
                    }
                    else
                    {
                        //index out of array error

                        if (y == surfaceTerrainHeight + terrainStartingHeight)
                        {
                            fgblockMap[x, y] = 1;
                        }
                        else
                        {
                            fgblockMap[x, y] = 2;
                        }
                    }
                }
            }
        }
    }

    public bool blockModified;

    public void ModifyBlockbg(int x, int y, byte blockType)
    {
        if (x >= worldWidth || x < 0 || y < 0 || y >= worldHeight) { return; }

        //backgroundTilemap.SetTile(new Vector3Int(x, y, 0), bgTileFromBlockType(blockType));
        bgblockMap[x, y] = blockType;
        //chunks[WorldCoordinatesToNearestChunkCoordinate(new Vector2(x,y))].bgtiles[]
    }

    public void ModifyBlockmg(int x, int y, byte blockType)
    {
        if (x >= worldWidth || x < 0 || y < 0 || y >= worldHeight) { return; }

        midgroundTilemap.SetTile(new Vector3Int(x, y, 0), mgTileFromBlockType(blockType));
        mgblockMap[x, y] = blockType;
    }

    public void ModifyBlockfg(int x, int y, byte blockType)
    {
        if (x >= worldWidth || x < 0 || y < 0 || y >= worldHeight) { return; }

        var belowBlock = new Vector3Int(x, y - 1, 0);

        if (blockTilemap.GetTile(belowBlock) == grassTile)
        {
            blockTilemap.SetTile(belowBlock, dirtTile);
            fgblockMap[belowBlock.x, belowBlock.y] = 2;
        }

        blockTilemap.SetTile(new Vector3Int(x, y, 0), fgTileFromBlockType(blockType));
        fgblockMap[x, y] = blockType;
        blockModified = true;
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
                int treeHeight = Random.Range(5, maxTreeHeight + 1);
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

                Vector3Int startingPosition = new Vector3Int(x, highestTiles[x] + 1, 0);
                GenerateTree(treeHeight, spawnLeftStump, spawnRightStump, startingPosition);
            }
        }
    }

    void GenerateTree(int treeHeight, bool spawnLeftStump, bool spawnRightStump, Vector3Int startingPosition)
    {
        for (int i = 0; i < treeHeight; i++)
        {
            if (i == 0)
            {
                //treestumpM
                mgblockMap[startingPosition.x, startingPosition.y] = 5;

                if (spawnLeftStump)
                {
                    //treestumpBL
                    mgblockMap[startingPosition.x - 1, startingPosition.y] = 6;
                }

                if (spawnRightStump)
                {
                    //treestumpBR
                    mgblockMap[startingPosition.x + 1, startingPosition.y] = 7;
                }
            }
            else if (treeHeight - i > 1)
            {
                //treeTRUCNK
                mgblockMap[startingPosition.x, startingPosition.y + i] = 8;
            }
            else
            {
                //treeleaves
                mgblockMap[startingPosition.x, startingPosition.y + i] = 9;
            }

        }
    }

    void GenerateFlowers()
    {
        float xOffset = Random.Range(0f, 99999f);

        for (int x = 0; x < worldWidth; x++)
        {
            if (blockTilemap.GetTile(new Vector3Int(x + 1, highestTiles[x], 0)) == null || blockTilemap.GetTile(new Vector3Int(x - 1, highestTiles[x], 0)) == null)
            {
                continue;
            }

            float xVal = ((x * (terrainScale / 100)) + xOffset);
            float normalnoise = Mathf.PerlinNoise(xVal, xOffset);

            if (Random.Range(0f, normalnoise) <= .02f)
            {
                if (Random.Range(0, 2) == 0)
                {

                    midgroundTilemap.SetTile(new Vector3Int(x, highestTiles[x] + 1, 0), redFlowerTile);
                }
                else
                {
                    midgroundTilemap.SetTile(new Vector3Int(x, highestTiles[x] + 1, 0), yellowFlowerTile);
                }
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

        player = Instantiate(playerPrefab, new Vector3(worldWidth / 2, highestTiles[worldWidth / 2] + 3, 0), Quaternion.identity);
        playerRb = player.GetComponent<Rigidbody2D>();

        player.blockTilemap = blockTilemap;
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
                Vector3Int bottomRightBlock = new Vector3Int(chunks[chunkPos].topLeftBlock.x + chunkSize, chunks[chunkPos].topLeftBlock.y - chunkSize, 0);
                Vector3Int topLeftBlock = new Vector3Int(chunks[chunkPos].topLeftBlock.x, chunks[chunkPos].topLeftBlock.y, 0);

                Debug.DrawLine(bottomRightBlock, bottomRightBlock + new Vector3Int(0, chunkSize, 0), color, Mathf.Infinity);
                Debug.DrawLine(bottomRightBlock, bottomRightBlock - new Vector3Int(chunkSize, 0, 0), color, Mathf.Infinity);
                Debug.DrawLine(topLeftBlock, topLeftBlock - new Vector3Int(0, chunkSize, 0), color, Mathf.Infinity);
                Debug.DrawLine(topLeftBlock, topLeftBlock + new Vector3Int(chunkSize, 0, 0), color, Mathf.Infinity);

            }
        }
    }

    void AddCaveDetail()
    {
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < highestTiles[x]; y++)
            {
                bgblockMap[x, y] = 1;
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
                var topLeftBlock = new Vector2Int((x * chunkSize), (y * chunkSize) + chunkSize);

                Vector3Int[] positions = new Vector3Int[chunkSize * chunkSize];
                TileBase[] nullTiles = new TileBase[chunkSize * chunkSize];
                TileBase[] fgtiles = new TileBase[chunkSize * chunkSize];
                TileBase[] mgtiles = new TileBase[chunkSize * chunkSize];
                TileBase[] bgtiles = new TileBase[chunkSize * chunkSize];
                int iterations = 0;

                for (int wx = topLeftBlock.x; wx < topLeftBlock.x + chunkSize; wx++)
                {
                    for (int wy = topLeftBlock.y - chunkSize; wy < topLeftBlock.y; wy++)
                    {
                        if (!WithinWorldBounds(wx, wy)) { continue; }

                        positions[iterations] = new Vector3Int(wx, wy, 0);
                        fgtiles[iterations] = fgTileFromBlockType(fgblockMap[wx, wy]);
                        mgtiles[iterations] = mgTileFromBlockType(mgblockMap[wx, wy]);
                        bgtiles[iterations] = bgTileFromBlockType(bgblockMap[wx, wy]);
                        //nullTiles[iterations] = null;
                        iterations++;
                    }
                }

                Chunk chunk = new Chunk(topLeftBlock, positions, fgtiles, mgtiles, bgtiles, nullTiles);
                chunks.Add(new Vector2Int(x, y), chunk);
            }
    }

    Vector2Int WorldCoordinatesToNearestChunkCoordinate(Vector2 input)
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

        var topRightChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinate(topRightLoadPosition);
        var bottomLeftChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinate(bottomLeftLoadPosition);

        for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
        {
            for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
            {
                if (!WithinWorldBounds(x * chunkSize, y * chunkSize)) { continue; }
                Chunk chunk = chunks[new Vector2Int(x, y)];

                blockTilemap.SetTiles(chunk.positions, chunk.fgtiles);
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
                blockTilemap.SetTiles(chunk.positions, chunk.nullTiles);
                midgroundTilemap.SetTiles(chunk.positions, chunk.nullTiles);
                backgroundTilemap.SetTiles(chunk.positions, chunk.nullTiles);
            }

            chunk.loaded = false;
            chunk.inUnloadQueue = false;

            chunksToUnload.RemoveAt(0);

            if (chunksToUnload.Count % 2 == 0)
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

            var topRightChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinate(topRightLoadPosition);
            var bottomLeftChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinate(bottomLeftLoadPosition);

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

                if (bottomLeftChunkLoadCoordinate.x < 0) { print("End of world not gonna generate (left)"); return; }

                for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
                {
                    //test if i need this heere
                    if (y < 0 || y >= chunksOnY) { continue; }

                    Chunk chunk = chunks[new Vector2Int(bottomLeftChunkLoadCoordinate.x, y)];

                    if (!chunk.loaded)
                    {
                        blockTilemap.SetTiles(chunk.positions, chunk.fgtiles);
                        midgroundTilemap.SetTiles(chunk.positions, chunk.mgtiles);
                        backgroundTilemap.SetTiles(chunk.positions, chunk.bgtiles);
                    }
                    else
                    {
                        //it must be in the queue
                        chunk.inUnloadQueue = false;
                    }

                    chunk.loaded = true;
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

                if (topRightChunkLoadCoordinate.x >= chunksOnX) { print("End of world not gonna generate (right)"); return; }

                for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
                {
                    if (y < 0 || y >= chunksOnY) { continue; }

                    Chunk chunk = chunks[new Vector2Int(topRightChunkLoadCoordinate.x, y)];

                    if (!chunk.loaded)
                    {
                        blockTilemap.SetTiles(chunk.positions, chunk.fgtiles);
                        midgroundTilemap.SetTiles(chunk.positions, chunk.mgtiles);
                        backgroundTilemap.SetTiles(chunk.positions, chunk.bgtiles);
                    }
                    else
                    {
                        //it must be in the queue
                        chunk.inUnloadQueue = false;
                    }

                    chunk.loaded = true;
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

                if (bottomLeftChunkLoadCoordinate.y < 0) { print("End of world not gonna generate (bottom)"); return; }

                for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
                {
                    //test with and without this check
                    if (x < 0 || x >= chunksOnX) { continue; }

                    Chunk chunk = chunks[new Vector2Int(x, bottomLeftChunkLoadCoordinate.y)];

                    if (!chunk.loaded)
                    {
                        blockTilemap.SetTiles(chunk.positions, chunk.fgtiles);
                        midgroundTilemap.SetTiles(chunk.positions, chunk.mgtiles);
                        backgroundTilemap.SetTiles(chunk.positions, chunk.bgtiles);
                    }
                    else
                    {
                        chunk.inUnloadQueue = false;
                    }

                    chunk.loaded = true;
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

                if (topRightChunkLoadCoordinate.y >= chunksOnY) { print("End of world not gonna generate (top)"); return; }

                for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
                {
                    //test with and without this check
                    if (x < 0 || x >= chunksOnX) { continue; }

                    Chunk chunk = chunks[new Vector2Int(x, topRightChunkLoadCoordinate.y)];
                    if (!chunk.loaded)
                    {
                        blockTilemap.SetTiles(chunk.positions, chunk.fgtiles);
                        midgroundTilemap.SetTiles(chunk.positions, chunk.mgtiles);
                        backgroundTilemap.SetTiles(chunk.positions, chunk.bgtiles);
                    }
                    else
                    {
                        chunk.inUnloadQueue = false;
                    }

                    chunk.loaded = true;
                }
            }

            //LoadTrees(bottomLeftLoadPosition, topRightLoadPosition);
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

                    blockTilemap.SetTile(new Vector3Int(x, y, 0), fgTileFromBlockType(fgblockMap[x, y]));
                }
            }
        }

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

    RuleTile bgTileFromBlockType(byte blockType)
    {
        switch (blockType)
        {
            //air
            case 0: return null;
            //dirt wall
            case 1: return dirtWall;
        }

        return null;
    }

    RuleTile mgTileFromBlockType(byte blockType)
    {
        switch (blockType)
        {
            //air
            case 0: return null;
            //redtorch
            case 1: return redTorch;
            //greentorch
            case 2: return greenTorch;
            //bluetorch
            case 3: return blueTorch;
            //normal torch
            case 4: return torch;
            //treestumpMiddle
            case 5: return treeStumpM;
            //treesstumpLeft
            case 6: return treeStumpBL;
            //treesstumpRight
            case 7: return treeStumpBR;
            //treeTrunk
            case 8: return treeTrunk;
            //Tree leavess
            case 9: return treeLeaves;
        }

        return null;
    }

    RuleTile fgTileFromBlockType(byte blockType)
    {
        switch (blockType)
        {
            //air
            case 0: return null;
            //grass
            case 1: return grassTile;
            //dirt
            case 2: return dirtTile;
            //stone
            case 3: return stoneTile;
                //4wood5iron
        }
        return null;
    }
}
