using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateColliders : MonoBehaviour
{
    [SerializeField] int tilemapLayer;
    [SerializeField] WorldGenerator wg;
    [SerializeField] int checkWidth;
    [SerializeField] int checkHeight;
    BoxCollider2D[,] colliders;

    void Start()
    {
        wg = FindObjectOfType<WorldGenerator>();

        colliders = new BoxCollider2D[checkWidth, checkHeight];

        var g = new GameObject($"{gameObject.name} Surrounding Collision");
        var masterGob = GameObject.Find("Tilemap Collision").transform;
        g.transform.SetParent(masterGob);
        g.layer = tilemapLayer;

        for (int x = 0; x < checkWidth; x++)
        {
            for (int y = 0; y < checkHeight; y++)
            {
                var bc = g.AddComponent<BoxCollider2D>();
                bc.enabled = false;
                colliders[x, y] = bc;
            }
        }
    }

    Vector3 previousPosition;

    void Update()
    {
        if (transform.position == previousPosition && !wg.blockModified) { return; }

        int checkPosX = (int)transform.position.x - checkWidth / 2;
        int checkPosY = (int)transform.position.y - checkHeight / 2;

        for (int x = 0; x < checkWidth; x++)
        {
            for (int y = 0; y < checkHeight; y++)
            {
                if (checkPosX + x >= wg.worldWidth || checkPosY + y >= wg.worldHeight || checkPosX < 0 || checkPosY < 0) { continue; }

                if (wg.fgblockMap[checkPosX + x, checkPosY + y] != 0)
                {
                    colliders[x, y].offset = new Vector2(checkPosX + x + .5f, checkPosY + y + .5f);
                    colliders[x, y].enabled = true;
                }
                else
                {
                    colliders[x, y].enabled = false;
                }
            }
        }

        previousPosition = transform.position;
        //if (wg.blockMap[posX, posY] != 0)
        //{
        //    colliders[2, 2].enabled = true;
        //    colliders[2, 2].offset = new Vector2(posX, posY);
        //}
        //if (wg.blockMap[posX - 1, posY] != 0)
        //{
        //    colliders[1, 2].enabled = true;
        //    colliders[1, 2].offset = new Vector2(posX, posY);
        //}
        //if (wg.blockMap[posX + 1, posY] != 0)
        //{
        //    colliders[2, 2].enabled = true;
        //    colliders[3, 2].offset = new Vector2(posX, posY);
        //}
        //if (wg.blockMap[posX, posY + 1] != 0)
        //{
        //    colliders[2, 2].enabled = true;
        //    colliders[2, 2].offset = new Vector2(posX, posY);
        //}
        //if (wg.blockMap[posX, posY - 1] != 0)
        //{
        //    colliders[2, 2].enabled = true;
        //    colliders[2, 2].offset = new Vector2(posX, posY);
        //}
        //if (wg.blockMap[posX + 1, posY - 1] != 0)
        //{
        //    colliders[2, 2].enabled = true;
        //    colliders[2, 2].offset = new Vector2(posX, posY);
        //}
        //if (wg.blockMap[posX - 1, posY - 1] != 0)
        //{
        //    colliders[2, 2].enabled = true;
        //    colliders[2, 2].offset = new Vector2(posX, posY);
        //}
    }
}
