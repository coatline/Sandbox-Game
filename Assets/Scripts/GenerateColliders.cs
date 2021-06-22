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
    [SerializeField] int entityWidth;
    [SerializeField] int entityHeight;
    //BoxCollider2D[,] colliders;
    Dictionary<Vector2Int, BoxCollider2D> colliders;
    List<Vector2Int> colliderPositions;
    //DO NOT GENERATE COLLIDERS FOR THE MIDDLE OF THE ENTITY TO SAVE ABOUT 2-3 COLLIDER CHECKS

    void Start()
    {
        wg = FindObjectOfType<WorldGenerator>();

        //colliders = new BoxCollider2D[checkWidth, checkHeight];
        colliders = new Dictionary<Vector2Int, BoxCollider2D>();
        colliderPositions = new List<Vector2Int>();

        var g = new GameObject($"{gameObject.name} Surrounding Collision");
        var masterGob = GameObject.Find("Tilemap Collision").transform;
        g.transform.SetParent(masterGob);
        g.layer = tilemapLayer;

        for (int x = 0; x < checkWidth; x++)
        {
            for (int y = 0; y < checkHeight; y++)
            {
                if((x==0&&y==0)|| (x == 0 && y == checkHeight - 1)|| (x == checkWidth - 1 && y == 0)|| (x == checkWidth - 1 && y == checkHeight - 1)) { continue; }
                var bc = g.AddComponent<BoxCollider2D>();
                bc.enabled = false;
                colliders.Add(new Vector2Int(x, y), bc);
                colliderPositions.Add(new Vector2Int(x, y));
                //colliders[x, y] = bc;
            }
        }

        //for (int x = 0; x < entityWidth; x++)
        //{
        //    for (int y = 0; y < entityHeight; y++)
        //    {
        //        transform.position/2
        //    }
        //}
    }

    Vector3 previousPosition;

    void FixedUpdate()
    {
        if (transform.position == previousPosition && !wg.blockModified) { return; }

        int checkPosX = (int)transform.position.x - checkWidth / 2;
        int checkPosY = (int)transform.position.y - checkHeight / 2;

        //for (int x = 0; x < checkWidth; x++)
        //{
        //    for (int y = 0; y < checkHeight; y++)
        //    {
        for (int i = 0; i < colliderPositions.Count; i++)
        {
            int x = colliderPositions[i].x;
            int y = colliderPositions[i].y;

            if (checkPosX + x >= wg.worldWidth || checkPosY + y >= wg.worldHeight || checkPosX < 0 || checkPosY < 0) { continue; }

            BoxCollider2D bc;

            if (colliders.TryGetValue(new Vector2Int(x, y), out bc))
            {
                if (wg.fgblockMap[checkPosX + x, checkPosY + y] != 0)
                {
                    bc.enabled = true;
                    bc.offset = new Vector2(checkPosX + x + .5f, checkPosY + y + .5f);
                }
                else
                {
                    bc.enabled = false;
                }
            }
        }
        //if (wg.fgblockMap[checkPosX + x, checkPosY + y] != 0)
        //{
        //    colliders[x, y].offset = new Vector2(checkPosX + x + .5f, checkPosY + y + .5f);
        //    colliders[x, y].enabled = true;
        //}
        //else
        //{
        //    colliders[x, y].enabled = false;
        //}
        //    }
        //}

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
