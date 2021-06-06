using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    [Range(5, 20)]
    [SerializeField] float movementSpeed;
    [Range(1, 20)]
    [SerializeField] float jumpForce;
    [Range(.01f, 1)]
    [SerializeField] float jumpTime;
    [Range(10, 50f)]
    [SerializeField] float fallingSpeedCap;
    [SerializeField] Transform feetPosition;
    [SerializeField] LayerMask terrainLayer;
    [SerializeField] float reach;
    CalculateLighting cl;
    WorldGenerator wg;
    Rigidbody2D rb;
    Camera cam;


    public Tilemap backgroundTilemap;
    public Tilemap blockTilemap;
    bool isGrounded;
    bool jumping;
    float jumpTimer;

    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        wg = FindObjectOfType<WorldGenerator>();
        cl = FindObjectOfType<CalculateLighting>();
    }

    float moveInputs;

    private void FixedUpdate()
    {
        moveInputs = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInputs * movementSpeed, rb.velocity.y);
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(feetPosition.position, feetPosition.position + new Vector3(0, .1f, 0), Color.red);
        Debug.DrawLine(feetPosition.position, feetPosition.position + new Vector3(0, -.1f, 0), Color.red);
        Debug.DrawLine(feetPosition.position, feetPosition.position + new Vector3(-.1f, 0, 0), Color.red);
        Debug.DrawLine(feetPosition.position, feetPosition.position + new Vector3(.1f, 0, 0), Color.red);
    }

    void Update()
    {
        if (moveInputs > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (moveInputs < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }

        if (rb.velocity.y < -fallingSpeedCap)
        {
            rb.velocity = new Vector2(rb.velocity.x, -fallingSpeedCap);
        }

        if (Input.GetMouseButton(0))
        {
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(transform.position, mousePosition) < reach)
            {
                Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

                if (blockTilemap.GetTile(new Vector3Int(blockPosition.x, blockPosition.y, 0)) != null)
                {
                    cl.AddLightSource(blockPosition.x, blockPosition.y, true);
                }

                blockTilemap.SetTile(new Vector3Int(blockPosition.x, blockPosition.y, 0), null);
            }
        }
        else if (Input.GetMouseButton(1))
        {
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(transform.position, mousePosition) < reach)
            {
                Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

                if (blockTilemap.GetTile(new Vector3Int(blockPosition.x, blockPosition.y, 0)) == null)
                {
                    cl.RemoveLightSource(blockPosition.x, blockPosition.y);
                    cl.ModifyBlock(blockPosition.x, blockPosition.y, false);
                    wg.PlaceBlock(blockPosition.x, blockPosition.y, WorldGenerator.BlockType.dirt);
                }
            }
        }

        DoJump();
    }

    void DoJump()
    {
        isGrounded = Physics2D.OverlapBox(feetPosition.position, new Vector2(.25f, .1f), 0, terrainLayer);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            jumpTimer = 0;
            jumping = true;
            rb.velocity = Vector2.up * jumpForce;
        }
        else if (Input.GetKey(KeyCode.Space) && jumping)
        {
            //Fall
            if (jumpTimer > jumpTime)
            {
                jumping = false;
            }
            //Jump
            else
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimer += Time.deltaTime;
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            jumping = false;
        }
    }
}
