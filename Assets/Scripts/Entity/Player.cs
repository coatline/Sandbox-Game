using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(JumpBehavior))]
[RequireComponent(typeof(HandleItem))]

public class Player : Entity
{
    [SerializeField] JumpBehavior jb;

    [Range(.01f, 1)]
    [SerializeField] float jumpTime;

    [SerializeField] PlayerHandleItem hi;
    [SerializeField] float reach;

    InventoryManager inventoryManager;
    ChestInventory cim;
    CursorBehavior cursor;
    WorldModifier wm;
    Camera cam;

    float jumpTimer;
    float moveInputs;
    InventorySlot currentSlot;
    bool cursorItem;
    bool jumping;

    protected void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        wm = FindObjectOfType<WorldModifier>();
        cursor = FindObjectOfType<CursorBehavior>();
        cim = FindObjectOfType<ChestInventory>();
        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    void Update()
    {
        CheckChestDist();
        Inputs();
    }

    [HideInInspector]
    public Vector2Int currentChest;

    void CheckChestDist()
    {
        if (currentChest == -Vector2Int.one) { return; }

        if (!CanBeUsingChest(currentChest))
        {
            cim.CloseChest();
        }
    }

    public int DamageValue()
    {
        WeaponData weaponData = null;

        if (hi.currentItemPackage.item && hi.currentItemPackage.item.weaponData.damageVal > 0)
        {
            weaponData = hi.currentItemPackage.item.weaponData;
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

    public void SetItemAndSlot(ItemPackage newItem, InventorySlot newSlot = null)
    {
        hi.ChangeItem(newItem);

        if (newSlot)
        {
            hi.currentSlot = newSlot;
        }

        UpdateCursorImage();
    }

    void UpdateCursorImage()
    {
        if (hi.currentItemPackage.item != null)
        {
            if (cursor)
            {
                if (hi.currentItemPackage.item.itemData.showPlacement)
                {
                    cursor.DisplayPlacement(hi.currentItemPackage.item.itemData.heldSprite);
                }
                else
                {
                    cursor.EndDisplayPlacement();
                }
            }
        }
        else
        {
            cursor.EndDisplayPlacement();
        }
    }

    void RightClick()
    {
        if (!OverActiveUI())
        {
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int blockPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

            if (CanBeUsingChest(blockPosition))
            {
                wm.TryInteractAt(blockPosition.x, blockPosition.y, ref currentChest);
            }
        }
    }

    void LeftClick()
    {
        if (!OverActiveUI())
        {
            Vector3 mpos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int pos = new Vector2Int((int)mpos.x, (int)mpos.y);
            hi.UseItem(pos);

            UpdateCursorImage();
        }
    }

    bool CanBeUsingChest(Vector2Int chestPos)
    {
        if (Vector2.Distance(transform.position, chestPos) > reach)
        {
            return false;
        }

        return true;
    }

    bool OverActiveUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0)
            {
                foreach (var go in raycastResults)
                {
                    if (go.gameObject.activeSelf)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void JumpInputs()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jb.OnGround())
            {
                jumpTimer = 0;
                jumping = true;

                jb.Jump();
            }
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            if (jumping)
            {
                // Fall
                if (jumpTimer > jumpTime)
                {
                    jumping = false;
                }

                // Jump
                else
                {
                    jb.Jump();
                    jumpTimer += Time.deltaTime;
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            jumping = false;
        }
    }

    void Inputs()
    {
        moveInputs = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInputs * speed, rb.velocity.y);

        if (Input.GetMouseButton(0))
        {
            LeftClick();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            RightClick();
        }

        JumpInputs();
    }
}
