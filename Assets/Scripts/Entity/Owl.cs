using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Owl : Enemy
{
    enum State
    {
        preparing,
        swooping,
        hovering
    }

    [SerializeField] float targetElevation;
    [SerializeField] float swoopSideOffset;
    [SerializeField] float minSwoopDelay;
    [SerializeField] float maxSwoopDelay;
    [SerializeField] float varyingFlap;
    [SerializeField] float flapForce;
    bool canflap = true;
    Vector3 targetPos;
    [SerializeField] State state;

    private void Start()
    {
        state = State.hovering;
        StartCoroutine(PeriodicallySwoop());
    }

    public override void Move()
    {
        if (state == State.preparing)
        {
            MoveTowards(targetPos);

            if (Vector2.Distance(targetPos, transform.position) < .25f)
            {
                state = State.swooping;
            }

            if (canflap && transform.position.y < targetPos.y)
            {
                StartCoroutine(Flap());
            }
        }
        else if (state == State.swooping)
        {
            if (Vector2.Distance(player.transform.position, transform.position) < .75f)
            {
                state = State.hovering;
            }
            else if (transform.position.y < player.transform.position.y)
            {
                if (canflap)
                {
                    StartCoroutine(Flap());
                }
            }

            MoveTowards(player.transform.position);
        }
        else
        {
            if (canflap && transform.position.y - player.transform.position.y < targetElevation)
            {
                StartCoroutine(Flap());
                MoveTowards(player.transform.position);
            }
        }

    }

    IEnumerator PeriodicallySwoop()
    {
        yield return new WaitForSeconds(Random.Range(minSwoopDelay, maxSwoopDelay));

        if (state == State.preparing || state == State.swooping)
        {
            state = State.hovering;
        }
        else
        {
            state = State.preparing;

            if (Random.Range(0, 2) == 0)
            {
                targetPos = transform.position + new Vector3(swoopSideOffset, 0);
            }
            else
            {
                targetPos = transform.position + new Vector3(-swoopSideOffset, 0);
            }
        }

        StartCoroutine(PeriodicallySwoop());
    }

    IEnumerator FlapCooldown()
    {
        yield return new WaitForSeconds(.5f);
        canflap = true;
    }

    IEnumerator Flap()
    {
        rb.AddForce(new Vector2(Random.Range(-varyingFlap, varyingFlap), flapForce));
        yield return new WaitForSeconds(.05f);
        canflap = false;
        StartCoroutine(FlapCooldown());
    }
}
