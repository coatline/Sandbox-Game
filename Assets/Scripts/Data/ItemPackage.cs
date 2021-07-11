using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemPackage
{
    public ItemDataContainer item;
    public int count;

    public ItemPackage(ItemDataContainer item, int count)
    {
        this.item = item;
        this.count = count;
    }
}
