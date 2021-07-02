using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public ItemPackage itemPackage;

    public void SetItem(ItemPackage newItemPackage)
    {
        GetComponent<SpriteRenderer>().sprite = newItemPackage.item.itemSprite;
        itemPackage = newItemPackage;
    }

    // When getting pulled disable collider?
    //private void OnTriggerEnter(Collider other)
    //{
    //    GetComponent<Collider2D>().enabled = false;
    //}
}
