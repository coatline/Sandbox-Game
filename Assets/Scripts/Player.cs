using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [Range(5, 100)]
    [SerializeField] float movementSpeed;
    [Range(1, 20)]
    [SerializeField] float jumpForce;
    [Range(.01f, 1)]
    [SerializeField] float jumpTime;
    [Range(10, 50f)]
    [SerializeField] float fallingSpeedCap;
    [SerializeField] Transform feetPosition;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float reach;
    [SerializeField] ItemDataContainer dirt;
    [SerializeField] ItemDataContainer torch;
    InventoryManager inventoryManager;
    CursorBehavior cursor;
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
        cursor = FindObjectOfType<CursorBehavior>();
        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    float moveInputs;

    private void FixedUpdate()
    {
        moveInputs = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInputs * movementSpeed, rb.velocity.y);

        if (rb.velocity.y < -fallingSpeedCap)
        {
            rb.velocity = new Vector2(rb.velocity.x, -fallingSpeedCap);
        }
    }

    Vector3Int previousPosAndIndex;
    bool extinguished;
    bool lit;

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

        ItemDataContainer currentItem = inventoryManager.CurrentItem();

        if(cursor.currentItem)
        {
            currentItem = cursor.currentItem;
        }

        if (currentItem && currentItem.emitsLight)
        {
            if (lit)
            {
                wg.mgblockMap[previousPosAndIndex.x, previousPosAndIndex.y] = (short)previousPosAndIndex.z;
            }

            lit = true;
            Vector2Int blockPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            previousPosAndIndex = new Vector3Int(blockPosition.x, blockPosition.y, wg.mgblockMap[blockPosition.x, blockPosition.y]);
            wg.mgblockMap[blockPosition.x, blockPosition.y] = torch.id;
            extinguished = false;
        }
        else if (!extinguished)
        {
            wg.mgblockMap[previousPosAndIndex.x, previousPosAndIndex.y] = (short)previousPosAndIndex.z;
            extinguished = true;
            lit = false;
        }

        //just have a list of objects that emit light and look at those

        if (Input.GetMouseButton(0))
        {
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(transform.position, mousePosition) < reach)
            {
                Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

                {
                    if (wg.fgblockMap[blockPosition.x, blockPosition.y] == 0)
                    {
                        wg.BreakBlock(blockPosition.x, blockPosition.y, WorldLayer.midground);
                    }
                    else
                    {
                        wg.BreakBlock(blockPosition.x, blockPosition.y, WorldLayer.foreground);
                    }
                }
            }
        }
        else if (Input.GetMouseButton(1))
        {
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(transform.position, mousePosition) < reach)
            {
                Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

                if (blockPosition.x < 0 || blockPosition.x >= wg.worldWidth || blockPosition.y < 0 || blockPosition.y >= wg.worldHeight) { return; }


                if (currentItem != null && Vector2.Distance(blockPosition, transform.position) > .8f && CanPlace(currentItem.tileData.layer, new Vector3Int(blockPosition.x, blockPosition.y, 0)))
                {
                    //probably do not need to do this
                    if (inventoryManager.CurrentSlot().count > 0)
                    {
                        wg.PlaceBlock(blockPosition.x, blockPosition.y, currentItem);
                        inventoryManager.CurrentSlot().RemoveItem(1);
                    }
                }
            }
        }

        DoJump();
    }

    bool CanPlace(WorldLayer layer, Vector3Int pos)
    {
        var air = 0;

        if (layer == WorldLayer.foreground)
        {
            if (wg.fgblockMap[pos.x, pos.y] == air)
            {
                if (wg.bgblockMap[pos.x, pos.y] != air || wg.fgblockMap[pos.x + 1, pos.y] != air || wg.fgblockMap[pos.x - 1, pos.y] != air || wg.fgblockMap[pos.x, pos.y - 1] != air || wg.fgblockMap[pos.x, pos.y + 1] != air)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        if (layer == WorldLayer.midground)
        {
            if (wg.mgblockMap[pos.x, pos.y] == air && wg.fgblockMap[pos.x,pos.y]==air)
            {
                if (wg.bgblockMap[pos.x, pos.y] != air || wg.fgblockMap[pos.x + 1, pos.y] != air || wg.fgblockMap[pos.x - 1, pos.y] != air || wg.fgblockMap[pos.x, pos.y - 1] != air || wg.fgblockMap[pos.x, pos.y + 1] != air)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        if (wg.bgblockMap[pos.x, pos.y] == air)
        {
            if (wg.bgblockMap[pos.x + 1, pos.y] != air || wg.bgblockMap[pos.x - 1, pos.y] != air || wg.bgblockMap[pos.x, pos.y - 1] != air || wg.bgblockMap[pos.x, pos.y + 1] != air)
            {
                return true;
            }
        }

        return false;
    }

    void DoJump()
    {
        isGrounded = Physics2D.OverlapBox(feetPosition.position, new Vector2(.55f, .4f), 0, groundLayer);

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Pickup") && GetComponent<Collider2D>().IsTouching(collision))
        {
            Pickup pickup = collision.gameObject.GetComponent<Pickup>();

            //Send this to a pool instead
            Destroy(collision.gameObject);
            inventoryManager.AddItem(pickup.itemData, pickup.count);
        }
    }
}
