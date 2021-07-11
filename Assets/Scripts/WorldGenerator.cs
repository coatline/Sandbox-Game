using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldGenerator : MonoBehaviour
{
    Camera mainCamera;
    Player player;
    [SerializeField] bool inMenu;

    [Header("World Size")]
    public short worldWidth;
    public short worldHeight;
    public int chunkSize;

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
    [SerializeField] PickupManager pickupManager;
    [SerializeField] Player playerPrefab;
    [SerializeField] Pickup pickupPrefab;
    [SerializeField] BlockBreakingVisual bbv;
    [SerializeField] DayNightCycle dnc;
    //[SerializeField] List<SoundData> sounds;

    [SerializeField] List<Structure> structures;
    public List<ItemDataContainer> itemData;
    AudioSource audioS;

    [Header("Settings")]
    public int excessChunksToLoad;
    [Range(0, 100f)]
    [SerializeField] float chanceForTreeToSpawn;
    [Range(6, 20f)]
    [SerializeField] int maxTreeHeight;

    Vector3 viewDistance;
    public Dictionary<Vector2Int, ItemDataContainer> multiTileItems;
    Dictionary<string, Structure> structureFromName;
    //Dictionary<string, SoundData> soundFromName;
    Dictionary<Vector2Int, int> blockDurability;
    Dictionary<string, short> itemIDfromName;
    Dictionary<Vector2Int, Chunk> chunks;
    [HideInInspector]
    public List<short> highestTiles;
    [HideInInspector]
    public short[,,] blockMap;
    int chunksOnX;
    int chunksOnY;
    bool savable;

    void Awake()
    {
        if (inMenu) { return; }

        mainCamera = Camera.main;
        viewDistance = new Vector3((mainCamera.orthographicSize * mainCamera.aspect), mainCamera.orthographicSize, 0);
        viewDistance.x += chunkSize / 2;
        viewDistance.y += chunkSize / 2;

        multiTileItems = new Dictionary<Vector2Int, ItemDataContainer>();
        blockDurability = new Dictionary<Vector2Int, int>();
        highestTiles = new List<short>();

        savable = (SaveData.currentWorld != null);

        if (savable)
        {
            Load();
        }
        else
        {
            blockMap = new short[worldWidth, worldHeight, 3];
            print("This world cannot be saved!");
        }

        chunks = new Dictionary<Vector2Int, Chunk>();
        chunksToUnload = new List<Vector2Int>();

        chunksOnX = Mathf.CeilToInt(((float)worldWidth / chunkSize));
        chunksOnY = Mathf.CeilToInt(((float)worldHeight / chunkSize));

        var cb = GetComponent<CreateBarrier>();
        cb.offset = new Vector2(worldWidth / 2, worldHeight / 2);
        cb.mapSize = new Vector2(worldWidth / 2, worldHeight / 2);

        audioS = GetComponent<AudioSource>();

        if (!savable)
        {
            GenerateWorldValues();
        }

        AddChunks();

        SpawnPlayer();

        InitializeWorld();
    }

    void Load()
    {
        worldWidth = SaveData.currentWorld.worldWidth;
        worldHeight = SaveData.currentWorld.worldHeight;

        blockMap = new short[worldWidth, worldHeight, 3];

        for (int x = 0; x < worldWidth; x++)
            for (int y = 0; y < worldHeight; y++)
                for (int z = 0; z < 3; z++)
                {
                    blockMap[x, y, z] = SaveData.currentWorld.blockData[(x + (y * worldWidth) + (z * (worldHeight * worldWidth)))];
                }

        for (int i = 0; i < SaveData.currentWorld.highestTiles.Length; i++)
        {
            highestTiles.Add(SaveData.currentWorld.highestTiles[i]);
        }
    }

    void GenerateValues()
    {
        if (itemIDfromName == null)
        {
            InitializeData();
        }

        GenerateInitalTerrain();
        GenerateTrees();
        GenerateCaves();
        AddCaveDetail();
    }

    void GenerateWorldValues()
    {
        multiTileItems = new Dictionary<Vector2Int, ItemDataContainer>();
        highestTiles = new List<short>();

        terrainStartingHeight = worldHeight / 2;

        blockMap = new short[worldWidth, worldHeight, 3];

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
                    data[(x + (y * worldWidth) + (z * (worldHeight * worldWidth)))] = blockMap[x, y, z];
                }

        return data;
    }

    public void Save()
    {
        if (!savable) { return; }

        short[] saveBlockMap = new short[worldWidth * worldHeight * 3];

        for (int x = 0; x < worldWidth; x++)
            for (int y = 0; y < worldHeight; y++)
                for (int z = 0; z < 3; z++)
                {
                    saveBlockMap[(x + (y * worldWidth) + (z * (worldHeight * worldWidth)))] = blockMap[x, y, z];
                }

        SaveData.currentWorld.Save(worldWidth, worldHeight, dnc.GetTime(), saveBlockMap, highestTiles.ToArray());
    }

    private void OnApplicationQuit()
    {
        if (inMenu) { return; }

        Save();
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
        //soundFromName = new Dictionary<string, SoundData>();

        for (short i = 0; i < itemData.Count; i++)
        {
            itemData[i].itemData.id = i;

            if (itemData[i].itemData.itemName == "")
            {
                Debug.LogError($"item id of {itemData[i].itemData.id}'s name needs to be assigned!");
            }

            itemIDfromName.Add(itemData[i].itemData.itemName, i);
        }

        for (int j = 0; j < structures.Count; j++)
        {
            if (structures[j]._name == "")
            {
                Debug.LogError($"structure of {j}'s name needs to be assigned!");
            }

            structureFromName.Add(structures[j]._name, structures[j]);
        }

        //for (int k = 0; k < sounds.Count; k++)
        //{
        //    soundFromName.Add(sounds[k].name, sounds[k]);
        //}
    }

    void GenerateInitalTerrain()
    {
        float xOffset = Random.Range(0f, 99999f);

        short dirt = itemIDfromName["Dirt"];
        short stone = itemIDfromName["Stone"];

        for (int x = 0; x < worldWidth; x++)
        {
            float xVal = ((x * (terrainScale / 100)) + xOffset);
            float normalnoise = Mathf.PerlinNoise(xVal, xOffset);

            float yVal = ((normalnoise) * (maxTerrainHeight));

            int surfaceTerrainHeight = Mathf.FloorToInt(yVal);

            highestTiles.Add((short)(surfaceTerrainHeight + terrainStartingHeight));

            for (int y = surfaceTerrainHeight + terrainStartingHeight; y >= 0; y--)
            {
                //if (y < 0 || y >= worldHeight || x < 0 || x >= terrainWidth) { print("INVALIDE POSITION"); continue; }

                if (usePerlinCaves && (surfaceTerrainHeight + terrainStartingHeight) - y >= stoneStartOffset)
                {
                    blockMap[x, y, 0] = stone;
                }
                else
                {
                    if ((surfaceTerrainHeight + terrainStartingHeight) - y > sprinkleStoneStartOffset && Random.Range(0, 3) == 0)
                    {
                        blockMap[x, y, 0] = stone;
                    }
                    else
                    {
                        if (y == surfaceTerrainHeight + terrainStartingHeight)
                        {
                            blockMap[x, y, 0] = itemIDfromName["Grass"];
                        }
                        else
                        {
                            blockMap[x, y, 0] = dirt;
                        }
                    }
                }
            }
        }
    }

    public Vector2Int blockModifiedAt = -Vector2Int.one;

    void GenerateStructure(Structure structure, int x, int y, bool duringRuntime = false)
    {
        for (int rx = 0; rx < structure.width; rx++)
            for (int ry = 0; ry < structure.height; ry++)
            {
                var index = rx + (ry * structure.width);
                var tile = structure.tiles[structure.structureData[index]];
                Vector2Int worldPosition = new Vector2Int((x + rx), y + ry);

                if (!duringRuntime)
                {
                    blockMap[worldPosition.x, worldPosition.y, tile.tileData.placeOnLayer] = tile.itemData.id;
                }
                else
                {
                    PlaceBlock(worldPosition.x, worldPosition.y, tile, true);
                }
            }
    }

    // Pretty much a Queue
    List<Vector2Int> treeBlocksToChop = new List<Vector2Int>();

    bool chopping;

    IEnumerator ChopTree()
    {
        while (treeBlocksToChop.Count > 0)
        {
            Vector2Int pos = treeBlocksToChop[0];
            BreakTreeBlock(pos.x, pos.y);
            //DamageBlock(pos.x, pos.y, 1, 255, true);
            treeBlocksToChop.RemoveAt(0);
            yield return new WaitForSeconds(.015f);
        }

        chopping = false;
    }

    bool CanAttach(int x, int y, ItemDataContainer item)
    {
        if (!item.tileData.noAttachToBackground)
        {
            // It can attach to walls

            if (blockMap[x, y, 2] != 0)
            {
                return true;
            }
        }

        var layer = item.tileData.attachToLayer;

        if (blockMap[x + 1, y, layer] != 0) { return true; }
        else if (blockMap[x - 1, y, layer] != 0) { return true; }
        else if (blockMap[x, y + 1, layer] != 0) { return true; }
        else if (blockMap[x, y - 1, layer] != 0) { return true; }

        for (int i = 0; i < item.tileData.otherAttachTo.Count; i++)
        {
            var target = item.tileData.otherAttachTo[i];

            if (blockMap[x + 1, y, target.tileData.placeOnLayer] == target.itemData.id) { return true; }
            else if (blockMap[x - 1, y, target.tileData.placeOnLayer] == target.itemData.id) { return true; }
            else if (blockMap[x, y + 1, target.tileData.placeOnLayer] == target.itemData.id) { return true; }
            else if (blockMap[x, y - 1, target.tileData.placeOnLayer] == target.itemData.id) { return true; }
        }

        return false;
    }

    public bool CanPlace(int x, int y, ItemDataContainer item)
    {
        var layer = item.tileData.placeOnLayer;

        if (blockMap[x, y, layer] != 0) { return false; }

        if (layer == 2 && CanAttach(x, y, item))
        {
            return true;
        }

        if (blockMap[x, y, 0] == 0 && blockMap[x, y, 1] == 0 && CanAttach(x, y, item))
        {
            return true;
        }

        return false;
    }

    public short CanBreak(int x, int y, Tool tool)
    {
        if ((itemData[blockMap[x, y, 0]].tileData.requiredTool == tool && blockMap[x, y, 0] != 0))
        {
            return 0;
        }
        else if ((itemData[blockMap[x, y, 1]].tileData.requiredTool == tool && blockMap[x, y, 1] != 0))
        {
            return 1;
        }
        else if ((itemData[blockMap[x, y, 2]].tileData.requiredTool == tool && blockMap[x, y, 2] != 0))
        {
            return 2;
        }

        return -1;
    }

    void TryPlayClip(ItemDataContainer item, bool breakS = false)
    {
        if (breakS)
        {
            if (item.tileData.breakSound)
            {
                audioS.PlayOneShot(item.tileData.breakSound.sound.RandomSound());
            }
        }
        else if (item.itemData.useSound)
        {
            audioS.PlayOneShot(item.itemData.useSound.sound.RandomSound());
        }
    }

    public void BreakBlock(int x, int y, byte layer, bool tryDropPickup = true, bool playBreakSound = true)
    {
        ItemDataContainer targetBlock = itemData[blockMap[x, y, layer]];
        SetBlockInTilemap(x, y, layer);

        blockMap[x, y, layer] = 0;

        if (layer == 0) { blockModifiedAt = new Vector2Int(x, y); }

        if (tryDropPickup)
        {
            SpawnPickup(x, y, targetBlock);
        }

        if (playBreakSound)
        {
            TryPlayClip(targetBlock, true);
        }
    }

    bool DamageBlock(int x, int y, byte layer, int strength, ItemDataContainer targetBlock)
    {
        Vector2Int blockPos = new Vector2Int(x, y);

        if (blockDurability.TryGetValue(blockPos, out int val))
        {
            // There is already damage done to this block

            blockDurability[blockPos] -= strength;

            if (blockDurability[blockPos] > 0)
            {
                if (!targetBlock.tileData.hideBreakingGraphic)
                {
                    bbv.DisplayDamage(x, y, targetBlock.tileData.hardness, blockDurability[blockPos]);
                }

                TryPlayClip(targetBlock);
                return false;
            }
            else
            {
                blockDurability.Remove(blockPos);
                bbv.EmitParticles(x, y);
                bbv.Finish();

                return true;
            }
        }
        else
        {
            // There is NOT already damage done to this block

            if (targetBlock.tileData.hardness - strength > 0)
            {
                blockDurability.Add(blockPos, targetBlock.tileData.hardness - strength);

                if (!targetBlock.tileData.hideBreakingGraphic)
                {
                    bbv.DisplayDamage(x, y, targetBlock.tileData.hardness, targetBlock.tileData.hardness - strength);
                    TryPlayClip(targetBlock);
                }

                return false;
            }
            else
            {
                blockDurability.Remove(blockPos);
                bbv.EmitParticles(x, y);
                bbv.Finish();

                return true;
            }
        }
    }

    public void HitBlock(int x, int y, byte layer, int strength)
    {
        ItemDataContainer targetBlock = itemData[blockMap[x, y, layer]];

        if (!DamageBlock(x, y, layer, strength, targetBlock)) { return; }

        switch (layer)
        {
            case 0: BreakBlock(x, y, layer, true); break;
            case 1:

                if (targetBlock.tileData.treeTile)
                {
                    BreakTreeBlock(x, y);
                    TryPlayClip(targetBlock, true);
                }
                else
                {
                    BreakBlock(x, y, layer, true, true);
                }
                break;
            case 2:
                BreakBlock(x, y, layer, true);
                break;
        }

        if (targetBlock.tileData.isPartOfMultiTile)
        {
            BreakMultiTileBlock(x, y, layer);
        }
    }

    void BreakMultiTileBlock(int x, int y, byte layer)
    {
        TileData targetBlock = itemData[blockMap[x, y, layer]].tileData;
        var rootPosition = new Vector2Int(x - targetBlock.partOfMultiTileData.relativePosition.x, y - targetBlock.partOfMultiTileData.relativePosition.y);
        var structureData = multiTileItems[rootPosition];

        for (int sx = 0; sx < structureData.tileData.multiBlockStructure.width; sx++)
        {
            for (int sy = 0; sy < structureData.tileData.multiBlockStructure.height; sy++)
            {
                var posx = rootPosition.x + sx;
                var posy = rootPosition.y + sy;

                if (posx == x && posy == y) { continue; }
                BreakBlock(posx, posy, layer, false);
            }
        }

        SpawnPickup(x, y, structureData);
        multiTileItems.Remove(rootPosition);
    }

    void BreakTreeBlock(int x, int y)
    {
        BreakBlock(x, y, 1, true, false);

        if (itemData[blockMap[x, y + 1, 1]].tileData.treeTile)
        {

            treeBlocksToChop.Add(new Vector2Int(x, y + 1));

            // Check for stumps

            if (itemData[blockMap[x + 1, y, 1]].tileData.treeTile)
            {
                treeBlocksToChop.Add(new Vector2Int(x + 1, y));
            }

            if (itemData[blockMap[x - 1, y, 1]].tileData.treeTile)
            {
                treeBlocksToChop.Add(new Vector2Int(x - 1, y));
            }

            if (!chopping)
            {
                chopping = true;
                StartCoroutine(ChopTree());
            }
        }
    }

    void SetBlockInTilemap(int x, int y, byte layer, RuleTile tile = null)
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

    void SpawnPickup(int x, int y, ItemDataContainer itemBroken)
    {
        if (itemBroken.tileData.itemDroppedOnBreak.item == null) { return; }
        var rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        var pickup = Instantiate(pickupPrefab, new Vector3(x + Random.Range(-.25f, .25f), y + Random.Range(-.25f, .25f), 0) + Vector3.one / 2, rotation);
        pickup.SetItem(itemBroken.tileData.itemDroppedOnBreak);
        pickupManager.AddPickup(pickup);
    }

    public void PlaceBlock(int x, int y, ItemDataContainer tileData, bool generatingStructure = false)
    {
        if (tileData.tileData.placeOnLayer == 0)
        {
            blockModifiedAt = new Vector2Int(x, y);
        }

        var chunkCoordinates = WorldCoordinatesToNearestChunkCoordinates(new Vector2(x, y));
        var chunkWorldCoordinates = chunkCoordinates * new Vector2Int(chunkSize, chunkSize);

        if (tileData.tileData.multiBlockStructure && !generatingStructure)
        {
            // It takes up more than one tile

            multiTileItems.Add(new Vector2Int(x, y), tileData);
            GenerateStructure(tileData.tileData.multiBlockStructure, x, y, true);
            return;
        }
        else
        {
            blockMap[x, y, tileData.tileData.placeOnLayer] = tileData.itemData.id;
        }

        switch (tileData.tileData.placeOnLayer)
        {
            case 0:

                foregroundTilemap.SetTile(new Vector3Int(x, y, 0), tileData.tileData.tile);
                chunks[chunkCoordinates].fgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = tileData.tileData.tile;

                break;
            case 1:

                midgroundTilemap.SetTile(new Vector3Int(x, y, 0), tileData.tileData.tile);
                chunks[chunkCoordinates].mgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = tileData.tileData.tile;

                break;
            case 2:

                backgroundTilemap.SetTile(new Vector3Int(x, y, 0), tileData.tileData.tile);
                chunks[chunkCoordinates].bgtiles[(x - chunkWorldCoordinates.x) + ((y - chunkWorldCoordinates.y) * chunkSize)] = tileData.tileData.tile;

                break;
        }
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

                if (blockMap[x - 2, highestTiles[x], 0] == 0)
                {
                    spawnLeftStump = false;
                }
                if (blockMap[x + 2, highestTiles[x], 0] == 0)
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
        //trees.Add(startingPosition, newTree);

        for (int i = 0; i < treeHeight; i++)
        {
            if (i == 0)
            {
                if (!spawnLeftStump && !spawnRightStump)
                {
                    blockMap[startingPosition.x, startingPosition.y, 1] = itemIDfromName["TreeTrunk"];
                    continue;
                }

                blockMap[startingPosition.x, startingPosition.y, 1] = itemIDfromName["TreeStumpMiddle"];

                if (spawnLeftStump)
                {
                    blockMap[startingPosition.x - 1, startingPosition.y, 1] = itemIDfromName["TreeStumpLeft"];
                }

                if (spawnRightStump)
                {
                    blockMap[startingPosition.x + 1, startingPosition.y, 1] = itemIDfromName["TreeStumpRight"];
                }
            }
            else if (treeHeight - i > 1)
            {
                blockMap[startingPosition.x, startingPosition.y + i, 1] = itemIDfromName["TreeTrunk"];
            }
            else
            {
                blockMap[startingPosition.x, startingPosition.y + i, 1] = itemIDfromName["TreeTop"];
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
        if (inMenu) { return; }

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
                blockMap[x, y, 2] = itemIDfromName["DirtWall"];
            }
        }

        var stoneId = itemIDfromName["Stone"];
        var dirtId = itemIDfromName["Dirt"];

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < highestTiles[x]; y++)
            {
                if (Random.Range(0, 100) > 10) { continue; }

                if (blockMap[x, y, 0] == stoneId || blockMap[x, y, 0] == dirtId)
                {
                    if (blockMap[x, y + 1, 0] == 0)
                    {
                        blockMap[x, y + 1, 0] = itemIDfromName["Wood"];
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
                        blockMap[x, y, 0] = 0;
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
                                blockMap[x, y, 0] = 3;
                            }
                            else
                            {
                                blockMap[x, y, 0] = 0;
                            }
                        }
                        //Create order from madness
                        else
                        {
                            if (x == 0 || y == 0 || y >= worldHeight - 1 || x >= worldWidth - 1)
                            {
                                blockMap[x, y, 0] = 3;
                                continue;
                            }

                            int faces = 0;

                            if (blockMap[x - 1, y, 0] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x + 1, y, 0] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x, y + 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x, y - 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x - 1, y - 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x + 1, y + 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x - 1, y + 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (blockMap[x + 1, y - 1, 0] != 0)
                            {
                                faces++;
                            }
                            if (faces < 4)
                            {
                                blockMap[x, y, 0] = 0;
                            }
                            else if (faces > 4)
                            {
                                //if (Random.Range(0, 5) == 0)
                                //{
                                //    blockMap[x, y] = BlockType.dirt;
                                //}
                                //else
                                {
                                    blockMap[x, y, 0] = 3;
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
                    blockMap[x, y, 0] = 3;
                }
                else if (noise < .1f)
                {

                }
            }
        }
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
                            fgtiles[iterations] = itemData[(blockMap[wx, wy, 0])].tileData.tile;
                            mgtiles[iterations] = itemData[(blockMap[wx, wy, 1])].tileData.tile;
                            bgtiles[iterations] = itemData[(blockMap[wx, wy, 2])].tileData.tile;
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

                    foregroundTilemap.SetTile(new Vector3Int(x, y, 0), itemData[(blockMap[x, y, 0])].tileData.tile);
                    midgroundTilemap.SetTile(new Vector3Int(x, y, 0), itemData[(blockMap[x, y, 1])].tileData.tile);
                    backgroundTilemap.SetTile(new Vector3Int(x, y, 0), itemData[(blockMap[x, y, 2])].tileData.tile);
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
        if (inMenu) { return; }

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
}


