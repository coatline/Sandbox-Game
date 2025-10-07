using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JumpBehavior : MonoBehaviour
{
    [SerializeField] Transform feetPosition;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float jumpForce;

    public bool OnGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(feetPosition.position, Vector2.down, .05f, (1 << LayerMask.NameToLayer("Blocks")));
        return (hit.collider != null);
    }

    public void Jump(float multiplier = 1)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier);
    }
}
