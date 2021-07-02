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
    [SerializeField] SpriteRenderer itemHold;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float reach;
    [SerializeField] Animation swingAnimation;
    InventoryManager inventoryManager;
    CursorBehavior cursor;
    RenderLighting rl;
    WorldGenerator wg;
    Rigidbody2D rb;
    Camera cam;
    Animator a;
    int swingAnimationHash = Animator.StringToHash("PlayerSwing");

    public Tilemap backgroundTilemap;
    public Tilemap blockTilemap;
    bool isGrounded;
    bool jumping;
    float jumpTimer;

    void Start()
    {
        cam = Camera.main;
        a = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rl = FindObjectOfType<RenderLighting>();
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

    bool lit;

    void Update()
    {
        FaceDirection();

        HandleItems();

        HandleInputs();
    }


    GameObject dynamicLight;
    ItemPackage currentItemPackage;
    bool canUse = true;
    bool cursorItem;

    IEnumerator UseTimer()
    {
        float useTime=0;

        if (currentItemPackage.item != null)
        {
            useTime = currentItemPackage.item.useTime;
        }
        else
        {
            yield return null;
        }

        yield return new WaitForSeconds(useTime);
        canUse = true;
    }

    public bool swinging;
    public bool idle;

    void UseItem()
    {
        if (!currentItemPackage.item) { return; }

        var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

        if (currentItemPackage.item.itemType == ItemType.tool)
        {
            if (Vector2.Distance(transform.position, mousePosition) < reach)
            {
                if (blockPosition.x < 0 || blockPosition.x >= wg.worldWidth || blockPosition.y < 0 || blockPosition.y >= wg.worldHeight) { return; }

                if (wg.CanBreak(blockPosition.x, blockPosition.y, currentItemPackage.item.weaponData.toolData.toolType))
                {
                    if (wg.blockMap[blockPosition.x, blockPosition.y, 0] == 0)
                    {
                        wg.BreakBlock(blockPosition.x, blockPosition.y, 1);
                    }
                    else
                    {
                        wg.BreakBlock(blockPosition.x, blockPosition.y, 0);
                    }
                }
            }
            else
            {
                canUse = true;
                return;
            }
        }
        else if (currentItemPackage.item.itemType == ItemType.block)
        {
            if (Vector2.Distance(transform.position, mousePosition) < reach)
            {
                if (blockPosition.x < 0 || blockPosition.x >= wg.worldWidth || blockPosition.y < 0 || blockPosition.y >= wg.worldHeight) { return; }

                var distance = Vector2.Distance(blockPosition, transform.position);

                if (currentItemPackage != null && distance > .8f && distance < reach && wg.CanPlace(blockPosition.x, blockPosition.y, currentItemPackage.item))
                {
                    if (currentItemPackage.count > 0)
                    {
                        if (cursorItem)
                        {
                            wg.PlaceBlock(blockPosition.x, blockPosition.y, currentItemPackage.item);
                            //cursor.UseItem(1);
                        }
                        else
                        {
                            wg.PlaceBlock(blockPosition.x, blockPosition.y, currentItemPackage.item);
                            inventoryManager.CurrentSlot().RemoveItem(1);
                        }
                    }
                }
                else
                {
                    canUse = true;
                    return;
                }
            }
        }

        canUse = false;
        StartCoroutine(UseTimer());
    }

    void SetCurrentItem()
    {
        if (cursor.itemPackage.item != null)
        {
            currentItemPackage = cursor.itemPackage;
            cursorItem = true;
        }
        else
        {
            currentItemPackage = inventoryManager.CurrentItemPackage();
        }
    }

    void DoDynamicLighting()
    {
        if (currentItemPackage.item.emitsLight)
        {
            if (!lit)
            {
                dynamicLight = new GameObject("Light");
                dynamicLight.transform.SetParent(transform);
                dynamicLight.transform.position = itemHold.transform.position;
                rl.dynamicLightEmitters.Add(dynamicLight.transform);
                lit = true;
            }
        }
        else if (lit)
        {
            Destroy(dynamicLight);
            lit = false;
        }
    }

    void DisplayHand()
    {
        if (currentItemPackage.item.showOnSelect && idle)
        {
            if (!itemHold.enabled)
            {
                itemHold.enabled = true;
            }

            itemHold.sprite = currentItemPackage.item.selectedSprite;
        }
        else if (!swinging && itemHold.enabled)
        {
            itemHold.enabled = false;
        }
    }

    void HandleItems()
    {
        SetCurrentItem();

        if (currentItemPackage.item != null)
        {
            if (swinging && !itemHold.enabled && !currentItemPackage.item.hideOnUse)
            {
                itemHold.enabled = true;
            }

            DisplayHand();

            DoDynamicLighting();

            if (Input.GetMouseButton(0))
            {
                itemHold.sprite = currentItemPackage.item.itemSprite;
                a.speed = 4 - (currentItemPackage.item.useTime * 8);

                if (!swinging)
                {
                    a.Play(swingAnimationHash);
                }

                if (canUse)
                {
                    UseItem();
                }
            }
        }
        else
        {
            if (!swinging && itemHold.enabled)
            {
                // No item do not show anything in hand

                itemHold.enabled = false;
            }
            else if (lit)
            {
                lit = false;
                Destroy(dynamicLight);
            }
        }
    }

    void HandleInputs()
    {
        DoJump();

        if (Input.GetKeyDown(KeyCode.F))
        {
            transform.position = new Vector3(wg.worldWidth / 2, wg.highestTiles[wg.worldWidth / 2] + 2);
        }
    }

    void FaceDirection()
    {
        if (moveInputs > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (moveInputs < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
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

            inventoryManager.AddItem(pickup.itemPackage);
        }
    }
}
