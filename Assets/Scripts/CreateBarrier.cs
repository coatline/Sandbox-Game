using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBarrier : MonoBehaviour
{
    [SerializeField] Vector2 overlap;
    [SerializeField] string layer;
    public Vector2 offset;
    public Vector2 mapSize;

    void Start()
    {
        Barrierize();
    }

    void Barrierize()
    {
        var borderHolder = new GameObject();
        borderHolder.name = "BorderHolder";

        for (int i = 0; i < 4; i++)
        {
            var border = new GameObject();
            border.name = $"Border {i + 1}";
            border.transform.parent = borderHolder.transform;

            border.AddComponent<BoxCollider2D>();
            var bc = border.GetComponent<BoxCollider2D>();
            bc.size = new Vector2((mapSize.x * 2), (mapSize.y * 2));
            border.layer = LayerMask.NameToLayer( layer);

            switch (i)
            {
                //up
                case 0: border.transform.position = new Vector3((offset.x), ((mapSize.y + (offset.y * 2)))); bc.size += new Vector2(overlap.x, 0); break;
                //down
                case 1: border.transform.position = new Vector3((offset.x), (-mapSize.y)); bc.size += new Vector2(overlap.x, 0); break;
                //right
                case 2: border.transform.position = new Vector3((mapSize.x + (offset.x * 2)), (offset.y)); bc.size += new Vector2(0, overlap.y); break;
                //left
                case 3: border.transform.position = new Vector3((-mapSize.x), (offset.y)); bc.size += new Vector2(0, overlap.y); break;
            }

        }
    }
}
