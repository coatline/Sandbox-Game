using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [HideInInspector]
    public Player player;

    public override void Start()
    {
    }

    void Update()
    {
        DoMovement();
    }

    public override void Move()
    {
        float dir = (player.transform.position - transform.position).normalized.x;
        rb.velocity = new Vector2(dir * speed, rb.velocity.y);
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerAttack"))
        {
            var dir = (transform.position - player.transform.position).normalized;
            dir.y = Mathf.Clamp(dir.y, .4f, 1);
            Damage(player.DamageValue(), dir);
        }
    }
}
