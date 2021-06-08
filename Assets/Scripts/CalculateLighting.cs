using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateLighting : MonoBehaviour
{
    [SerializeField] WorldGenerator wg;
    [SerializeField] Vector2Int overlap;
    [Range(.6f, .99f)]
    [SerializeField] float airDropoff;
    [Range(.6f, .9f)]
    [SerializeField] float inBlockDropoff;
    [SerializeField] float lowestLightLevel;
    [SerializeField] FilterMode filterMode;
    //The farthest that light can go with air dropoff can not be larger than this
    [SerializeField] int lightRadius;
    SpriteRenderer sr;
    Vector2Int size;
    Vector2 viewDistance;
    float[,] lightValues;
    Camera cam;

    private void Start()
    {
        float intensity = 1;

        var dist = 1 - Mathf.Sqrt(2);
        //var distV = Mathf.Pow(airDropoff, dist);

        while (intensity > lowestLightLevel)
        {
            intensity *= airDropoff/** distV*/;
            lightRadius++;
        }

        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        lightValues = new float[wg.worldWidth, wg.worldHeight];

        viewDistance = new Vector3((cam.orthographicSize * 1.78f), cam.orthographicSize + .01f, 0);
        size = new Vector2Int(3 + (int)viewDistance.x * 2, 3 + (int)viewDistance.y * 2) + overlap;

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
                    LightBlock(new Vector2Int(x, y), new Vector2Int(x, y), 1, 1);
                }
            }
        }
    }

    public void LightBlock(Vector2Int pos, Vector2Int initialPos, float intensity, int iteration)
    {
        if (intensity <= lowestLightLevel || iteration > lightRadius)
        {
            return;
        }

        var dropoff = airDropoff;

        if (wg.blockMap[pos.x, pos.y] != WorldGenerator.BlockType.air) { dropoff = inBlockDropoff; }

        lightValues[pos.x, pos.y] = intensity;

        int x = pos.x - initialPos.x;
        int y = pos.y - initialPos.y;

        if (Mathf.Abs(x) == Mathf.Abs(y))
        {
            if (pos == initialPos)
            {
                LightNeighbor(new Vector2Int(1, 0), pos, initialPos, intensity, dropoff, iteration);
                LightNeighbor(new Vector2Int(-1, 0), pos, initialPos, intensity, dropoff, iteration);
                LightNeighbor(new Vector2Int(0, 1), pos, initialPos, intensity, dropoff, iteration);
                LightNeighbor(new Vector2Int(0, -1), pos, initialPos, intensity, dropoff, iteration);
                //topRight
                LightNeighbor(new Vector2Int(1, 1), pos, initialPos, intensity, dropoff, iteration);
                //topLeft
                LightNeighbor(new Vector2Int(-1, 1), pos, initialPos, intensity, dropoff, iteration);
                //bottomRight
                LightNeighbor(new Vector2Int(1, -1), pos, initialPos, intensity, dropoff, iteration);
                //bottomLeft
                LightNeighbor(new Vector2Int(-1, -1), pos, initialPos, intensity, dropoff, iteration);
            }
            //we are going in 3 directions
            else if (x > 0)
            {
                //go right
                if (y > 0)
                {
                    //go up
                    LightNeighbor(new Vector2Int(1, 1), pos, initialPos, intensity, dropoff, iteration);
                    LightNeighbor(new Vector2Int(1, 0), pos, initialPos, intensity, dropoff, iteration);
                    LightNeighbor(new Vector2Int(0, 1), pos, initialPos, intensity, dropoff, iteration);
                }
                else
                {
                    //go down
                    LightNeighbor(new Vector2Int(1, -1), pos, initialPos, intensity, dropoff, iteration);
                    LightNeighbor(new Vector2Int(1, 0), pos, initialPos, intensity, dropoff, iteration);
                    LightNeighbor(new Vector2Int(0, -1), pos, initialPos, intensity, dropoff, iteration);
                }
            }
            else
            {
                //go left
                if (y > 0)
                {
                    //go up
                    LightNeighbor(new Vector2Int(-1, 1), pos, initialPos, intensity, dropoff, iteration);
                    LightNeighbor(new Vector2Int(-1, 0), pos, initialPos, intensity, dropoff, iteration);
                    LightNeighbor(new Vector2Int(0, 1), pos, initialPos, intensity, dropoff, iteration);
                }
                else
                {
                    //go down
                    LightNeighbor(new Vector2Int(-1, -1), pos, initialPos, intensity, dropoff, iteration);
                    LightNeighbor(new Vector2Int(-1, 0), pos, initialPos, intensity, dropoff, iteration);
                    LightNeighbor(new Vector2Int(0, -1), pos, initialPos, intensity, dropoff, iteration);
                }
            }
        }
        else if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            //we are going horizontal
            if (x > 0)
            {
                //go right
                LightNeighbor(new Vector2Int(1, 0), pos, initialPos, intensity, dropoff, iteration);
            }
            else
            {
                LightNeighbor(new Vector2Int(-1, 0), pos, initialPos, intensity, dropoff, iteration);
            }
        }
        else
        {
            //we are going vertical
            if (y > 0)
            {
                //go up
                LightNeighbor(new Vector2Int(0, 1), pos, initialPos, intensity, dropoff, iteration);
            }
            else
            {
                //go down
                LightNeighbor(new Vector2Int(0, -1), pos, initialPos, intensity, dropoff, iteration);
            }
        }
    }

    void LightNeighbor(Vector2Int neighborPosition, Vector2Int myPosition, Vector2Int initialPosition, float intensity, float dropoff, int iteration)
    {
        neighborPosition += myPosition;

        if (neighborPosition.x < 0 || neighborPosition.y < 0 || neighborPosition.x >= wg.worldWidth || neighborPosition.y >= wg.worldHeight) { return; }

        //var dist = Mathf.Sqrt((neighborPosition.x - myPosition.x) * (neighborPosition.x - myPosition.x) + (neighborPosition.y - myPosition.y) * (neighborPosition.y - myPosition.y));
        var targetIntensity = intensity * dropoff /** Mathf.Pow(dropoff, dist)*/;

        if (lightValues[neighborPosition.x, neighborPosition.y] < targetIntensity)
        {
            LightBlock(neighborPosition, initialPosition, targetIntensity, iteration + 1);
        }
    }

    void UnlightNeighbor(Vector2Int neighborPosition, Vector2Int myPosition, Vector2Int initialPosition, int iteration)
    {
        neighborPosition += myPosition;

        if (neighborPosition.x < 0 || neighborPosition.y < 0 || neighborPosition.x >= wg.worldWidth || neighborPosition.y >= wg.worldHeight) { return; }

        if (lightValues[neighborPosition.x, neighborPosition.y] < 1)
        {
            UnlightBlock(neighborPosition, initialPosition, false, iteration + 1);
        }
        else
        {
            //UnlightBlock(neighborPosition, initialPosition, true, iteration + 1);
        }
    }

    void UnlightBlock(Vector2Int pos, Vector2Int initialPos, bool justChecking, int iteration)
    {
        if (pos.x >= wg.worldWidth || pos.x < 0 || pos.y < 0 || pos.y >= wg.worldHeight) { return; }

        if (iteration > lightRadius)
        {
            return;
        }

        int x = pos.x - initialPos.x;
        int y = pos.y - initialPos.y;

        if (Mathf.Abs(x) == Mathf.Abs(y))
        {
            if (pos == initialPos)
            {
                UnlightNeighbor(new Vector2Int(1, 0), pos, initialPos, iteration);
                UnlightNeighbor(new Vector2Int(-1, 0), pos, initialPos, iteration);
                UnlightNeighbor(new Vector2Int(0, 1), pos, initialPos, iteration);
                UnlightNeighbor(new Vector2Int(0, -1), pos, initialPos, iteration);
                //topRight
                UnlightNeighbor(new Vector2Int(1, 1), pos, initialPos, iteration);
                //topLeft
                UnlightNeighbor(new Vector2Int(-1, 1), pos, initialPos, iteration);
                //bottomRight
                UnlightNeighbor(new Vector2Int(1, -1), pos, initialPos, iteration);
                //bottomLeft
                UnlightNeighbor(new Vector2Int(-1, -1), pos, initialPos, iteration);
            }

            //we are going in 3 directions
            else if (x > 0)
            {
                //go right
                if (y > 0)
                {
                    //go up
                    UnlightNeighbor(new Vector2Int(1, 1), pos, initialPos, iteration);
                    UnlightNeighbor(new Vector2Int(1, 0), pos, initialPos, iteration);
                    UnlightNeighbor(new Vector2Int(0, 1), pos, initialPos, iteration);
                }
                else
                {
                    //go down
                    UnlightNeighbor(new Vector2Int(1, -1), pos, initialPos, iteration);
                    UnlightNeighbor(new Vector2Int(1, 0), pos, initialPos, iteration);
                    UnlightNeighbor(new Vector2Int(0, -1), pos, initialPos, iteration);
                }
            }
            else
            {
                //go left
                if (y > 0)
                {
                    //go up
                    UnlightNeighbor(new Vector2Int(-1, 1), pos, initialPos, iteration);
                    UnlightNeighbor(new Vector2Int(-1, 0), pos, initialPos, iteration);
                    UnlightNeighbor(new Vector2Int(0, 1), pos, initialPos, iteration);
                }
                else
                {
                    //go down
                    UnlightNeighbor(new Vector2Int(-1, -1), pos, initialPos, iteration);
                    UnlightNeighbor(new Vector2Int(-1, 0), pos, initialPos, iteration);
                    UnlightNeighbor(new Vector2Int(0, -1), pos, initialPos, iteration);
                }
            }
        }
        else if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            //we are going horizontal
            if (x > 0)
            {
                //go right
                UnlightNeighbor(new Vector2Int(1, 0), pos, initialPos, iteration);
            }
            else
            {
                UnlightNeighbor(new Vector2Int(-1, 0), pos, initialPos, iteration);
            }
        }
        else
        {
            //we are going vertical
            if (y > 0)
            {
                //go up
                UnlightNeighbor(new Vector2Int(0, 1), pos, initialPos, iteration);
            }
            else
            {
                //go down
                UnlightNeighbor(new Vector2Int(0, -1), pos, initialPos, iteration);
            }
        }

        if (!justChecking)
        {
            lightValues[pos.x, pos.y] = 0;
            unlitBlocks.Add(pos);
        }
    }

    public void ModifyBlock(Vector2Int pos, bool placingBlock)
    {
        //Removing a block can open up space for light to travel farther
        //Placing a block can block light from getting somewhere with the same strength that it would if it was not there

        //UpdateSurroundingBlocks(pos);

        if (!placingBlock)
        {
            if (pos.y > wg.highestTiles[pos.x] - wg.caveStartingOffset)
            {
                LightBlock(pos, pos, 1, 1);
            }
            else
            {
                //RemoveLightSource(pos);
                RemoveLightSource(pos);
            }
        }
        else
        {
            //if (lightValues[pos.x, pos.y] == 1)
            {
                RemoveLightSource(pos);
            }
            //if blocktype is torch
            //AddLightSource(pos.x, pos.y);
        }

        RegenerateLighting();
    }

    void AddBlocksToRelight(ref List<Vector2Int> blocksToRelight, Vector2Int neighborPos, Vector2Int initialPos)
    {
        neighborPos += initialPos;
        if (neighborPos.x < 0 || neighborPos.y < 0 || neighborPos.x >= wg.worldWidth || neighborPos.y >= wg.worldHeight) { return; }

        if (lightValues[neighborPos.x, neighborPos.y] > lightValues[initialPos.x, initialPos.y] && !blocksToRelight.Contains(neighborPos))
        {
            blocksToRelight.Add(neighborPos);
        }
    }

    List<Vector2Int> unlitBlocks;

    public void RemoveLightSource(Vector2Int pos)
    {
        unlitBlocks = new List<Vector2Int>();

        UnlightBlock(pos, pos, false, 1);

        List<Vector2Int> blocksToRelight = new List<Vector2Int>();

        for (int i = 0; i < unlitBlocks.Count; i++)
        {
            //int radius = 0;
            //float intensity = 0;
            //while (intensity > lowestLightLevel)
            //{
            //    intensity *= inBlockDropoff;
            //    radius++;
            //}

            //// it is not checking the right radius of blocks
            //for (int nx = pos.x - radius; nx < pos.x + radius+1; nx++)
            //{
            //    for (int ny = pos.y - radius; ny < pos.y + radius+1; ny++)
            //    {
            //        if (nx == unlitBlocks[i].x && ny == unlitBlocks[i].y) { continue; }
            //        AddBlocksToRelight(ref blocksToRelight, new Vector2Int(nx, ny), unlitBlocks[i]);
            //    }
            //}
            //int x = pos.x - unlitBlocks[i].x;
            //int y = pos.y - unlitBlocks[i].y;

            Vector2Int initialPos = unlitBlocks[i];

            AddBlocksToRelight(ref blocksToRelight, new Vector2Int(1, 0), initialPos);
            AddBlocksToRelight(ref blocksToRelight, new Vector2Int(-1, 0), initialPos);
            AddBlocksToRelight(ref blocksToRelight, new Vector2Int(0, 1), initialPos);
            AddBlocksToRelight(ref blocksToRelight, new Vector2Int(0, -1), initialPos);
            //topRight       
            AddBlocksToRelight(ref blocksToRelight, new Vector2Int(1, 1), initialPos);
            //topLeft       
            AddBlocksToRelight(ref blocksToRelight, new Vector2Int(-1, 1), initialPos);
            //bottomRight
            AddBlocksToRelight(ref blocksToRelight, new Vector2Int(1, -1), initialPos);
            //bottomLeft
            AddBlocksToRelight(ref blocksToRelight, new Vector2Int(-1, -1), initialPos);

            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(2, 0), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(2, 1), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(2, -1), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(2, 2), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(2, -2), initialPos);

            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(-2, 0), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(-2, 1), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(-2, -1), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(-2, -2), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(-2, 2), initialPos);

            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(1, 2), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(-1, 2), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(0, 2), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(0, -2), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(1, -2), initialPos);
            //AddBlocksToRelight(ref blocksToRelight, new Vector2Int(-1, -2), initialPos);
        }


        for (int k = 0; k < blocksToRelight.Count; k++)
        {
            var btrPos = blocksToRelight[k];

            LightBlock(btrPos, btrPos, lightValues[btrPos.x, btrPos.y], 1);
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

                float tileColor = lightValues[blockmapX, blockmapY];

                Color color = new Color(0, 0, 0, 1 - tileColor);

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