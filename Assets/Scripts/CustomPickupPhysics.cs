using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPickupPhysics : MonoBehaviour
{
    BoxCollider2D bc;
    WorldGenerator wg;
    Player player;

    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
        player = FindObjectOfType<Player>();
        wg = FindObjectOfType<WorldGenerator>();
    }

    void Update()
    {
        //transform.position

        if (Vector3.Distance(player.transform.position, transform.position) < 3f)
        {
            bc.enabled = true;
        }
        else
        {
            bc.enabled = false;
        }

        if (wg.blockMap[(int)transform.position.x, (int)transform.position.y, 0] == 0)
        {
            transform.Translate(new Vector3(0, -1 * Time.deltaTime, 0));
        }
    }
}
