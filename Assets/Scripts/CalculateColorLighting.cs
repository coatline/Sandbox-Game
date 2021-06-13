using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateColorLighting : MonoBehaviour
{
    [SerializeField] FilterMode filterMode;
    [SerializeField] LightingRenderer lr;
    [SerializeField] Vector2Int overlap;
    [SerializeField] WorldGenerator wg;
    [SerializeField] Color ambientColor;
    [SerializeField] int lightRadius;
    [Range(.6f, .99f)]
    [SerializeField] float airDropoff;
    [Range(.6f, .9f)]
    [SerializeField] float blockDropoff;
    [SerializeField] float lowestLightLevel;
    float blockDiagonalDropOff;
    float airDiagonalDropOff;
    SpriteRenderer sr;
    int lightValuesPropertyID;
    Vector2Int frameSize;

    //TODO: maybe use alpha of this array to depict whether or not it will emit light for a nice memory saving
    Color[,] lightValues;
    Vector3 viewDistance;
    bool[,] toEmit;
    Vector2Int size;
    Camera cam;

    void Start()
    {
        blockDiagonalDropOff = Mathf.Pow(blockDropoff, Mathf.Sqrt(2));
        airDiagonalDropOff = Mathf.Pow(airDropoff, Mathf.Sqrt(2));

        //cameraTexturePropertyID = Shader.PropertyToID("_CameraTex");
        lightValuesPropertyID = Shader.PropertyToID("_LightValues");
        //lr.shaderMaterial.SetTexture(cameraTexturePropertyID, cameraRenderTexture);
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;

        var unitInUV = cam.WorldToViewportPoint(Vector3.zero) - cam.WorldToViewportPoint(Vector3.one);

        lr.shaderMaterial.SetFloat("_UnitInUV", unitInUV.x);

        viewDistance = cam.GetComponent<CameraFollowWithBarriers>().cameraSizeInUnits;
        size = new Vector2Int((int)viewDistance.x * 2, (int)viewDistance.y * 2) + overlap;
        frameSize = size + overlap;

        lightValues = new Color[wg.worldWidth, wg.worldHeight];
        toEmit = new bool[wg.worldWidth, wg.worldHeight];

        InitializeSprite();
        InitalizeLighting();
    }

    void InitializeSprite()
    {
        Color[] nearbyLightData = new Color[(frameSize.x) * (frameSize.y)];

        for (int x = 0; x < frameSize.x; x++)
        {
            for (int y = 0; y < frameSize.y; y++)
            {
                nearbyLightData[x + (frameSize.x * y)] = Color.white;
            }
        }

        var texture = new Texture2D(frameSize.x, frameSize.y);
        texture.SetPixels(nearbyLightData);
        texture.filterMode = filterMode;
        texture.Apply();

        sr.sprite = Sprite.Create(texture, new Rect(0, 0, frameSize.x, frameSize.y), Vector2.zero);
    }

    Vector3 previousCamPosition;

    void Update()
    {
        transform.position = new Vector3Int((int)cam.transform.position.x - size.x / 2, (int)cam.transform.position.y - size.y / 2, 0);

        if (newLightSources.Count > 0 || removedLightSources.Count > 0)
        {
            CalculateLighting();
            UpdateTexture();
        }

        if (transform.position != previousCamPosition)
        {
            UpdateTexture();
        }

        previousCamPosition = cam.transform.position;
    }

    void InitalizeLighting()
    {
        for (int x = 0; x < wg.worldWidth; x++)
        {
            for (int y = 0; y < wg.worldHeight; y++)
            {
                if (!WithinBounds(x, y)) { continue; }

                //lightValues[x, y] = Color.black;
                //toEmit[x, y] = false;

                byte tile = wg.blockMap[x, y];

                switch (tile)
                {
                    case 0:
                        if (y > wg.highestTiles[x] - wg.caveStartingOffset)
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
                        lightValues[x, y] = new Color(1, .647f, 0);
                        toEmit[x, y] = true;
                        break;
                }
            }
        }

        for (int x = 0; x < wg.worldWidth; x++)
        {
            for (int y = 0; y < wg.worldHeight; y++)
            {
                if (toEmit[x, y])
                {
                    EmitLight(x, y, lightValues[x, y]);
                }
            }
        }
    }

    public void RemoveLightSource(int x, int y)
    {
        if (toEmit[x, y])
        {
            toEmit[x, y] = false;
        }
        else
        {
            return;
        }
        removedLightSources.Add(new Vector2Int(x, y));
    }

    List<Vector2Int> newLightSources = new List<Vector2Int>();
    List<Vector2Int> removedLightSources = new List<Vector2Int>();

    public void AddLightSource(int x, int y, bool placingBlock)
    {
        if (!placingBlock)
        {
            //if there is a wall behind it then do not emit light
            if(y < wg.highestTiles[x] - wg.caveStartingOffset)
            {
                return;
            }
        }

        toEmit[x, y] = true;
        newLightSources.Add(new Vector2Int(x, y));
    }

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
                return new Color(1, .647f, 0);
        }

        return Color.black;
    }

    List<Vector2Int> toReEmit;
    List<Color> toReEmitColors;

    void ResetSurroundingBlocks(int x, int y)
    {
        for (int nx = x - lightRadius; nx <= x + lightRadius; nx++)
        {
            for (int ny = y - lightRadius; ny <= y + lightRadius; ny++)
            {
                if (nx == x && ny == y) { lightValues[nx, ny] = Color.black; continue; }

                if (toEmit[nx, ny])
                {
                    toReEmitColors.Add(lightValues[nx, ny]);
                    toReEmit.Add(new Vector2Int(nx, ny));
                    toEmit[nx, ny] = false;
                    ResetSurroundingBlocks(x, y);
                }

                lightValues[nx, ny] = Color.black;
            }
        }
    }

    void CalculateLighting()
    {
        toReEmit = new List<Vector2Int>();
        toReEmitColors = new List<Color>();

        for (int k = 0; k < removedLightSources.Count; k++)
        {
            var pos = removedLightSources[k];
            ResetSurroundingBlocks(pos.x, pos.y);
        }

        for (int j = 0; j < toReEmit.Count; j++)
        {
            var pos = toReEmit[j];
            EmitLight(pos.x, pos.y, toReEmitColors[j]);
            toEmit[pos.x, pos.y] = true;
        }

        for (int i = 0; i < newLightSources.Count; i++)
        {
            var pos = newLightSources[i];
            byte tile = wg.blockMap[pos.x, pos.y];
            EmitLight(pos.x, pos.y, ColorFromBlockType(tile));
            newLightSources.Remove(pos);
        }
    }

    Color[,] singleLightEmmision;
    List<int[]> lightFillQueue = new List<int[]>();

    void EmitLight(int rootX, int rootY, Color color)
    {
        lightFillQueue.Clear();

        //probably do not do this but just set all values to zero
        singleLightEmmision = new Color[lightRadius * 2 + 1, lightRadius * 2 + 1];


        singleLightEmmision[lightRadius, lightRadius] = color;
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
            Color targetColor = singleLightEmmision[lightRadius + x - rootX, lightRadius + y - rootY];

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
                    if (!WithinBounds(nx, ny)) { continue; }

                    int neighborLayer = Mathf.Max(Mathf.Abs(nx - rootX), Mathf.Abs(ny - rootY));

                    if (neighborLayer <= lightRadius && neighborLayer == currentLayer + 1)
                    {
                        float dropOff = 0;

                        if (wg.blockMap[nx, ny] == 0)
                        {
                            dropOff = (nx != x && ny != y) ? airDiagonalDropOff : airDropoff;
                        }
                        else
                        {
                            dropOff = (nx != x && ny != y) ? blockDiagonalDropOff : blockDropoff;
                        }

                        int emitX = lightRadius + nx - rootX;
                        int emitY = lightRadius + ny - rootY;

                        if (singleLightEmmision[emitX, emitY].r + singleLightEmmision[emitX, emitY].g + singleLightEmmision[emitX, emitY].b == 0)
                        {
                            lightFillQueue.Add(new int[] { nx, ny });
                        }

                        singleLightEmmision[emitX, emitY].r = Mathf.Max(targetColor.r * dropOff, singleLightEmmision[emitX, emitY].r);
                        singleLightEmmision[emitX, emitY].g = Mathf.Max(targetColor.g * dropOff, singleLightEmmision[emitX, emitY].g);
                        singleLightEmmision[emitX, emitY].b = Mathf.Max(targetColor.b * dropOff, singleLightEmmision[emitX, emitY].b);
                    }
                }
            }
        }




        //for (int x = rootX - lightRadius; x <= rootX + lightRadius; x++)
        //{
        //    for (int y = rootY - lightRadius; y <= rootY + lightRadius; y++)
        //    {
        //        if (!WithinBounds(x, y)) { continue; }

        //        //int currentLayer = Mathf.Max(Mathf.Abs(x - rootX), Mathf.Abs(y - rootY));

        //        var coordsX = Mathf.Abs(x - rootX);
        //        var coordsY = Mathf.Abs(y - rootY);

        //        float dropOff = (coordsX == coordsY) ? diagonalDropOff : blockDropoff;

        //        color *= new Color(dropOff,dropOff,dropOff,1);
        //        //print(color);
        //        //Color targetColor=Color.black;

        //        //for (int i = 0; i < currentLayer; i++)
        //        //{
        //        //    targetColor = color * new Color(dropOff, dropOff, dropOff);
        //        //}

        //        //Color currentColor = lightValues[x, y];

        //        //if ((targetColor.r > lowestLightLevel || targetColor.g > lowestLightLevel || targetColor.b > lowestLightLevel) &&
        //        //        (targetColor.r > currentColor.r || targetColor.g > currentColor.g || targetColor.b > currentColor.b))
        //        //{
        //        //    lightValues[x, y] = (new Color(Mathf.Max(currentColor.r, targetColor.r), Mathf.Max(currentColor.g, targetColor.g), Mathf.Max(currentColor.b, targetColor.b)));
        //        //}



        //        lightValues[x, y] = color;
        //    }
        //}
    }

    bool WithinBounds(int x, int y)
    {
        return x < wg.worldWidth && x >= 0 && y < wg.worldHeight && y >= 0;
    }

    Color[] nearbyLightData;

    void UpdateTexture()
    {
        nearbyLightData = new Color[(frameSize.x) * (frameSize.y)];

        for (int x = 0; x < frameSize.x; x++)
        {
            for (int y = 0; y < frameSize.y; y++)
            {
                Vector2Int worldPosition = new Vector2Int(x + (int)transform.position.x, y + (int)transform.position.y);

                if (worldPosition.x < 0 || worldPosition.y < 0 || worldPosition.x >= wg.worldWidth || worldPosition.y >= wg.worldHeight)
                {
                    continue;
                }

                Color tileColor = lightValues[worldPosition.x, worldPosition.y];

                nearbyLightData[x + (frameSize.x * y)] = tileColor;
            }
        }

        var texture = new Texture2D(frameSize.x, frameSize.y);
        texture.SetPixels(nearbyLightData);
        texture.filterMode = filterMode;
        texture.Apply();

        Vector3 offset = cam.WorldToViewportPoint(cam.transform.position) - cam.WorldToViewportPoint(new Vector3((int)cam.transform.position.x, (int)cam.transform.position.y));

        //lr.shaderMaterial.SetTexture("_CameraTex", texture);
        lr.shaderMaterial.SetTexture(lightValuesPropertyID, texture);
    }
}
