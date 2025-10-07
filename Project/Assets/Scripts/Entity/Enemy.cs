using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [HideInInspector]
    public Player player;

    public override void Move()
    {
        MoveTowards(player.transform.position);
    }

    public void MoveTowards(Vector3 pos)
    {
        float dir = (pos - transform.position).normalized.x;
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
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
