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
    SpriteRenderer sr;
    int lightValuesPropertyID;
    int xOffsetPropertyID;
    int yOffsetPropertyID;
    Vector2Int frameSize;

    Color[,] lightValues;
    Vector3 viewDistance;
    bool[,] toEmit;
    Vector2Int size;
    Camera cam;

    void Start()
    {
        //cameraTexturePropertyID = Shader.PropertyToID("_CameraTex");
        lightValuesPropertyID = Shader.PropertyToID("_LightValues");
        xOffsetPropertyID = Shader.PropertyToID("_XOffset");
        yOffsetPropertyID = Shader.PropertyToID("_YOffset");
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

                lightValues[x, y] = Color.black;
                toEmit[x, y] = false;

                BlockType tile = wg.blockMap[x, y];

                switch (tile)
                {
                    case BlockType.air:
                        lightValues[x, y] = ambientColor;
                        toEmit[x, y] = true;
                        break;
                    case BlockType.bluetorch:
                        lightValues[x, y] = Color.blue;
                        toEmit[x, y] = true;
                        break;
                    case BlockType.redtorch:
                        lightValues[x, y] = Color.red;
                        toEmit[x, y] = true;
                        break;
                    case BlockType.greentorch:
                        lightValues[x, y] = Color.green;
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

    public void AddLightSource(int x, int y, BlockType blocktype)
    {
        toEmit[x, y] = true;
        newLightSources.Add(new Vector2Int(x, y));
    }

    Color ColorFromBlockType(BlockType blocktype)
    {
        switch (blocktype)
        {
            case BlockType.air:
                return ambientColor;
            case BlockType.bluetorch:
                return Color.blue;
            case BlockType.redtorch:
                return Color.red;
            case BlockType.greentorch:
                return Color.green;
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
            BlockType tile = wg.blockMap[pos.x, pos.y];
            EmitLight(pos.x, pos.y, ColorFromBlockType(tile));
            newLightSources.Remove(pos);
        }


        //for (int x = 0; x < size.x; x++)
        //{
        //    for (int y = 0; y < size.y; y++)
        //    {
        //        if (!WithinFrame(x, y)) { continue; }

        //        lightValues[x, y] = Color.black;
        //        toEmit[x, y] = false;

        //        BlockType tile = wg.blockMap[x + (int)transform.position.x, y + (int)transform.position.y];

        //        switch (tile)
        //        {
        //            case BlockType.air:
        //                lightValues[x, y] = ambientColor;
        //                toEmit[x, y] = true;
        //                break;
        //            case BlockType.bluetorch:
        //                lightValues[x, y] = Color.blue;
        //                toEmit[x, y] = true;
        //                break;
        //            case BlockType.redtorch:
        //                lightValues[x, y] = Color.red;
        //                toEmit[x, y] = true;
        //                break;
        //            case BlockType.greentorch:
        //                lightValues[x, y] = Color.green;
        //                toEmit[x, y] = true;
        //                break;
        //        }
        //    }
        //}

        ////Vector2Int pos = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        //for (int x = 0; x < size.x; x++)
        //{
        //    for (int y = 0; y < size.y; y++)
        //    {
        //        if (toEmit[x, y])
        //        {
        //            EmitLight(x, y, lightValues[x, y]);
        //            //Color color = GetLight(x, y);
        //            //EmitLight(x, y, color);
        //        }
        //    }
        //}
    }

    void EmitLight(int rootX, int rootY, Color color)
    {
        for (int x = rootX - lightRadius; x <= rootX + lightRadius; x++)
        {
            for (int y = rootY - lightRadius; y <= rootY + lightRadius; y++)
            {
                SetLight(x, y, color);
            }
        }
    }

    bool WithinBounds(int x, int y)
    {
        return x < wg.worldWidth && x >= 0 && y < wg.worldHeight && y >= 0;
    }

    void SetLight(int x, int y, Color color)
    {
        if (WithinBounds(x, y))
        {
            lightValues[x, y] = color;
        }
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
        lr.shaderMaterial.SetFloat(xOffsetPropertyID, offset.x);
        lr.shaderMaterial.SetFloat(yOffsetPropertyID, offset.y);
    }
}
