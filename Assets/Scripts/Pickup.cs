using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public ItemDataContainer itemData;
    public int count;

    public void SetItem(ItemDataContainer item, int count)
    {
        GetComponent<SpriteRenderer>().sprite = item.itemSprite;
        itemData = item;
        this.count = count;
    }
}
