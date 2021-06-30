using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    [SerializeField] RenderLighting rl;

    public void AddPickup(Pickup pickup)
    {
        if (pickup.itemPackage.item.emitsLight)
        {
            rl.dynamicLightEmitters.Add(pickup.transform);
        }

        pickup.transform.SetParent(transform);
    }
}
