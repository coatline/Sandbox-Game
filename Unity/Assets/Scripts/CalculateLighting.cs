using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateLighting : MonoBehaviour
{
    [SerializeField] WorldGenerator wg;
    [SerializeField] Vector2Int overlap;
    [Range(.6f, .99f)]
    [SerializeField] float airDropoff;
    [Range(.6f,.9f)]
    [SerializeField] float inBlockDropoff;
    [SerializeField] float lowestLightLevel;
    [SerializeField] FilterMode filterMode;
    SpriteRenderer sr;
    Vector2Int size;
    Vector2 viewDistance;
    float[,] lightValues;
    Camera cam;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        lightValues = new float[wg.worldWidth, wg.worldHeight];

        viewDistance = new Vector3((cam.orthographicSize * 1.78f), cam.orthographicSize + .01f, 0);
        size = new Vector2Int(3 + (int)viewDistance.x * 2, 1 + (int)viewDistance.y * 2) + overlap;

        InitializeLights();
    }

    Vector3 previousPosition;

    private void Update()
    {
        transform.position = new Vector3Int((int)(cam.transform.position.x - viewDistance.x - overlap.x), (int)(cam.transform.position.y - viewDistance.y - overlap.y), 0);

        if (previousPosition != transform.position)
        {
            RegenerateLighting();
        }

        previousPosition = transform.position;
    }

    void InitializeLights()
    {
        for (int x = 0; x < wg.worldWidth; x++)
        {
            for (int y = 0; y < wg.worldHeight; y++)
            {
                if (y > wg.highestTiles[x] - wg.caveStartingOffset && wg.blockMap[x, y] == WorldGenerator.BlockType.air)
                {
                    LightBlock(x, y, 1, 0);
                }
            }
        }
    }

    void LightBlock(int x, int y, float intensity, int iteration)
    {
        if (intensity <= lowestLightLevel)
        {
            return;
        }

        lightValues[x, y] = intensity;

        var dropoff = airDropoff;

        if (wg.blockMap[x, y] != WorldGenerator.BlockType.air) { dropoff = inBlockDropoff; }

        for (int neighborX = x - 1; neighborX < x + 2; neighborX++)
        {
            for (int neighborY = y - 1; neighborY < y + 2; neighborY++)
            {
                if (neighborX < 0 || neighborY < 0 || neighborX >= wg.worldWidth || neighborY >= wg.worldHeight) { continue; }

                if (lightValues[neighborX, neighborY] == 1 && neighborX == x && neighborY == y) { continue; }

                var dist = Mathf.Sqrt((neighborX - x) * (neighborX - x) + (neighborY - y) * (neighborY - y));
                var targetIntensity = intensity * dropoff * Mathf.Pow(dropoff, dist);
                if (lightValues[neighborX, neighborY] < targetIntensity)
                {
                    LightBlock(neighborX, neighborY, targetIntensity, iteration + 1);
                }
            }

        }
    }

    void UnlightBlock(int x, int y, float intensity, Vector2Int initalPosition)
    {
        if (x >= wg.worldWidth || x < 0 || y < 0 || y >= wg.worldHeight) { return; }

        if (unlitBlocks.Contains(new Vector2Int(x, y)) || intensity <= lowestLightLevel)
        {
            return;
        }

        var dropoff = airDropoff;

        if (wg.blockMap[x, y] != WorldGenerator.BlockType.air) { dropoff = inBlockDropoff; }

        var dist = Mathf.Sqrt((initalPosition.x - x) * (initalPosition.x - x) + (initalPosition.y - y) * (initalPosition.y - y));

        intensity = dropoff * Mathf.Pow(dropoff, dist);

        for (int neighborX = x - 1; neighborX < x + 2; neighborX++)
        {
            for (int neighborY = y - 1; neighborY < y + 2; neighborY++)
            {
                if (neighborX == x && neighborY == y) { continue; }
                if (neighborX < 0 || neighborY < 0 || neighborX >= wg.worldWidth || neighborY >= wg.worldHeight) { continue; }

                if (lightValues[neighborX, neighborY] < lightValues[x, y])
                {
                    UnlightBlock(neighborX, neighborY, intensity,initalPosition);
                }
            }
        }

        lightValues[x, y] = 0;
        unlitBlocks.Add(new Vector2Int(x, y));
    }

    public void ModifyBlock(int x, int y, bool removingBlock)
    {
        //Remove a block
        //Removing a block can open up space for light to travel farther
        //if (removingBlock)
        //{
        //    /*LIGHT THE BLOCK AROUND IT AGIAN THIS DOESNT WORK BECAUSE IF I REMOVE A BLOCK THE AIR BLOCK REACTS TO LIGHT DIFFERENTLY SO DO WHAT YOU DO WHEN YOU PLACE A BLOCK GET NEIGHBOR TILES AND SEE IF THEY ARE LIGHTING AND RELIGHT THEM */
        //    List<Vector2Int> blocksToRelight = new List<Vector2Int>();

        //    for (int neighborX = x - 1; neighborX < x + 2; neighborX++)
        //    {
        //        for (int neighborY = y - 1; neighborY < y + 2; neighborY++)
        //        {
        //            if (neighborX < 0 || neighborY < 0 || neighborX >= wg.worldWidth || neighborY >= wg.worldHeight) { continue; }

        //            if (lightValues[neighborX, neighborY] > lightValues[x, y] && !blocksToRelight.Contains(new Vector2Int(neighborX, neighborY)))
        //            {
        //                blocksToRelight.Add(new Vector2Int(neighborX, neighborY));
        //            } 
        //        }
        //    }

        //    for (int i = 0; i < blocksToRelight.Count; i++)
        //    {
        //        var btrx = blocksToRelight[i].x;
        //        var btry = blocksToRelight[i].y;
        //        LightBlock(btrx, btry, lightValues[btrx, btry], 0);
        //    }

        //    RegenerateLighting();
        //    return;
        //}
        //Place a block
        //Placing a block can block light from getting somewhere with the same strength that it would if it was not there
        //else
        //{
        //    List<Vector2Int> blocksToRelight = new List<Vector2Int>();

        //    for (int neighborX = x - 1; neighborX < x + 2; neighborX++)
        //    {
        //        for (int neighborY = y - 1; neighborY < y + 2; neighborY++)
        //        {
        //            if (neighborX < 0 || neighborY < 0 || neighborX >= wg.worldWidth || neighborY >= wg.worldHeight) { continue; }

        //            if (lightValues[neighborX, neighborY] > lightValues[x, y] && !blocksToRelight.Contains(new Vector2Int(neighborX, neighborY)))
        //            {
        //                blocksToRelight.Add(new Vector2Int(neighborX, neighborY));
        //            }
        //        }
        //    }

        //    for (int i = 0; i < blocksToRelight.Count; i++)
        //    {
        //        var btrx = blocksToRelight[i].x;
        //        var btry = blocksToRelight[i].y;
        //        LightBlock(btrx, btry, lightValues[btrx, btry], 0);
        //    }

        //    RegenerateLighting();
        //    return;
        //}
    }

    public void AddLightSource(int x, int y, bool removingBlock)
    {
        if (removingBlock)
        {
            ModifyBlock(x, y, true);

            //if it is less than where daylight can seep through
            if (y < wg.highestTiles[x] - wg.caveStartingOffset)
            {
                return;
            }
        }

        LightBlock(x, y, 1, 0);
        RegenerateLighting();
    }

    List<Vector2Int> unlitBlocks;

    public void RemoveLightSource(int x, int y)
    {
        unlitBlocks = new List<Vector2Int>();

        UnlightBlock(x, y, 1, new Vector2Int(x, y));

        List<Vector2Int> blocksToRelight = new List<Vector2Int>();

        for (int i = 0; i < unlitBlocks.Count; i++)
        {
            for (int neighborX = unlitBlocks[i].x - 1; neighborX < unlitBlocks[i].x + 2; neighborX++)
            {
                for (int neighborY = unlitBlocks[i].y - 1; neighborY < unlitBlocks[i].y + 2; neighborY++)
                {
                    if (neighborX < 0 || neighborY < 0 || neighborX >= wg.worldWidth || neighborY >= wg.worldHeight) { continue; }

                    if (lightValues[neighborX, neighborY] > lightValues[unlitBlocks[i].x, unlitBlocks[i].y] && !blocksToRelight.Contains(new Vector2Int(neighborX, neighborY)))
                    {
                        blocksToRelight.Add(new Vector2Int(neighborX, neighborY));
                    }
                }
            }
        }

        for (int k = 0; k < blocksToRelight.Count; k++)
        {
            var btrx = blocksToRelight[k].x;
            var btry = blocksToRelight[k].y;
            LightBlock(btrx, btry, lightValues[btrx, btry], 0);
        }

        RegenerateLighting();
    }

    Color[] nearbyLightData;

    void RegenerateLighting()
    {
        nearbyLightData = new Color[(size.x) * (size.y)];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var blockmapX = x + (int)transform.position.x;
                var blockmapY = y + (int)transform.position.y;

                if (blockmapX < 0 || blockmapY < 0 || blockmapX >= wg.worldWidth || blockmapY >= wg.worldHeight)
                {
                    continue;
                }

                Color color = new Color(1, 1, 1, 1 - lightValues[blockmapX, blockmapY]);

                nearbyLightData[x + (size.x * y)] = color;
            }
        }

        var texture = new Texture2D(size.x, size.y);
        texture.SetPixels(nearbyLightData);
        texture.filterMode = filterMode;
        texture.Apply();

        sr.sprite = Sprite.Create(texture, new Rect(0, 0, size.x, size.y), Vector2.zero);

    }
}
