using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] float invincibilityTime;
    [SerializeField] Transform feetPosition;
    [SerializeField] LayerMask msk;
    public Rigidbody2D rb;
    public int health;
    public float speed;
    private bool canMove = true;
    bool recovered = true;
    bool invincible;
    private int dir;

    public virtual void Move()
    {
        if (invincible) { return; }
        if (canMove)
        {
            rb.velocity = new Vector2(dir * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    protected void DoMovement()
    {
        if (invincible)
        {
            return;
        }
        else if (!recovered)
        {
            if (OnGround())
            {
                recovered = true;
            }
            else
            {
                return;
            }
        }

        if (rb.velocity.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (rb.velocity.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        Move();
    }

    protected IEnumerator Interval()
    {
        ChangeDirection();
        canMove = true;
        yield return new WaitForSeconds(Random.Range(1f, 6f));
        canMove = false;
        yield return new WaitForSeconds(Random.Range(0, 10f));
        StartCoroutine(Interval());
    }

    protected void ChangeDirection()
    {
        if (Random.Range(0, 2) == 0)
        {
            dir = -1;
        }
        else
        {
            dir = 1;
        }
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(feetPosition.position, feetPosition.position + Vector3.down * .05f);
    }

    public bool OnGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(feetPosition.position, Vector2.down, .05f, (1 << LayerMask.NameToLayer("Blocks")));
        return (hit.collider != null);
    }

    IEnumerator InvincibilityTimer()
    {
        invincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        invincible = false;
    }

    protected void UpdateHealthUI()
    {

    }

    public void Damage(int dmg, Vector3 dir)
    {
        if (invincible) { return; }
        else { StartCoroutine(InvincibilityTimer()); }
        recovered = false;

        // Update Health Bar
        health -= dmg;

        // Knockback
        rb.AddForce(dir * 550);

        if (health <= 0) { Kill(); }
    }

    public virtual void Start()
    {
        StartCoroutine(Interval());
        print("NPC");
    }

    void Update()
    {
        DoMovement();
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.gameObject.CompareTag("PlayerAttack"))
        {
            //Damage(player.DamageValue(), (transform.position - player.transform.position).normalized + new Vector3(0, .25f, 0));
        }
    }
}
