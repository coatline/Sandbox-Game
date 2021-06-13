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
    [SerializeField] Tile treeTrunk;
    [SerializeField] Tile treeLeaves;
    Camera mainCamera;
    TestPlayer player;

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
    [SerializeField] RuleTile grassTile;
    [SerializeField] RuleTile dirtTile;
    [SerializeField] RuleTile stoneTile;
    [SerializeField] RuleTile blueTorch;
    [SerializeField] RuleTile redTorch;
    [SerializeField] RuleTile greenTorch;
    [SerializeField] Tile redFlowerTile;
    [SerializeField] Tile yellowFlowerTile;
    [SerializeField] TestPlayer playerPrefab;


    [Header("Settings")]
    [SerializeField] int excessChunksToLoad;
    [Range(0, 100f)]
    [SerializeField] float chanceForTreeToSpawn;
    [Range(6, 20f)]
    [SerializeField] int maxTreeHeight;


    Vector3 viewDistance;
    List<TreeData> trees;
    Dictionary<Vector2Int, Chunk> chunks;
    public List<int> highestTiles;
    //0air1grass2dirt3stone4wood5iron6redtorch7greentorch8bluetorch9torch
    public byte[,] blockMap;

    void Awake()
    {
        viewDistance.x += chunkSize / 2;
        viewDistance.y += chunkSize / 2;

        terrainWidth = worldWidth;
        terrainStartingHeight = worldHeight / 2;

        mainCamera = Camera.main;

        viewDistance = new Vector3((mainCamera.orthographicSize * 1.78f), mainCamera.orthographicSize + .01f, 0);
        blockMap = new byte[worldWidth, worldHeight];
        chunks = new Dictionary<Vector2Int, Chunk>();
        highestTiles = new List<int>();
        trees = new List<TreeData>();

        AddChunks();
        GenerateInitalTerrain();
        GenerateTrees();
        //GenerateFlowers();
        //GenerateOres();
        GenerateCaves();
        SpawnPlayer();
        LoadChunks();
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
        topright.transform.position = new Vector3(worldWidth - ((worldWidth % chunkSize)), worldHeight);
        var bottomleft = new GameObject("Bottom Left Camera Barrier");
        bottomleft.transform.position = new Vector3(0, 0);

        topright.transform.SetParent(camerabarriers.transform);
        bottomleft.transform.SetParent(camerabarriers.transform);

        player = Instantiate(playerPrefab, new Vector3(worldWidth / 2, highestTiles[worldWidth / 2] + 3, 0), Quaternion.identity);

        player.blockTilemap = blockTilemap;
        player.backgroundTilemap = midgroundTilemap;

        var script = mainCamera.GetComponent<CameraFollowWithBarriers>();
        mainCamera.transform.position = player.transform.position - new Vector3(0, 0, 10);

        script.topRightBarrier = topright.transform;
        script.bottomLeftBarrier = bottomleft.transform;
        script.followObject = player.transform;

    }

    //Define chunks
    void AddChunks()
    {
        float chunksOnY = (float)worldHeight / chunkSize;
        float chunksOnX = (float)worldWidth / chunkSize;

        for (int x = 0; x < chunksOnX; x++)
            for (int y = 0; y < chunksOnY; y++)
            {
                var middleOfChunk = new Vector3Int((x * chunkSize) + chunkSize / 2, (y * chunkSize) + chunkSize / 2, 0);
                //maybe only plug in middle value save memory
                chunks.Add(new Vector2Int(x, y), new Chunk(new Vector2Int((middleOfChunk.x - chunkSize / 2), (middleOfChunk.y + chunkSize / 2))/*, new Vector3Int(middleOfChunk.x + chunkSize / 2, middleOfChunk.y - chunkSize / 2, 0)*/));
            }
    }


    bool drawn = false;
    private void OnDrawGizmos()
    {
        if (!displayChunks || drawn || chunks == null) { return; }

        drawn = true;

        for (int x = 0; x < (float)worldWidth / chunkSize; x++)
        {
            for (int y = 0; y < (float)worldHeight / chunkSize; y++)
            {
                Color color = Color.red;


                Vector2Int chunkPos = new Vector2Int(x, y);

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

    List<Vector2Int> previousIndicies = new List<Vector2Int>();
    //List<Vector2Int> chunksToUnload = new List<Vector2Int>();
    //List<Chunk> chunksToLoad = new List<Chunk>();

    //MAYBE ONLY EXTRA CHUNKS IN DIRECTION MOVING

    //Load larger chunks of tiles to decrease draw calls

    Vector2Int WorldCoordinatesToNearestChunkCoordinate(Vector2 input)
    {
        Vector2Int chunkCoords = new Vector2Int((int)input.x / chunkSize, (int)input.y / chunkSize);
        return chunkCoords;
    }

    Vector3 lastCameraPositionUpdate;

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

            List<Vector2Int> chunkCoordinatesToLoad = new List<Vector2Int>();

            for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
            {
                for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
                {
                    if (x < 0 || y < 0 || x >= worldWidth / chunkSize || y >= worldHeight / chunkSize) { continue; }
                    chunkCoordinatesToLoad.Add(new Vector2Int(x, y));
                    //Note: Maybe do not store all of these in a list and go ahead and do the work in this loop
                }
            }

            for (int j = 0; j < chunkCoordinatesToLoad.Count; j++)
            {
                Vector2Int chunkCoord = chunkCoordinatesToLoad[j];

                if (previousIndicies.Contains(chunkCoord)) { previousIndicies.Remove(chunkCoord); continue; }

                Chunk chunk = chunks[chunkCoord];

                var bottomRightBlock = chunk.topLeftBlock + new Vector2Int(chunkSize, -chunkSize);

                for (int x = chunk.topLeftBlock.x; x < chunk.topLeftBlock.x + (bottomRightBlock.x - chunk.topLeftBlock.x); x++)
                {
                    for (int y = bottomRightBlock.y; y < bottomRightBlock.y + (chunk.topLeftBlock.y - bottomRightBlock.y); y++)
                    {
                        if (x >= worldWidth)
                        {
                            //Note: May cause issues may need to be continue
                            break;
                        }

                        blockTilemap.SetTile(new Vector3Int(x, y, 0), TileFromBlockType(blockMap[x, y]));
                    }
                }
            }

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

            //Unload remaining chunks that were not removed
            for (int i = 0; i < previousIndicies.Count; i++)
            {
                Chunk unloadChunk = chunks[previousIndicies[i]];

                var bottomRightBlock = unloadChunk.topLeftBlock + new Vector2Int(chunkSize, -chunkSize);

                for (int x = unloadChunk.topLeftBlock.x; x < unloadChunk.topLeftBlock.x + (bottomRightBlock.x - unloadChunk.topLeftBlock.x); x++)
                {
                    for (int y = bottomRightBlock.y; y < bottomRightBlock.y + (unloadChunk.topLeftBlock.y - bottomRightBlock.y); y++)
                    {
                        if (x >= worldWidth)
                        {
                            break;
                        }

                        blockTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    }
                }
            }

            previousIndicies = chunkCoordinatesToLoad;
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
            //bluetorch
            case 7: return blueTorch;
            //greentorch
            case 8: return greenTorch;
            //normal torch
            case 9: return greenTorch;
        }

        return null;
    }

    void Update()
    {
        if (Vector3.Distance(mainCamera.transform.position, lastCameraPositionUpdate) >= chunkSize)
        {
            LoadChunks();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }
}
