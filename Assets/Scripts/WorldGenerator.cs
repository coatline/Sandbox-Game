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
    [SerializeField] ExtendedRuleTile treeStumpBL;
    [SerializeField] ExtendedRuleTile treeStumpBR;
    [SerializeField] ExtendedRuleTile treeStumpM;
    [SerializeField] Tile treeTrunk;
    [SerializeField] Tile treeLeaves;
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
    [Range(.01f, .4f)]
    [SerializeField] float caveSize;
    [Range(.1f, 5)]
    [SerializeField] float perlinCaveScale;

    [Header("Terrain Settings")]
    [Range(.1f, 5)]
    [SerializeField] float terrainScale;
    [SerializeField] float flowerScale;
    [SerializeField] float maxTerrainHeight;
    [SerializeField] bool displayChunks;
    [SerializeField] bool fillInWorld;
    int terrainStartingHeight;
    int terrainWidth;

    [Header("References")]
    [SerializeField] Tilemap blockTilemap;
    [SerializeField] Tilemap midgroundTilemap;
    [SerializeField] Tilemap backgroundTilemap;
    [SerializeField] ExtendedRuleTile grassTile;
    [SerializeField] ExtendedRuleTile dirtTile;
    [SerializeField] ExtendedRuleTile stoneTile;
    [SerializeField] ExtendedRuleTile blueTorch;
    [SerializeField] ExtendedRuleTile redTorch;
    [SerializeField] ExtendedRuleTile greenTorch;
    [SerializeField] Tile redFlowerTile;
    [SerializeField] Tile yellowFlowerTile;
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
    List<TreeData> trees;
    Dictionary<Vector2Int, Chunk> chunks;
    public List<int> highestTiles;
    //0air1grass2dirt3stone4wood5iron6redtorch7greentorch8bluetorch9torch
    public byte[,] blockMap;

    void Awake()
    {
        chunksOnX = Mathf.CeilToInt(((float)worldWidth / chunkSize));
        chunksOnY = Mathf.CeilToInt(((float)worldHeight / chunkSize));

        viewDistance.x += chunkSize / 2;
        viewDistance.y += chunkSize / 2;

        terrainWidth = worldWidth;
        terrainStartingHeight = worldHeight / 2;

        mainCamera = Camera.main;
         
        viewDistance = new Vector3((mainCamera.orthographicSize * mainCamera.aspect), mainCamera.orthographicSize, 0);
        blockMap = new byte[worldWidth, worldHeight];

        chunks = new Dictionary<Vector2Int, Chunk>();
        chunksToUnload = new List<Vector2Int>();
        highestTiles = new List<int>();
        trees = new List<TreeData>();

        GenerateInitalTerrain();

        GenerateTrees();

        //GenerateFlowers();

        //GenerateOres();

        //GenerateCaves();

        AddChunks();

        SpawnPlayer();

        InitializeWorld();
    }

    //Generate height map for terrain
    void GenerateInitalTerrain()
    {
        float xOffset = Random.Range(0f, 99999f);

        for (int x = 0; x < terrainWidth; x++)
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
                    blockMap[x, y] = 3;
                }
                else
                {
                    if ((surfaceTerrainHeight + terrainStartingHeight) - y > sprinkleStoneStartOffset && Random.Range(0, 3) == 0)
                    {
                        blockMap[x, y] = 3;
                    }
                    else
                    {
                        //index out of array error

                        if (y == surfaceTerrainHeight + terrainStartingHeight)
                        {
                            blockMap[x, y] = 1;
                        }
                        else
                        {
                            blockMap[x, y] = 2;
                        }
                    }
                }
            }
        }
    }

    public void ModifyBlock(int x, int y, byte blockType)
    {
        if (x >= worldWidth || x < 0 || y < 0 || y >= worldHeight) { return; }

        var belowBlock = new Vector3Int(x, y - 1, 0);

        if (blockTilemap.GetTile(belowBlock) == grassTile)
        {
            blockTilemap.SetTile(belowBlock, dirtTile);
            blockMap[belowBlock.x, belowBlock.y] = 2;
        }

        blockTilemap.SetTile(new Vector3Int(x, y, 0), TileFromBlockType(blockType));
        blockMap[x, y] = blockType;
    }

    void GenerateTrees()
    {
        for (int x = 2; x < terrainWidth - 3; x += 3)
        {
            //do not put on uneven block
            if (blockMap[x - 1, highestTiles[x]] == 0 || blockMap[x + 1, highestTiles[x]] == 0)
            {
                continue;
            }

            if (Random.Range(0f, 100f) <= chanceForTreeToSpawn)
            {
                int treeHeight = Random.Range(5, maxTreeHeight + 1);
                bool spawnLeftStump = true;
                bool spawnRightStump = true;

                if (blockMap[x - 2, highestTiles[x]] == 0)
                {
                    spawnLeftStump = false;
                }
                if (blockMap[x + 2, highestTiles[x]] == 0)
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

                trees.Add(new TreeData(treeHeight, startingPosition, spawnLeftStump, spawnRightStump, false));
            }
        }
    }

    void DisplayTree(int treeHeight, bool spawnLeftStump, bool spawnRightStump, Vector3Int startingPosition)
    {
        for (int i = 0; i < treeHeight; i++)
        {
            if (i == 0)
            {
                midgroundTilemap.SetTile(startingPosition, treeStumpM);

                if (spawnLeftStump)
                {
                    midgroundTilemap.SetTile(startingPosition + new Vector3Int(-1, 0, 0), treeStumpBL);
                }

                if (spawnRightStump)
                {
                    midgroundTilemap.SetTile(startingPosition + new Vector3Int(1, 0, 0), treeStumpBR);
                }
            }
            else if (treeHeight - i > 1)
            {
                midgroundTilemap.SetTile(startingPosition + new Vector3Int(0, i, 0), treeTrunk);
            }
            else
            {
                midgroundTilemap.SetTile(startingPosition + new Vector3Int(0, i, 0), treeLeaves);
            }

        }
    }
    void HideTree(int treeHeight, bool spawnLeftStump, bool spawnRightStump, Vector3Int startingPosition)
    {
        for (int i = 0; i < treeHeight; i++)
        {
            if (i == 0)
            {
                midgroundTilemap.SetTile(startingPosition, null);

                if (spawnLeftStump)
                {
                    midgroundTilemap.SetTile(startingPosition + new Vector3Int(-1, 0, 0), null);
                }

                if (spawnRightStump)
                {
                    midgroundTilemap.SetTile(startingPosition + new Vector3Int(1, 0, 0), null);
                }
            }
            else
            {
                midgroundTilemap.SetTile(startingPosition + new Vector3Int(0, i, 0), null);
            }

        }
    }

    void GenerateFlowers()
    {
        float xOffset = Random.Range(0f, 99999f);

        for (int x = 0; x < terrainWidth; x++)
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

    void GenerateCaves()
    {
        if (usePerlinCaves)
        {
            var offset = new Vector2(Random.Range(0f, 99999f), Random.Range(0f, 99999f));

            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < highestTiles[x] - caveStartingOffset; y++)
                {
                    float xVal = ((x * (perlinCaveScale / 10)) + offset.x);
                    float yVal = ((y * (perlinCaveScale / 10)) + offset.y);

                    var noise = Mathf.PerlinNoise(xVal, yVal);

                    if (noise < caveSize)
                    {
                        //if (y < 0 || y >= worldHeight || x < 0 || x >= terrainWidth) { print("INVALIDE POSITION"); break; }
                        blockMap[x, y] = 0;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < smoothIterations; i++)
            {
                for (int x = 0; x < terrainWidth; x++)
                {
                    for (int y = 0; y < highestTiles[x] - caveStartingOffset; y++)
                    {
                        //Randomly set tiles
                        if (i == 0)
                        {
                            if (Random.Range(0, 2) == 0)
                            {
                                blockMap[x, y] = 3;
                            }
                            else
                            {
                                blockMap[x, y] = 0;
                            }
                        }
                        //Create order from madness
                        else
                        {
                            if (x == 0 || y == 0 || y >= worldHeight - 1 || x >= worldWidth - 1)
                            {
                                blockMap[x, y] = 3;
                                continue;
                            }

                            int faces = 0;

                            if (blockMap[x - 1, y] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x + 1, y] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x, y + 1] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x, y - 1] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x - 1, y - 1] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x + 1, y + 1] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x - 1, y + 1] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x + 1, y - 1] != 0)
                            {
                                faces++;
                            }
                            if (faces < 4)
                            {
                                blockMap[x, y] = 0;
                            }
                            else if (faces > 4)
                            {
                                //if (Random.Range(0, 5) == 0)
                                //{
                                //    blockMap[x, y] = BlockType.dirt;
                                //}
                                //else
                                {
                                    blockMap[x, y] = 3;
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

        for (int x = 0; x < terrainWidth; x++)
        {
            for (int y = 0; y < worldHeight - caveStartingOffset; y++)
            {
                var noise = Mathf.PerlinNoise((float)((x + offset.x) * perlinCaveScale / terrainStartingHeight - caveStartingOffset), (y + offset.y) * perlinCaveScale / (terrainStartingHeight - caveStartingOffset));

                if (noise < .4f)
                {
                    blockMap[x, y] = 3;
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
                TileBase[] tiles = new TileBase[chunkSize * chunkSize];
                int iterations = 0;

                for (int wx = topLeftBlock.x; wx < topLeftBlock.x + chunkSize; wx++)
                {
                    for (int wy = topLeftBlock.y; wy < topLeftBlock.y + chunkSize; wy++)
                    {
                        if (!WithinWorldBounds(wx, wy)) { continue; }

                        positions[iterations] = new Vector3Int(wx, wy, 0);
                        tiles[iterations] = TileFromBlockType(blockMap[wx, wy]);
                        //nullTiles[iterations] = null;
                        iterations++;
                    }
                }

                Chunk chunk = new Chunk(topLeftBlock, positions, tiles, nullTiles);
                chunks.Add(new Vector2Int(x, y), chunk);
            }
    }

    Vector2Int WorldCoordinatesToNearestChunkCoordinate(Vector2 input)
    {
        Vector2Int chunkCoords = new Vector2Int(Mathf.FloorToInt(input.x / chunkSize), Mathf.FloorToInt((int)input.y / chunkSize));
        return chunkCoords;
    }

    bool WithinWorldBounds(int x, int y)
    {
        return x >= 0 && x < worldWidth && y >= 0 && y < worldHeight;
    }

    void LoadTrees(Vector3 bottomLeftLoadPosition, Vector3 topRightLoadPosition)
    {
        //SHOULD DO THIS IN A MORE OPTIMIZED WAY
        //Load and Unload Trees
        for (int k = 0; k < trees.Count; k++)
        {
            TreeData tree = trees[k];

            //Tree is active 
            if ((tree.trunkPosition.x <= bottomLeftLoadPosition.x || tree.trunkPosition.x >= topRightLoadPosition.x))
            {
                if (tree.isActive)
                {
                    //Hide tree it is out of view
                    HideTree(tree.height, tree.spawnLeftTrunk, tree.spawnRightTrunk, tree.trunkPosition);
                    tree.isActive = false;
                }
            }
            else if (!tree.isActive)
            {
                //Display tree it is in view
                DisplayTree(tree.height, tree.spawnLeftTrunk, tree.spawnRightTrunk, tree.trunkPosition);
                tree.isActive = true;
            }
        }
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
                Chunk chunk = chunks[new Vector2Int(x, y)];

                blockTilemap.SetTiles(chunk.positions, chunk.tiles);

                chunk.loaded = true;
            }
        }
    }

    List<Vector2Int> chunksToUnload;
    bool unloading;

    IEnumerator UnloadChunks()
    {
        unloading = true;

        while (chunksToUnload.Count > 0)
        {
            Chunk chunk = chunks[chunksToUnload[0]];

            if (!chunk.inUnloadQueue)
            {
                chunksToUnload.RemoveAt(0);
                continue;
            }

            if (chunk.loaded)
            {
                blockTilemap.SetTiles(chunk.positions, chunk.nullTiles);
            }

            chunk.loaded = false;
            chunk.inUnloadQueue = false;

            chunksToUnload.RemoveAt(0);

            //if (chunksToUnload.Count > 75)
            //{
            //    if (chunksToUnload.Count % 2 == 0)
            //    {
            //        yield return new WaitForEndOfFrame();
            //    }
            //}
            //else
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

            if (loadDirection.x == -1)
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
                        blockTilemap.SetTiles(chunk.positions, chunk.tiles);
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
                        blockTilemap.SetTiles(chunk.positions, chunk.tiles);
                    }
                    else
                    {
                        //it must be in the queue
                        chunk.inUnloadQueue = false;
                    }

                    chunk.loaded = true;
                }
            }

            if (loadDirection.y == -1)
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
                
                    Chunk dchunk = chunks[new Vector2Int(6, bottomLeftChunkLoadCoordinate.y)];
                        blockTilemap.SetTiles(dchunk.positions, dchunk.nullTiles);
                print(bottomLeftChunkLoadCoordinate);
                if (bottomLeftChunkLoadCoordinate.y < 0) { print("End of world not gonna generate (bottom)"); return; }

                for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
                {
                    //test with and without this check
                    if (x < 0 || x >= chunksOnX) { continue; }

                    Chunk chunk = chunks[new Vector2Int(x, bottomLeftChunkLoadCoordinate.y)];

                    if (!chunk.loaded)
                    {
                        blockTilemap.SetTiles(chunk.positions, chunk.tiles);
                    }
                    else
                    {
                        chunk.inUnloadQueue = false;
                    }

                    chunk.loaded = true;
                }
                //print(bottomLeftChunkLoadCoordinate.y);
                //if (bottomLeftChunkLoadCoordinate.y == 0) { Chunk chunk = chunks[new Vector2Int(7, bottomLeftChunkLoadCoordinate.y)]; blockTilemap.SetTiles(chunk.positions, chunk.nullTiles) ; }
            }
            else
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
                        blockTilemap.SetTiles(chunk.positions, chunk.tiles);
                    }
                    else
                    {
                        chunk.inUnloadQueue = false;
                    }

                    chunk.loaded = true;
                }
            }

            LoadTrees(bottomLeftLoadPosition, topRightLoadPosition);
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

                    blockTilemap.SetTile(new Vector3Int(x, y, 0), TileFromBlockType(blockMap[x, y]));
                }
            }
        }

    }


    Vector3 lastCameraPositionUpdate;
    Vector2Int loadDirection;
    Rigidbody2D playerRb;

    void Update()
    {
        if (Vector3.Distance(mainCamera.transform.position, lastCameraPositionUpdate) >= chunkSize / 3f)
        {
            if (playerRb.velocity.x < 0)
            {
                if (playerRb.velocity.y < 0)
                {
                    loadDirection = new Vector2Int(-1, -1);
                }
                else if (playerRb.velocity.y > 0)
                {
                    loadDirection = new Vector2Int(-1, 1);
                }
            }
            else
            {
                if (playerRb.velocity.y < 0)
                {
                    loadDirection = new Vector2Int(1, -1);
                }
                else if (playerRb.velocity.y > 0)
                {
                    loadDirection = new Vector2Int(1, 1);
                }
            }

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

    RuleTile TileFromBlockType(byte blockType)
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
            //redtorch
            //4wood5iron
            case 6: return redTorch;
            //greentorch
            case 7: return greenTorch;
            //bluetorch
            case 8: return blueTorch;
            //normal torch
            case 9: return greenTorch;
        }

        return null;
    }
}
