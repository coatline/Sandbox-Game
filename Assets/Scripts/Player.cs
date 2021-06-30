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

        DoJump();
    }

    void UseItem()
    {
        if (!currentItemPackage.item) { return; }

        if (currentItemPackage.item.itemType == ItemType.tool)
        {
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

            if (CanBreak(currentItemPackage.item.weaponData.toolData.worldLayer, new Vector2Int((int)mousePosition.x, (int)mousePosition.y)) && Vector2.Distance(transform.position, mousePosition) < reach)
            {
                Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

                if (wg.fgblockMap[blockPosition.x, blockPosition.y] == 0)
                {
                    wg.BreakBlock(blockPosition.x, blockPosition.y, WorldLayer.midground);
                }
                else
                {
                    wg.BreakBlock(blockPosition.x, blockPosition.y, WorldLayer.foreground);
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
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(transform.position, mousePosition) < reach)
            {
                Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

                if (blockPosition.x < 0 || blockPosition.x >= wg.worldWidth || blockPosition.y < 0 || blockPosition.y >= wg.worldHeight) { return; }

                var distance = Vector2.Distance(blockPosition, transform.position);

                if (currentItemPackage != null && distance > .8f && distance < reach && CanPlace(currentItemPackage.item.tileData.layer, new Vector2Int(blockPosition.x, blockPosition.y)))
                {
                    if (currentItemPackage.count > 0)
                    {

                        if (cursorItem)
                        {
                            wg.PlaceBlock(blockPosition.x, blockPosition.y, currentItemPackage.item);
                            cursor.UseItem(1);
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

    GameObject dynamicLight;
    ItemPackage currentItemPackage;
    bool canUse = true;
    bool cursorItem;

    IEnumerator UseTimer()
    {
        yield return new WaitForSeconds(currentItemPackage.item.useTime);
        canUse = true;
    }

    public bool swinging;
    public bool idle;

    void HandleItems()
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

        if (swinging && !itemHold.enabled)
        {
            itemHold.enabled = true;
        }

        if (currentItemPackage.item != null)
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

            if (Input.GetMouseButton(0))
            {
                itemHold.sprite = currentItemPackage.item.itemSprite;
                a.speed = 2 - (currentItemPackage.item.useTime);

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

    bool CanBreak(WorldLayer layer, Vector2Int pos)
    {
        switch (layer)
        {
            case WorldLayer.foreground: return wg.fgblockMap[pos.x, pos.y] != 0;
            case WorldLayer.midground: return wg.mgblockMap[pos.x, pos.y] != 0;
            case WorldLayer.background: return wg.bgblockMap[pos.x, pos.y] != 0;
        }
        return false;
    }

    bool CanPlace(WorldLayer layer, Vector2Int pos)
    {
        if (layer == WorldLayer.foreground)
        {
            if (wg.fgblockMap[pos.x, pos.y] == 0)
            {
                if (wg.bgblockMap[pos.x, pos.y] != 0 || wg.fgblockMap[pos.x + 1, pos.y] != 0 || wg.fgblockMap[pos.x - 1, pos.y] != 0 || wg.fgblockMap[pos.x, pos.y - 1] != 0 || wg.fgblockMap[pos.x, pos.y + 1] != 0)
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
            if (wg.mgblockMap[pos.x, pos.y] == 0 && wg.fgblockMap[pos.x, pos.y] == 0)
            {
                if (wg.bgblockMap[pos.x, pos.y] != 0 || wg.fgblockMap[pos.x + 1, pos.y] != 0 || wg.fgblockMap[pos.x - 1, pos.y] != 0 || wg.fgblockMap[pos.x, pos.y - 1] != 0 || wg.fgblockMap[pos.x, pos.y + 1] != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        if (wg.bgblockMap[pos.x, pos.y] == 0)
        {
            if (wg.bgblockMap[pos.x + 1, pos.y] != 0 || wg.bgblockMap[pos.x - 1, pos.y] != 0 || wg.bgblockMap[pos.x, pos.y - 1] != 0 || wg.bgblockMap[pos.x, pos.y + 1] != 0)
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

            inventoryManager.AddItem(pickup.itemPackage);
        }
    }
}
