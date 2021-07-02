using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateColliders : MonoBehaviour
{
    [SerializeField] int tilemapLayer;
    [SerializeField] int checkWidth;
    [SerializeField] int checkHeight;
    WorldGenerator wg;
    Dictionary<Vector2Int, BoxCollider2D> colliders;
    List<Vector2Int> colliderPositions;
    GameObject colHolder;

    //DO NOT GENERATE COLLIDERS FOR THE MIDDLE OF THE ENTITY TO SAVE ABOUT 2-3 COLLIDER CHECKS

    void Start()
    {
        wg = FindObjectOfType<WorldGenerator>();

        //colliders = new BoxCollider2D[checkWidth, checkHeight];
        colliders = new Dictionary<Vector2Int, BoxCollider2D>();
        colliderPositions = new List<Vector2Int>();

        colHolder = new GameObject($"{gameObject.name} Surrounding Collision");
        var masterGob = GameObject.Find("Tilemap Collision").transform;
        colHolder.transform.SetParent(masterGob);
        colHolder.layer = tilemapLayer;

        for (int x = 0; x < checkWidth; x++)
        {
            for (int y = 0; y < checkHeight; y++)
            {
                if ((x == 0 && y == 0) || (x == 0 && y == checkHeight - 1) || (x == checkWidth - 1 && y == 0) || (x == checkWidth - 1 && y == checkHeight - 1)) { continue; }
                var bc = colHolder.AddComponent<BoxCollider2D>();
                bc.enabled = false;
                colliders.Add(new Vector2Int(x, y), bc);
                colliderPositions.Add(new Vector2Int(x, y));
            }
        }
    }

    Vector3 previousPosition;

    void FixedUpdate()
    {
        if (Vector2.Distance(transform.position, new Vector2Int((int)transform.position.x, (int)transform.position.y)) < .5f && wg.blockModifiedAt == -Vector2Int.one) { return; }

        int checkPosX = (int)transform.position.x - checkWidth / 2;
        int checkPosY = (int)transform.position.y - checkHeight / 2;

        for (int i = 0; i < colliderPositions.Count; i++)
        {
            int x = colliderPositions[i].x;
            int y = colliderPositions[i].y;

            if (checkPosX + x >= wg.worldWidth || checkPosY + y >= wg.worldHeight || checkPosX + x < 0 || checkPosY + y < 0) { continue; }

            BoxCollider2D bc;

            if (colliders.TryGetValue(new Vector2Int(x, y), out bc))
            {
                if (wg.blockMap[checkPosX + x, checkPosY + y, 0] != 0)
                {
                    bc.enabled = true;
                    bc.offset = new Vector2(checkPosX + x + .5f, checkPosY + y + .5f);
                }
                else
                {
                    bc.enabled = false;
                }
            }
            else
            {
                //print for a test
            }
        }

        previousPosition = transform.position;
    }

    private void OnDestroy()
    {
        Destroy(colHolder);
    }
}
