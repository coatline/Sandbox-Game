using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class CalculateColorLighting : MonoBehaviour
{
    WorldGenerator wg;
    Color ambientColor;
    int lightRadius;
    float airDropoff;
    float blockDropoff;
    float lowestLightLevel;
    public bool drawTiles;
    public Color[] pixels;
    float blockDiagonalDropOff;
    float airDiagonalDropOff;
    Vector2Int frameSize;
    bool running;

    public void StartLighting(Vector2Int frameSize, WorldGenerator wg, Color ambientColor, int lightRadius, float airDropoff, float blockDropoff, float lowestLightLevel)
    {
        this.frameSize = frameSize;
        this.lowestLightLevel = lowestLightLevel;
        this.blockDropoff = blockDropoff;
        this.ambientColor = ambientColor;
        this.lightRadius = lightRadius;
        this.airDropoff = airDropoff;
        this.wg = wg;

        if (running) return;
        running = true;

        blockDiagonalDropOff = Mathf.Pow(blockDropoff, Mathf.Sqrt(2));
        airDiagonalDropOff = Mathf.Pow(airDropoff, Mathf.Sqrt(2));

        singleLightEmission = new Color[lightRadius * 2 + 1, lightRadius * 2 + 1];
        lightValues = new Color[frameSize.x, frameSize.y];
        toEmit = new byte[frameSize.x, frameSize.y];
        pixels = new Color[frameSize.x * frameSize.y];
        singleLightEmission = new Color[lightRadius * 2 + 1, lightRadius * 2 + 1];

        lightingThread = new Thread(Run);
        lightingThread.Start();
    }

    //TODO: maybe use alpha of this array to depict whether or not it will emit light for a nice memory saving
    Color[,] lightValues;
    byte[,] toEmit;
    public byte[,] blockMap;

    public Vector2Int lightingPosition;
    Thread lightingThread;

    private void OnApplicationQuit()
    {
        lightingThread.Abort();
    }

    public void Run()
    {
        while (true)
        {
            if (drawTiles) { continue; }

            // Generate lighting values to render onto my texture.

            DoLighting();

            // Generate the pixels to render using those values.

            SetTexturePixels();

            // Signal that you are finished

            drawTiles = true;
        }
    }

    void SetTexturePixels()
    {
        for (int x = 0; x < frameSize.x; x++)
        {
            for (int y = 0; y < frameSize.y; y++)
            {
                pixels[x + (frameSize.x * y)] = lightValues[x, y];
            }
        }
    }

    void DoLighting()
    {
        for (int x = 0; x < frameSize.x; x++)
        {
            for (int y = 0; y < frameSize.y; y++)
            {
                int worldX = lightingPosition.x + x;
                int worldY = lightingPosition.y + y;

                if (!WithinWorldBounds(worldX, worldY)) { continue; }

                lightValues[x, y] = Color.black;
                toEmit[x, y] = 0;

                byte tile = blockMap[worldX, worldY];

                switch (tile)
                {
                    case 0:
                        if (worldY > wg.highestTiles[worldX] - wg.caveStartingOffset)
                        {
                            lightValues[x, y] = ambientColor;
                            toEmit[x, y] = 2;
                        }
                        break;
                    case 6:
                        lightValues[x, y] = Color.red;
                        toEmit[x, y] = 1;
                        break;
                    case 7:
                        lightValues[x, y] = Color.green;
                        toEmit[x, y] = 1;
                        break;
                    case 8:
                        lightValues[x, y] = Color.blue;
                        toEmit[x, y] = 1;
                        break;
                    case 9:
                        lightValues[x, y] = new Color(1f, .8f, .4f);
                        toEmit[x, y] = 1;
                        break;
                }
            }
        }

        for (int x = 0; x < frameSize.x; x++)
        {
            for (int y = 0; y < frameSize.y; y++)
            {
                if (toEmit[x, y] != 0)
                {
                    EmitLight(x, y, lightValues[x, y]);
                }
            }
        }
    }

    //Temporary
    Color ColorFromBlockType(byte blocktype)
    {
        switch (blocktype)
        {
            case 0:
                return ambientColor;
            case 6:
                return Color.blue;
            case 7:
                return Color.red;
            case 8:
                return Color.green;
            case 9:
                return new Color(1, .9f, .9f);
        }

        return Color.black;
    }

    Color[,] singleLightEmission;
    List<int[]> lightFillQueue = new List<int[]>();

    void EmitLight(int rootX, int rootY, Color color)
    {
        if (toEmit[rootX, rootY] == 2)
        {
            int skys=0;

            for (int nx = rootX - 1; nx <= rootX + 1; nx++)
            {
                for (int ny = rootY - 1; ny <= rootY + 1; ny++)
                {
                    if (!WithinFrameBounds(nx, ny)) { continue; }
                    if (toEmit[nx, ny] == 2)
                    {
                        //it is sky
                        skys++;
                    }
                }
            }

            //9 because it counts itself
            if(skys >= 9)
            {
                return;
            }
        }

        lightFillQueue.Clear();

        //Clear SingleLightEmmision
        //singleLightEmission = new Color[lightRadius * 2 + 1, lightRadius * 2+1];
        for (int x = 0; x < lightRadius * 2 + 1; x++)
        {
            for (int y = 0; y < lightRadius * 2 + 1; y++)
            {
                singleLightEmission[x, y] = Color.black;
            }
        }


        singleLightEmission[lightRadius, lightRadius] = color;
        lightFillQueue.Add(new int[] { rootX, rootY });

        while (lightFillQueue.Count > 0)
        {
            int[] currentTile = lightFillQueue[0];
            lightFillQueue.RemoveAt(0);
            int x = currentTile[0];
            int y = currentTile[1];

            int currentLayer = Mathf.Max(Mathf.Abs(x - rootX), Mathf.Abs(y - rootY));

            bool willPassOn = false;
            Color currentColor = lightValues[x, y];
            Color targetColor = singleLightEmission[lightRadius + x - rootX, lightRadius + y - rootY];

            if ((targetColor.r > lowestLightLevel || targetColor.g > lowestLightLevel || targetColor.b > lowestLightLevel) &&
                (targetColor.r > currentColor.r || targetColor.g > currentColor.g || targetColor.b > currentColor.b))
            {
                lightValues[x, y] = (new Color(Mathf.Max(currentColor.r, targetColor.r), Mathf.Max(currentColor.g, targetColor.g), Mathf.Max(currentColor.b, targetColor.b)));
                willPassOn = true;
            }

            if (!(x == rootX && y == rootY) && !willPassOn) { continue; }

            for (int nx = x - 1; nx <= x + 1; nx++)
            {
                for (int ny = y - 1; ny <= y + 1; ny++)
                {
                    var worldPosX = nx + lightingPosition.x;
                    var worldPosY = ny + lightingPosition.y;

                    if (!WithinFrameBounds(nx, ny) || !WithinWorldBounds(worldPosX, worldPosY)) { continue; }

                    int neighborLayer = Mathf.Max(Mathf.Abs(nx - rootX), Mathf.Abs(ny - rootY));

                    if (neighborLayer <= lightRadius && neighborLayer == currentLayer + 1)
                    {
                        float dropOff = 0;

                        if (blockMap[worldPosX, worldPosY] == 0)
                        {
                            dropOff = (nx != x && ny != y) ? airDiagonalDropOff : airDropoff;
                        }
                        else
                        {
                            dropOff = (nx != x && ny != y) ? blockDiagonalDropOff : blockDropoff;
                        }

                        int emitX = lightRadius + nx - rootX;
                        int emitY = lightRadius + ny - rootY;

                        if (singleLightEmission[emitX, emitY].r + singleLightEmission[emitX, emitY].g + singleLightEmission[emitX, emitY].b == 0)
                        {
                            lightFillQueue.Add(new int[] { nx, ny });
                        }

                        singleLightEmission[emitX, emitY].r = Mathf.Max(targetColor.r * dropOff, singleLightEmission[emitX, emitY].r);
                        singleLightEmission[emitX, emitY].g = Mathf.Max(targetColor.g * dropOff, singleLightEmission[emitX, emitY].g);
                        singleLightEmission[emitX, emitY].b = Mathf.Max(targetColor.b * dropOff, singleLightEmission[emitX, emitY].b);
                    }
                }
            }
        }
    }



    bool WithinFrameBounds(int x, int y)
    {
        return x < frameSize.x && x >= 0 && y < frameSize.y && y >= 0;
    }

    bool WithinWorldBounds(int x, int y)
    {
        return x < wg.worldWidth && x >= 0 && y < wg.worldHeight && y >= 0;
    }
}
//void EmitNeighbor(Vector2Int neighbor, Vector2Int pos, Vector2Int rootPos, Color color, float dropoff)
//{
//    Vector2Int neighborPos = neighbor + pos;

