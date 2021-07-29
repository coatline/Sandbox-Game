using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandleItem : HandleItem
{
    [HideInInspector]
    public InventorySlot currentSlot;
    InventoryManager im;

    void Start()
    {
        im = FindObjectOfType<InventoryManager>();
    }

    public override ItemType UseItem(Vector2Int position)
    {
        if (currentSlot.CurrentCount <= 0) { return ItemType.none; }

        ItemType used = base.UseItem(position);

        if (used == ItemType.block || used == ItemType.consumable) { currentSlot.TryModifyItem(currentSlot.CurrentItemPackage, -1); }

        return used;
    }

    public override void PickupItem(Pickup pickup)
    {
        im.AddItem(pickup.itemPackage);
        Destroy(pickup.gameObject);
    }
}
