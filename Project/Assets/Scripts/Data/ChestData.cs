using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ChestData
{
    public ItemPackage[] items;
    public Vector2Int pos;

    public ChestData(ItemPackage[] items, Vector2Int pos)
    {
        this.items = items;
        this.pos = pos;
    }

    public void AddItem(ItemPackage item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].item)
            {
                items[i] = item;
                break;
            }
        }
    }
}