//    var targetColor = color * dropoff;

//    //if (lightValues[neighborPos.x, neighborPos.y] < targetColor)
//    {
//        EmitLight(neighborPos.x, neighborPos.y, rootPos.x, rootPos.y, targetColor);
//    }
//}
//if ((color.r < lowestLightLevel && color.g < lowestLightLevel && color.b < lowestLightLevel) || !WithinFrameBounds(x, y) || !WithinWorldBounds(x, y))
//{
//    return;
//}

//Color targetColor = color;
//Color currentColor = lightValues[x, y];

//if ((targetColor.r > lowestLightLevel || targetColor.g > lowestLightLevel || targetColor.b > lowestLightLevel) &&
//        (targetColor.r > currentColor.r || targetColor.g > currentColor.g || targetColor.b > currentColor.b))
//{
//    lightValues[x, y] = (new Color(Mathf.Max(currentColor.r, targetColor.r), Mathf.Max(currentColor.g, targetColor.g), Mathf.Max(currentColor.b, targetColor.b)));
//}

//int relativeX = x - rootX;
//int relativeY = y - rootY;

//float dropoff = 0;

//Vector2Int pos = new Vector2Int(x, y);
//Vector2Int initialPos = new Vector2Int(rootX, rootY);

//if (blockMap[x, y] == 0)
//{
//    dropoff = (relativeX == relativeY) ? airDiagonalDropOff : airDropoff;
//}
//else
//{
//    dropoff = (relativeX == relativeY) ? blockDiagonalDropOff : blockDropoff;
//}


