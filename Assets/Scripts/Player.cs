using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] PolygonCollider2D itemHoldCollider;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float reach;
    InventoryManager inventoryManager;
    CursorBehavior cursor;
    RenderLighting rl;
    WorldGenerator wg;
    AudioSource audioS;
    Rigidbody2D rb;
    Camera cam;
    Animator a;
    int swingAnimationHash = Animator.StringToHash("PlayerSwing");

    public Tilemap backgroundTilemap;
    public Tilemap blockTilemap;
    bool isGrounded;
    bool jumping;
    float jumpTimer;
    float moveInputs;
    GameObject dynamicLight;
    ItemPackage currentItemPackage;
    InventorySlot currentSlot;
    bool canUse = true;
    bool cursorItem;
    bool timerRunning;

    void Start()
    {
        cam = Camera.main;
        a = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioS = GetComponent<AudioSource>();
        rl = FindObjectOfType<RenderLighting>();
        wg = FindObjectOfType<WorldGenerator>();
        cursor = FindObjectOfType<CursorBehavior>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        //currentItemPackage = new ItemPackage(null, 0);
    }

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

    IEnumerator UseTimer()
    {
        float useTime = 0;
        timerRunning = true;

        if (currentItemPackage.item != null)
        {
            useTime = currentItemPackage.item.itemData.useTime;
        }
        else
        {
            yield return null;
        }

        yield return new WaitForSeconds(useTime);
        timerRunning = false;
        canUse = true;
    }

    public bool swinging;
    public bool idle;

    public int DamageValue()
    {
        WeaponData weaponData = null;

        if (currentItemPackage.item && currentItemPackage.item.weaponData.damageVal > 0)
        {
            weaponData = currentItemPackage.item.weaponData;
        }
        else
        {
            return 0;
        }


        float multiplier = 1f;

        if (Random.Range(0, 100f) <= weaponData.criticalStrikeChance)
        {
            multiplier = Random.Range(1.5f, 3f);
        }

        return (int)(weaponData.damageVal * multiplier);
    }

    void UseItem()
    {
        if (!currentItemPackage.item || currentItemPackage.item.itemType == ItemType.meleeWeapon) { return; }

        var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);
        ItemDataConatainer currentItem = currentItemPackage.item;
        bool used = false;

        if (currentItemPackage.item.itemType == ItemType.tool)
        {
            if (Vector2.Distance(transform.position, mousePosition) < reach)
            {
                if (blockPosition.x < 0 || blockPosition.x >= wg.worldWidth || blockPosition.y < 0 || blockPosition.y >= wg.worldHeight) { return; }

                short canBreak = wg.CanBreak(blockPosition.x, blockPosition.y, currentItemPackage.item.weaponData.toolData.toolType);

                if (canBreak >= 0)
                {
                    wg.DamageBlock(blockPosition.x, blockPosition.y, (byte)canBreak, currentItemPackage.item.weaponData.toolData.strength);
                    used = true;
                }
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
                            cursor.UseItem(1);
                        }
                        else
                        {
                            wg.PlaceBlock(blockPosition.x, blockPosition.y, currentItemPackage.item);
                            currentSlot.RemoveCount(1);
                        }

                        used = true;
                    }
                }
            }
        }


        if (used && !timerRunning)
        {
            if (currentItem.itemData.useSound)
            {
                audioS.PlayOneShot(currentItem.itemData.useSound.sound.RandomSound());
            }

            canUse = false;
            StartCoroutine(UseTimer());
        }
    }

    public void SetItemAndSlot(ItemPackage newItem, InventorySlot newSlot = null)
    {
        currentItemPackage = newItem;

        if (newSlot)
        {
            currentSlot = newSlot;
        }

        if (currentItemPackage.item != null)
        {
            if (currentItemPackage.item.itemData.generateCollider)
            {
                itemHoldCollider.enabled = true;
                List<Vector2> shape = new List<Vector2>();
                currentItemPackage.item.itemData.itemSprite.GetPhysicsShape(0, shape);
                itemHoldCollider.SetPath(0, shape);
            }
            else
            {
                itemHoldCollider.enabled = false;
            }
        }

        //if (cursor.itemPackage.item != null)
        //{
        //    currentItemPackage = cursor.itemPackage;
        //    cursorItem = true;
        //}
        //else
        //{
        //    currentItemPackage = inventoryManager.CurrentItemPackage();
        //}
    }

    void DoDynamicLighting()
    {
        if (currentItemPackage.item.itemData.emitsLight)
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
        if (currentItemPackage.item.itemData.showOnSelect && idle)
        {
            if (!itemHold.enabled)
            {
                itemHold.enabled = true;
            }

            itemHold.sprite = currentItemPackage.item.itemData.heldSprite;
        }
        else if (!swinging && itemHold.enabled)
        {
            itemHold.enabled = false;
        }
    }

    void HandleItems()
    {
        if (currentItemPackage.item != null)
        {
            if (swinging && !itemHold.enabled && !currentItemPackage.item.itemData.hideOnUse)
            {
                itemHold.enabled = true;
            }

            if (swinging && currentItemPackage.item.itemData.generateCollider)
            {
                itemHoldCollider.enabled = true;
            }
            else
            {
                itemHoldCollider.enabled = false;
            }

            DisplayHand();

            DoDynamicLighting();

            if (Input.GetMouseButton(0))
            {
                itemHold.sprite = currentItemPackage.item.itemData.itemSprite;
                a.speed = 7f - (currentItemPackage.item.itemData.useTime * 10);

                if (currentItemPackage.item.actionOnUse == PlayerAction.swing && !swinging)
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
        isGrounded = Physics2D.OverlapBox(feetPosition.position, new Vector2(.725f, .4f), 0, groundLayer);

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
