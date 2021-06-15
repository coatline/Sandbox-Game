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
        toEmit = new bool[frameSize.x, frameSize.y];
        pixels = new Color[frameSize.x * frameSize.y];
        singleLightEmission = new Color[lightRadius * 2 + 1, lightRadius * 2 + 1];

        lightingThread = new Thread(Run);
        lightingThread.Start();
    }

    //TODO: maybe use alpha of this array to depict whether or not it will emit light for a nice memory saving
    Color[,] lightValues;
    bool[,] toEmit;
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
                toEmit[x, y] = false;

                byte tile = blockMap[worldX, worldY];

                switch (tile)
                {
                    case 0:
                        //if (worldY > wg.highestTiles[worldX] - wg.caveStartingOffset)
                        {
                            lightValues[x, y] = ambientColor;
                            toEmit[x, y] = true;
                        }
                        break;
                    case 6:
                        lightValues[x, y] = Color.blue;
                        toEmit[x, y] = true;
                        break;
                    case 7:
                        lightValues[x, y] = Color.red;
                        toEmit[x, y] = true;
                        break;
                    case 8:
                        lightValues[x, y] = Color.green;
                        toEmit[x, y] = true;
                        break;
                    case 9:
                        lightValues[x, y] = new Color(1, .99f, .95f);
                        toEmit[y, y] = true;
                        break;
                }
            }
        }

        //print("Light Sources Flagged. Starting Light Emission.");

        for (int x = 0; x < frameSize.x; x++)
        {
            for (int y = 0; y < frameSize.y; y++)
            {
                if (toEmit[x, y])
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
        lightFillQueue.Clear();

        //Clear SingleLightEmmision

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
