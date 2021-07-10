using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemPackage
{
    public ItemDataConatainer item;
    public int count;

    public ItemPackage(ItemDataConatainer item, int count)
    {
        this.item = item;
        this.count = count;
    }
}
