using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNames : MonoBehaviour
{
    [SerializeField] List<ItemDataContainer> items;
    [SerializeField] List<Structure> structures;

    [InspectorButton("SetNamesToGOBName")]
    [SerializeField] bool Go;
    
    void SetNamesToGOBName()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i]._name = items[i].name;
        }

        for (int i = 0; i < structures.Count; i++)
        {
            structures[i]._name = structures[i].name;
        }
    }
}
