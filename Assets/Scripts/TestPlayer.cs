using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Tilemaps;

public class TestPlayer : MonoBehaviour
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
    CalculateColorLighting ccl;
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
        ccl = FindObjectOfType<CalculateColorLighting>();
    }

    float moveInputs;

    private void FixedUpdate()
    {
        moveInputs = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInputs * movementSpeed, rb.velocity.y);
    }

    byte currentblocktype=6;
    bool placingTorch = false;

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

        if (Input.GetKeyDown(KeyCode.E))
        {
            placingTorch = !placingTorch;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentblocktype = 6;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentblocktype = 7;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentblocktype = 8;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentblocktype = 9;
        }

        if (Input.GetMouseButton(0))
        {
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(transform.position, mousePosition) < reach)
            {
                Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

                //why do i do this check?
                if (blockTilemap.GetTile(new Vector3Int(blockPosition.x, blockPosition.y, 0)) != null)
                {
                    wg.ModifyBlock(blockPosition.x, blockPosition.y, 0);
                    //ccl.AddLightSource(blockPosition.x, blockPosition.y, false);
                }
            }
        }
        else if (Input.GetMouseButton(1))
        {
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(transform.position, mousePosition) < reach)
            {
                Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

                if (placingTorch)
                {
                    //wg.blockMap[blockPosition.x, blockPosition.y] = currentblocktype;
                    wg.ModifyBlock(blockPosition.x, blockPosition.y, currentblocktype);
                    //ccl.AddLightSource(blockPosition.x, blockPosition.y, true);
                }
                else
                {
                    //if (Vector2.Distance(blockPosition, transform.position) > .8f && CanPlace(0, new Vector3Int(blockPosition.x, blockPosition.y, 0)))
                    {
                        wg.ModifyBlock(blockPosition.x, blockPosition.y,2);
                        //ccl.RemoveLightSource(blockPosition.x, blockPosition.y);
                    }
                }
            }
        }

        DoJump();
    }

    //0-1-2
    bool CanPlace(int depth, Vector3Int pos)
    {
        var air =0;

        if (depth == 0 && wg.blockMap[pos.x, pos.y] == air)
        {
            if (wg.blockMap[pos.x + 1, pos.y] != air)
            {
                return true;
            }
            else if (wg.blockMap[pos.x - 1, pos.y] != air)
            {
                return true;
            }
            else if (wg.blockMap[pos.x, pos.y - 1] != air)
            {
                return true;
            }
            else if (wg.blockMap[pos.x, pos.y + 1] != air)
            {
                return true;
            }
        }

        return false;
    }

    void DoJump()
    {
        isGrounded = Physics2D.OverlapBox(feetPosition.position, new Vector2(.55f, .25f), 0, terrainLayer);

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
