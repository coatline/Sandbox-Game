using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingRenderer : MonoBehaviour
{
    public Material shaderMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, shaderMaterial);
    }
}
