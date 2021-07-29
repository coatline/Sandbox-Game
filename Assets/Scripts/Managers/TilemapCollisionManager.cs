using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapCollisionManager : MonoBehaviour
{
    Queue<BoxCollider2D> collidersInQueue;
    [SerializeField] WorldGenerator wg;
    [SerializeField] WorldModifier wm;
    [SerializeField] int initialColliders;
    BoxCollider2D[,] colliders;

    void Start()
    {
        collidersInQueue = new Queue<BoxCollider2D>();
        colliders = new BoxCollider2D[wg.worldWidth, wg.worldHeight];

        for (int i = 0; i < initialColliders; i++)
        {
            CreateCollider();
        }
    }

    void CreateCollider()
    {
        var col = gameObject.AddComponent<BoxCollider2D>();
        col.enabled = false;
        collidersInQueue.Enqueue(col);
    }

    public void DestroyEntity(Vector2Int pos)
    {
        for (int x = pos.x - 1; x <= pos.x + 1; x++)
        {
            for (int y = pos.y - 1; y <= pos.y + 1; y++)
            {
                //get the list of cols and disable all of them as long as no entity is around
                UpdateColliderAt(x, y);
            }
        }
    }

    void RemoveColliderAt(int x, int y)
    {
        colliders[x, y].enabled = false;
        collidersInQueue.Enqueue(colliders[x, y]);
        colliders[x, y] = null;
    }

    public void UpdateColliderAt(int x, int y, bool remove = false)
    {
        if (x < 0 || x >= wg.worldWidth || y < 0 || y >= wg.worldHeight) { return; }

        if (GD.wd.blockMap[x, y, 0] == 0)
        {
            if (colliders[x, y] != null)
            {
                RemoveColliderAt(x, y);
            }
        }
        else if (!remove)
        {
            if (colliders[x, y] == null)
            {
                //print(collidersInQueue.Count);
                AddColliderAt(x, y);
            }
        }
    }

    public void AddColliderAt(int x, int y)
    {
        if (collidersInQueue.Count <= 0)
        {
            CreateCollider();
        }

        var col = collidersInQueue.Dequeue();
        colliders[x, y] = col;
        col.enabled = true;
        col.offset = new Vector2(x, y) + (Vector2.one / 2);
    }

    void FixedUpdate()
    {
        if (wm.blockModifiedAt == -Vector2Int.one) { return; }

        var x = wm.blockModifiedAt.x;
        var y = wm.blockModifiedAt.y;

        UpdateColliderAt(x, y);

        wm.blockModifiedAt = -Vector2Int.one;
    }
}