//if (Mathf.Abs(relativeX) == Mathf.Abs(relativeY))
//{
//    if (x == rootX && y == rootY)
//    {
//        EmitNeighbor(new Vector2Int(1, 0), pos, initialPos, color, dropoff);
//        EmitNeighbor(new Vector2Int(-1, 0), pos, initialPos, color, dropoff);
//        EmitNeighbor(new Vector2Int(0, 1), pos, initialPos, color, dropoff);
//        EmitNeighbor(new Vector2Int(0, -1), pos, initialPos, color, dropoff);
//        //topRight
//        EmitNeighbor(new Vector2Int(1, 1), pos, initialPos, color, dropoff);
//        //topLeft
//        EmitNeighbor(new Vector2Int(-1, 1), pos, initialPos, color, dropoff);
//        //bottomRight
//        EmitNeighbor(new Vector2Int(1, -1), pos, initialPos, color, dropoff);
//        //bottomLeft
//        EmitNeighbor(new Vector2Int(-1, -1), pos, initialPos, color, dropoff);
//    }
//    //we are going in 3 directions
//    else if (x > 0)
//    {
//        //go right
//        if (y > 0)
//        {
//            //go up
//            EmitNeighbor(new Vector2Int(1, 1), pos, initialPos, color, dropoff);
//            EmitNeighbor(new Vector2Int(1, 0), pos, initialPos, color, dropoff);
//            EmitNeighbor(new Vector2Int(0, 1), pos, initialPos, color, dropoff);
//        }
//        else
//        {
//            //go down
//            EmitNeighbor(new Vector2Int(1, -1), pos, initialPos, color, dropoff);
//            EmitNeighbor(new Vector2Int(1, 0), pos, initialPos, color, dropoff);
//            EmitNeighbor(new Vector2Int(0, -1), pos, initialPos, color, dropoff);
//        }
//    }
//    else
//    {
//        //go left
//        if (y > 0)
//        {
//            //go up
//            EmitNeighbor(new Vector2Int(-1, 1), pos, initialPos, color, dropoff);
//            EmitNeighbor(new Vector2Int(-1, 0), pos, initialPos, color, dropoff);
//            EmitNeighbor(new Vector2Int(0, 1), pos, initialPos, color, dropoff);
//        }
//        else
//        {
//            //go down
//            EmitNeighbor(new Vector2Int(-1, -1), pos, initialPos, color, dropoff);
//            EmitNeighbor(new Vector2Int(-1, 0), pos, initialPos, color, dropoff);
//            EmitNeighbor(new Vector2Int(0, -1), pos, initialPos, color, dropoff);
//        }
//    }
//}
//else if (Mathf.Abs(x) > Mathf.Abs(y))
//{
//    //we are going horizontal
//    if (x > 0)
//    {
//        //go right
//        EmitNeighbor(new Vector2Int(1, 0), pos, initialPos, color, dropoff);
//    }
//    else
//    {
//        EmitNeighbor(new Vector2Int(-1, 0), pos, initialPos, color, dropoff);
//    }
//}
//else
//{
//    //we are going vertical
//    if (y > 0)
//    {
//        //go up
//        EmitNeighbor(new Vector2Int(0, 1), pos, initialPos, color, dropoff);
//    }
//    else
//    {
//        //go down
//        EmitNeighbor(new Vector2Int(0, -1), pos, initialPos, color, dropoff);
//    }
//}