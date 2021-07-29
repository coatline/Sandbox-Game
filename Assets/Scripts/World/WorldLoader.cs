using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-1)]
public class WorldLoader : MonoBehaviour
{
    [SerializeField] Tilemap foregroundTilemap;
    [SerializeField] Tilemap midgroundTilemap;
    [SerializeField] Tilemap backgroundTilemap;

    [SerializeField] Player playerPrefab;
    [SerializeField] DataManager dm;
    Camera mainCamera;
    Player player;

    [SerializeField] bool fillInWorld;
    [SerializeField] bool displayChunks;
    bool loadingNewChunks;
    bool unloading;

    Dictionary<Vector2Int, Chunk> chunks;
    List<Vector2Int> chunksToUnload;

    Vector3 lastCameraPositionUpdate;
    Vector3 viewDistance;

    public int excessChunksToLoad;
    public int chunkSize;
    int chunksOnX;
    int chunksOnY;
    int worldWidth;
    int worldHeight;

    void Awake()
    {
        worldWidth = GD.wd.worldWidth;
        worldHeight = GD.wd.worldHeight;

        mainCamera = Camera.main;

        viewDistance = new Vector3((mainCamera.orthographicSize * mainCamera.aspect) + chunkSize / 2, mainCamera.orthographicSize + chunkSize / 2, 0);

        chunksOnX = Mathf.CeilToInt(((float)worldWidth / chunkSize));
        chunksOnY = Mathf.CeilToInt(((float)worldHeight / chunkSize));

        chunks = new Dictionary<Vector2Int, Chunk>();
        chunksToUnload = new List<Vector2Int>();

        AddChunks();
        InitializeWorld();
        AddBarrier();
    }

    public void InitializeWorld()
    {
        SpawnPlayer();

        Vector3 loadRange = new Vector2(viewDistance.x + (excessChunksToLoad * chunkSize), viewDistance.y + (excessChunksToLoad * chunkSize));
        Vector3 topRightLoadPosition = mainCamera.transform.position + loadRange;
        Vector3 bottomLeftLoadPosition = mainCamera.transform.position - loadRange;

        var topRightChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinates(topRightLoadPosition);
        var bottomLeftChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinates(bottomLeftLoadPosition);
        // Issue HERE I THINK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        for (int x = bottomLeftChunkLoadCoordinate.x; x <= topRightChunkLoadCoordinate.x; x++)
        {
            for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
            {
                if (!WithinWorldBounds(x * chunkSize, y * chunkSize)) { continue; }
                Chunk chunk = chunks[new Vector2Int(x, y)];

                LoadChunk(chunk);
                chunk.loaded = true;
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

        player = Instantiate(playerPrefab, new Vector3(worldWidth / 2, GD.wd.highestTiles[worldWidth / 2] + 2, 0), Quaternion.identity);

        var script = mainCamera.GetComponent<CameraFollowWithBarriers>();
        mainCamera.transform.position = player.transform.position - new Vector3(0, 0, 10);

        script.topRightBarrier = topright.transform;
        script.bottomLeftBarrier = bottomleft.transform;
        script.followObject = player.transform;
    }

    Vector2Int WorldCoordinatesToNearestChunkCoordinates(Vector2 input)
    {
        Vector2Int chunkCoords = new Vector2Int((int)(input.x / chunkSize), (int)((int)input.y / chunkSize));
        return chunkCoords;
    }

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
                            fgtiles[iterations] = dm.GetTile(GD.wd.blockMap[wx, wy, 0]);
                            mgtiles[iterations] = dm.GetTile(GD.wd.blockMap[wx, wy, 1]);
                            bgtiles[iterations] = dm.GetTile(GD.wd.blockMap[wx, wy, 2]);
                            iterations++;
                        }
                    }

                Chunk chunk = new Chunk(bottomLeftBlock, positions, fgtiles, mgtiles, bgtiles, nullTiles);
                chunks.Add(new Vector2Int(x, y), chunk);
            }
    }

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

            // Top right of player
            Vector3 loadRange = new Vector2(viewDistance.x + (excessChunksToLoad * chunkSize), viewDistance.y + (excessChunksToLoad * chunkSize));
            Vector3 topRightLoadPosition = mainCamera.transform.position + loadRange;
            Vector3 bottomLeftLoadPosition = mainCamera.transform.position - loadRange;

            var topRightChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinates(topRightLoadPosition);
            var bottomLeftChunkLoadCoordinate = WorldCoordinatesToNearestChunkCoordinates(bottomLeftLoadPosition);

            if (topRightChunkLoadCoordinate.x + 1 < chunksOnX)
            {
                for (int y = bottomLeftChunkLoadCoordinate.y; y <= topRightChunkLoadCoordinate.y; y++)
                {
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
                        chunk.inUnloadQueue = false;
                    }

                    chunk.loaded = true;
                }
            }
            else
            {
                //print("End of world not gonna generate (left)");
            }

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
                        chunk.inUnloadQueue = false;
                    }

                    chunk.loaded = true;
                }
            }
            else
            {
                //print("End of world not gonna generate (right)");
            }

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

                    foregroundTilemap.SetTile(new Vector3Int(x, y, 0), dm.GetTile(GD.wd.blockMap[x, y, 0]));
                    midgroundTilemap.SetTile(new Vector3Int(x, y, 0), dm.GetTile(GD.wd.blockMap[x, y, 1]));
                    backgroundTilemap.SetTile(new Vector3Int(x, y, 0), dm.GetTile(GD.wd.blockMap[x, y, 2]));
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

    void Update()
    {
        loadingNewChunks = false;

        if (Vector3.Distance(mainCamera.transform.position, lastCameraPositionUpdate) >= chunkSize / 2f)
        {
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

    public void SetBlockInTilemap(int x, int y, byte layer, TileBase tile = null)
    {
        var chunkCoordinates = WorldCoordinatesToNearestChunkCoordinates(new Vector2(x, y));
        var chunkWorldCoordinates = chunkCoordinates * new Vector2Int(chunkSize, chunkSize);

        if (layer == 0)
        {
            foregroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            chunks[chunkCoordinates].fgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = tile;
        }
        else if (layer == 1)
        {
            midgroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            chunks[chunkCoordinates].mgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = tile;
        }
        else
        {
            backgroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            chunks[chunkCoordinates].bgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = tile;
        }
    }

    bool WithinWorldBounds(int x, int y)
    {
        return x >= 0 && x < worldWidth && y >= 0 && y < worldHeight;
    }

    void AddBarrier()
    {
        CreateBarrier cb = GetComponent<CreateBarrier>();
        cb.offset = new Vector2(worldWidth / 2, worldHeight / 2);
        cb.mapSize = new Vector2(worldWidth / 2, worldHeight / 2);
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
}
