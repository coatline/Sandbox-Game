using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public ItemPackage itemPackage;
    [SerializeField] Collider2D c;

    public void SetItem(ItemPackage newItemPackage)
    {
        GetComponent<SpriteRenderer>().sprite = newItemPackage.item.itemData.itemSprite;
        itemPackage = newItemPackage;
    }

    // When getting pulled disable collider?
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            c.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            c.enabled = true;
        }
    }
}
