using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class RenderLighting : MonoBehaviour
{
    [SerializeField] FilterMode filterMode;
    [SerializeField] LightingRenderer lr;
    [SerializeField] Vector2Int overlap;
    [SerializeField] int lightRadius;
    [SerializeField] int fps;

    [Header("Light Settings")]
    [Range(.6f, .99f)]
    [SerializeField] float airDropoff;
    [Range(.3f, .9f)]
    [SerializeField] float blockDropoff;
    [SerializeField] float lowestLightLevel;
    [SerializeField] bool drawLighting;

    public List<Transform> dynamicLightEmitters;
    CalculateColorLighting ccl;
    SpriteRenderer sr;
    Camera cam;
    int lightValuesPropertyID;

    Vector2Int frameSize;
    Vector2Int size;

    private void Start()
    {
        lightValuesPropertyID = Shader.PropertyToID("_LightValues");
        cam = Camera.main;

        SetMinLightRadius();
        overlap = new Vector2Int((lightRadius - 6) * 2, (lightRadius - 6) * 2);

        Vector3 viewDistance = cam.GetComponent<CameraFollowWithBarriers>().cameraSizeInUnits;
        size = new Vector2Int((int)viewDistance.x * 2, (int)viewDistance.y * 2) + overlap;
        frameSize = size;

        transform.position = new Vector3Int((int)cam.transform.position.x - size.x / 2, (int)cam.transform.position.y - size.y / 2, 0);

        InitializeSprite();

        texture = new Texture2D(frameSize.x, frameSize.y);

        ccl = GetComponent<CalculateColorLighting>();
        ccl.lightingPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        ccl.StartupLighting(frameSize, lightRadius, airDropoff, blockDropoff, lowestLightLevel);
    }

    void SetMinLightRadius()
    {
        float dropoff = 1;
        int radius = 0;

        while (dropoff > lowestLightLevel)
        {
            dropoff *= airDropoff;
            radius++;
        }

        lightRadius = radius;
    }

    Texture2D texture;

    void UpdateTexture(Color[] pixels)
    {
        if (!drawLighting) { return; }
        texture.SetPixels(pixels);
        texture.filterMode = filterMode;
        texture.Apply();
        lr.shaderMaterial.SetTexture(lightValuesPropertyID, texture);
    }

    void InitializeSprite()
    {
        sr = GetComponent<SpriteRenderer>();

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

    Vector2Int theoreticalPosition;

    void Update()
    {
        if (!ccl.running)
        {
            if (ccl.lightingPosition == theoreticalPosition)
            {
                UpdateTexture(ccl.pixels);
            }

            if (transform.position != new Vector3(theoreticalPosition.x, theoreticalPosition.y) && ccl.lightingPosition == theoreticalPosition)
            {
                transform.position = new Vector3(theoreticalPosition.x, theoreticalPosition.y);
            }

            theoreticalPosition = new Vector2Int((int)cam.transform.position.x - size.x / 2, (int)cam.transform.position.y - size.y / 2);
            ccl.lightingPosition = theoreticalPosition;

            ccl.lightEmitters.Clear();

            for (int i = 0; i < dynamicLightEmitters.Count; i++)
            {
                if (dynamicLightEmitters[i] == null)
                {
                    //does this skip over an element?
                    dynamicLightEmitters.RemoveAt(i);
                    i--;
                }
                else
                {
                    ccl.lightEmitters.Add(new Vector2Int((int)dynamicLightEmitters[i].position.x, (int)dynamicLightEmitters[i].position.y));
                }
            }

            ccl.StartThread();
        }

        if (Input.GetKey(KeyCode.P))
        {
            Time.timeScale = .01f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
