using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleItem : MonoBehaviour
{
    [SerializeField] PolygonCollider2D itemHoldCollider;
    [SerializeField] SpriteRenderer itemHold;
    [SerializeField] AnimationClip swingAnimation;
    [SerializeField] AnimationClip shootAnimation;
    [SerializeField] AudioSource audioS;
    [SerializeField] Animator a;
    [SerializeField] Entity e;
    [Range(1, 8)]
    [SerializeField] float reach;
    [HideInInspector]
    public ItemPackage currentItemPackage;
    GameObject dynamicLight;
    RenderLighting rl;
    WorldModifier wm;
    public bool animating;
    bool lit;

    public void ChangeItem(ItemPackage item)
    {
        currentItemPackage = item;
        UpdateHand();
    }

    public void DropItem()
    {
        UpdateHand();
    }

    void UpdateHand()
    {
        if (!currentItemPackage.item)
        {
            itemHold.sprite = null;
            return;
        }

        DoDynamicLighting();
        GenerateCollider();
        itemHold.sprite = currentItemPackage.item.itemData.itemSprite;
    }

    void DoDynamicLighting()
    {
        if (currentItemPackage.item)
        {
            if (currentItemPackage.item.itemData.emitsLight)
            {
                if (!lit)
                {
                    if (!rl) { rl = FindObjectOfType<RenderLighting>(); }

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
    }

    void Update()
    {
        DisplayHand();
    }

    void DisplayHand()
    {
        if (!currentItemPackage.item)
        {
            itemHold.enabled = false;
            return;
        }

        if (animating)
        {
            if (currentItemPackage.item.itemData.hideOnUse)
            {
                itemHold.enabled = false;
            }
            else
            {
                itemHold.enabled = true;
            }

            itemHoldCollider.enabled = true;
        }
        else
        {
            if (currentItemPackage.item.itemData.showOnSelect)
            {
                itemHold.sprite = currentItemPackage.item.itemData.heldSprite;
                itemHold.enabled = true;
            }
            else
            {
                itemHold.enabled = false;
            }

            itemHoldCollider.enabled = false;
        }

    }

    void GenerateCollider()
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

    void ShowUse()
    {
        a.speed = (swingAnimation.length / currentItemPackage.item.itemData.animationTime);

        if (currentItemPackage.item.actionOnUse == PlayerAction.swing && !animating)
        {
            a.Play(swingAnimation.name);
        }
        else if (currentItemPackage.item.actionOnUse == PlayerAction.shoot)
        {
            a.Play(shootAnimation.name);
        }
    }

    public virtual ItemType UseItem(Vector2Int position)
    {
        if (!currentItemPackage.item)
        {
            return ItemType.none;
        }

        ShowUse();

        if (currentItemPackage.item.itemType == ItemType.meleeWeapon)
        {
            return ItemType.meleeWeapon;
        }

        if (!canUse)
        {
            return ItemType.none;
        }

        ItemType type = ItemType.none;
        ItemDataContainer currentItem = currentItemPackage.item;
        bool used = false;

        if (currentItemPackage.item.itemType == ItemType.tool)
        {
            if (Vector2.Distance(transform.position, position) < reach)
            {
                if (!wm) { wm = FindObjectOfType<WorldModifier>(); }

                short canBreak = wm.CanBreak(position.x, position.y, currentItemPackage.item.weaponData.toolData.toolType);
                if (canBreak >= 0)
                {
                    wm.HitBlock(position.x, position.y, (byte)canBreak, currentItemPackage.item.weaponData.toolData.strength);
                    type = ItemType.tool;
                    used = true;
                }
            }
        }
        else if (currentItemPackage.item.itemType == ItemType.block)
        {
            if (Vector2.Distance(transform.position, position) < reach)
            {
                if (!wm) { wm = FindObjectOfType<WorldModifier>(); }

                var distance = Vector2.Distance(position, transform.position);

                if (distance > .8f && distance < reach && wm.CanPlace(position.x, position.y, currentItemPackage.item))
                {
                    if (currentItem.tileData.multiBlockStructure && !wm.CanPlaceStructure(position.x, position.y, currentItem.tileData.multiBlockStructure))
                    {
                        return ItemType.none;
                    }

                    if (currentItemPackage.count > 0)
                    {
                        wm.PlaceBlock(position.x, position.y, currentItemPackage.item);

                        type = ItemType.block;
                        used = true;
                    }
                }
            }
        }
        else if (currentItemPackage.item.itemType == ItemType.rangedWeapon)
        {
            type = ItemType.rangedWeapon;
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

        return type;
    }

    bool canUse = true;
    bool timerRunning;

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

    public virtual void PickupItem(Pickup pickup)
    {
        ItemPackage oldI = currentItemPackage;

        if (currentItemPackage.item)
        {
            pickup.SetItem(oldI);
        }
        else
        {
            // Send this to a pool instead
            Destroy(pickup.gameObject);
        }

        ChangeItem(pickup.itemPackage);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Pickup"))
        {
            if (e.worldCollider.IsTouching(collision))
            {
                PickupItem(collision.gameObject.GetComponent<Pickup>());
            }
        }
    }
}
