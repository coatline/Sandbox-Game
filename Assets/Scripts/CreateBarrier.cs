using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBarrier : MonoBehaviour
{
    [SerializeField] Vector2 offset;
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

            switch (i)
            {
                case 0: border.transform.position = new Vector3(0, ((mapSize.y + (offset.y)) * 2)); break;
                case 1: border.transform.position = new Vector3(0, (-mapSize.y - offset.y * 2)); break;
                case 2: border.transform.position = new Vector3((mapSize.x + (offset.x)) * 2, 0); break;
                case 3: border.transform.position = new Vector3((-mapSize.x - offset.x * 2), 0); break;
            }

            border.AddComponent<BoxCollider2D>();
            var bc = border.GetComponent<BoxCollider2D>();
            bc.size = new Vector2((mapSize.x + offset.x) * 2, (mapSize.y + offset.y) * 2);
            border.layer = LayerMask.NameToLayer("Border");
        }
    }
}
