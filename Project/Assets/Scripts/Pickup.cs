using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pickup : MonoBehaviour
{
    //[SerializeField] LayerMask pickupLayer;
    //[SerializeField] float pickupDelay;
    [SerializeField] Collider2D wCol;
    public ItemPackage itemPackage;

    public void SetItem(ItemPackage newItemPackage)
    {
        GetComponent<SpriteRenderer>().sprite = newItemPackage.item.itemData.itemSprite;
        itemPackage = newItemPackage;
    }

    //private void Start()
    //{
    //    StartCoroutine(EnablePickupCollider());
    //}

    //IEnumerator EnablePickupCollider()
    //{
    //    yield return new WaitForSeconds(pickupDelay);
    //    gameObject.layer = 9 << (pickupLayer);
    //}

    // When getting pulled disable collider?
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            wCol.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            wCol.enabled = true;
        }
    }
}
