using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Entity : MonoBehaviour
{
    [SerializeField] Transform feetPosition;
    public Collider2D worldCollider;
    public Rigidbody2D rb;

    [Range(10, 50f)]
    [SerializeField] float fallingSpeedCap;
    [SerializeField] float invincibilityTime;
    public int health;
    public float speed;
    bool recovered = true;
    bool invincible;

    protected void FaceMovement()
    {
        if (rb.velocity.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (rb.velocity.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    protected void Orient()
    {
        if (rb.velocity.y < -fallingSpeedCap)
        {
            rb.velocity = new Vector2(rb.velocity.x, -fallingSpeedCap);
        }

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

        FaceMovement();
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

    public virtual void UpdateHealthUI()
    {

    }

    public virtual void Move() { }

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

    void FixedUpdate()
    {
        Orient();
        Move();
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.gameObject.CompareTag("PlayerAttack"))
        {
            //Damage(player.DamageValue(), (transform.position - player.transform.position).normalized + new Vector3(0, .25f, 0));
        }
    }
}
