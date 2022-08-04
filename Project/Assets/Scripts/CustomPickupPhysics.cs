using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPickupPhysics : MonoBehaviour
{
    BoxCollider2D bc;
    Player player;
    float speed = 2f;

    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
        player = FindObjectOfType<Player>();
    }

    void Update()
    {
        if (Vector3.Distance(player.transform.position, transform.position) < 3f)
        {
            bc.enabled = true;
        }
        else
        {
            bc.enabled = false;
        }

        speed += Time.deltaTime*3;

        if (GD.wd.blockMap[(int)transform.position.x, (int)transform.position.y, 0] == 0)
        {
            transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0), Space.World);
        }
        else
        {
            speed = 0;
        }
    }
}
