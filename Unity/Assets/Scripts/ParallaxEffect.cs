using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [SerializeField] Vector2 parallaxEffectMultiplier;
    [SerializeField] bool infiniteHorizontal;
    [SerializeField] bool infiniteVertical;
    [SerializeField] WorldGenerator wg;

    Transform cameraTransform;
    Vector3 lastCameraPosition;
    float textureUnitSizeX;
    float textureUnitSizeY;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        transform.position = new Vector3(cameraTransform.position.x, (wg.worldHeight / 2f) + textureUnitSizeY / 2);
        textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
        textureUnitSizeY = texture.height / sprite.pixelsPerUnit;
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x, deltaMovement.y * parallaxEffectMultiplier.y);
        lastCameraPosition = cameraTransform.position;

        if (infiniteHorizontal && Mathf.Abs(cameraTransform.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y);
        }
        if (infiniteVertical && Mathf.Abs(cameraTransform.position.y - transform.position.y) >= textureUnitSizeY)
        {
            float offsetPositionY = (cameraTransform.position.y - transform.position.y) % textureUnitSizeY;
            transform.position = new Vector3(transform.position.x, cameraTransform.position.y + offsetPositionY);
        }
    }
}
