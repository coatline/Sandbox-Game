using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemDataEditor : MonoBehaviour
{
    [SerializeField] List<ItemDataContainer> items;
    [SerializeField] List<Structure> structures;
    [SerializeField] List<TileBase> ruleTiles;

    [InspectorButton("ClearItemList", ButtonWidth = 175)]
    [SerializeField] bool ClearItems;
    [InspectorButton("ClearRuleTileList", ButtonWidth = 175)]
    [SerializeField] bool ClearRuleTiles;
    [InspectorButton("SetDataNamesToGOBName", ButtonWidth = 175)]
    [SerializeField] bool SetNamesToGOBName;
    [InspectorButton("SetRuleTilesToData", ButtonWidth = 175)]
    [SerializeField] bool SetItemDataRuleTiles;

    void ClearItemList()
    {
        items.Clear();
    }

    void ClearRuleTileList()
    {
        ruleTiles.Clear();
    }

    void SetDataNamesToGOBName()
    {
        for (int i = 0; i < items.Count; i++)
        {
            EditorUtility.SetDirty(items[i]);
        }

        for (int i = 0; i < structures.Count; i++)
        {
            structures[i]._name = structures[i].name;
            EditorUtility.SetDirty(structures[i]);
        }
    }

    void SetRuleTilesToData()
    {
        for (int i = 0; i < items.Count; i++)
        {
            for (int j = 0; j < ruleTiles.Count; j++)
            {
                if (items[i].name == ruleTiles[j].name)
                {
                    items[i].tileData.tile = ruleTiles[j];
                    EditorUtility.SetDirty(items[i]);
                    break;
                }
            }
        }

    }
}
