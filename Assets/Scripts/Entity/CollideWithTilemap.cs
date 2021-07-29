using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideWithTilemap : MonoBehaviour
{
    List<Vector2Int> relativeColliderPositions;
    public List<Vector2Int> activeColliders;
    [SerializeField] int checkWidth;
    [SerializeField] int checkHeight;
    TilemapCollisionManager tcm;

    private void Start()
    {
        tcm = FindObjectOfType<TilemapCollisionManager>();
        relativeColliderPositions = new List<Vector2Int>();
        activeColliders = new List<Vector2Int>();

        for (int x = 0; x < checkWidth; x++)
        {
            for (int y = 0; y < checkHeight; y++)
            {
                relativeColliderPositions.Add(new Vector2Int(x, y));
            }
        }
    }

    Vector2 previousPos;

    void FixedUpdate()
    {
        if (Vector2.Distance(previousPos, transform.position) > .5f)
        {
            int checkPosX = (int)transform.position.x - checkWidth / 2;
            int checkPosY = (int)transform.position.y - checkHeight / 2;

            for (int i = 0; i < relativeColliderPositions.Count; i++)
            {
                int x = relativeColliderPositions[i].x;
                int y = relativeColliderPositions[i].y;

                tcm.UpdateColliderAt(x + checkPosX, y + checkPosY);
            }

            previousPos = transform.position;
        }
    }




















}

//[SerializeField] int checkWidth;
//[SerializeField] int checkHeight;
//TilemapCollisionManager tcm;
//List<Vector2Int> relativeColliderPositions;
//List<Vector2Int> previousWorldColliderPositions;


//void Start()
//{
//    relativeColliderPositions = new List<Vector2Int>();
//    previousWorldColliderPositions = new List<Vector2Int>();
//    tcm = FindObjectOfType<TilemapCollisionManager>();
//    //tcm.MoveTo(new Vector2Int((int)transform.position.x, (int)transform.position.y), Vector2Int.zero, true);

//    for (int x = 0; x < checkWidth; x++)
//    {
//        for (int y = 0; y < checkHeight; y++)
//        {
//            if ((x == 0 && y == 0) || (x == 0 && y == checkHeight - 1) || (x == checkWidth - 1 && y == 0) || (x == checkWidth - 1 && y == checkHeight - 1)) { continue; }
//            //var bc = g.AddComponent<BoxCollider2D>();
//            //bc.enabled = false;
//            //tcm.AddColliderAt(x + (int)transform.position.x, y + (int)transform.position.y);
//            relativeColliderPositions.Add(new Vector2Int(x, y));
//        }
//    }
//}

//Vector2 previousPos;

//void FixedUpdate()
//{
//    if (Vector2.Distance(previousPos, transform.position) > .4f)
//    {

//        int checkPosX = (int)transform.position.x - checkWidth / 2;
//        int checkPosY = (int)transform.position.y - checkHeight / 2;

//        //for (int i = 0; i < worldColliderPositions.Count; i++)
//        //{
//        //    int x = worldColliderPositions[i].x;
//        //    int y = worldColliderPositions[i].y;

//        //    tcm.UpdateColliderAt(x, y);
//        //}
//        for (int j = 0; j < previousWorldColliderPositions.Count; j++)
//        {
//            tcm.UpdateColliderAt(previousWorldColliderPositions[j].x, previousWorldColliderPositions[j].y, true);
//        }

//        previousWorldColliderPositions.Clear();

//        for (int i = 0; i < relativeColliderPositions.Count; i++)
//        {
//            int x = relativeColliderPositions[i].x;
//            int y = relativeColliderPositions[i].y;

//            previousWorldColliderPositions.Add(new Vector2Int(x + checkPosX, y + checkPosY));

//            tcm.UpdateColliderAt(x + checkPosX, y + checkPosY);
//        }

//        previousPos = transform.position;

//        //tcm.MoveTo(position, previousPos);
//    }
//}

//private void OnDestroy()
//{
//    if (!tcm) { return; }
//    tcm.DestroyEntity(new Vector2Int((int)transform.position.x, (int)transform.position.y));
//}