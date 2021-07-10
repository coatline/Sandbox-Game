using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


public class StructureEditor : MonoBehaviour
{
    [SerializeField] Structure structureToEdit;
    [SerializeField] Tilemap tilemap;
    [InspectorButton("SaveStructureData", ButtonWidth = 150)]
    [SerializeField] bool SaveData;
    [InspectorButton("LoadStructureData", ButtonWidth = 150)]
    [SerializeField] bool LoadCurrentData;
    [InspectorButton("ClearMap", ButtonWidth = 150)]
    [SerializeField] bool ClearTilemap;
    [InspectorButton("ClearItemData", ButtonWidth = 150)]
    [SerializeField] bool ClearItemDataList;
    [SerializeField] List<ItemDataConatainer> itemData;
    List<ItemDataConatainer> tiles;

    void LoadStructureData()
    {
        for (int x = 0; x < structureToEdit.width; x++)
            for (int y = 0; y < structureToEdit.height; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), structureToEdit.tiles[structureToEdit.structureData[x + (y * structureToEdit.width)]].tileData.tile);
            }
    }

    private void ClearMap()
    {
        tilemap.ClearAllTiles();
    }

    void ClearItemData()
    {
        itemData.Clear();
    }

    void SaveStructureData()
    {
        Debug.ClearDeveloperConsole();

        structureToEdit.structureData = new int[structureToEdit.width * structureToEdit.height];
        tiles = new List<ItemDataConatainer>();
        structureToEdit.tiles.Clear();

        for (int x = 0; x < structureToEdit.width; x++)
            for (int y = 0; y < structureToEdit.height; y++)
            {
                var tile = (RuleTile)tilemap.GetTile(new Vector3Int(x, y, 0));

                for (int i = 0; i < itemData.Count; i++)
                {
                    if (itemData[i].tileData.tile == tile && !tiles.Contains(itemData[i]))
                    {
                        tiles.Add(itemData[i]);
                    }
                }
            }

        for (int x = 0; x < structureToEdit.width; x++)
            for (int y = 0; y < structureToEdit.height; y++)
            {
                structureToEdit.structureData[x + (y * structureToEdit.width)] = TilesIndex((RuleTile)tilemap.GetTile(new Vector3Int(x, y, 0)));
            }

        structureToEdit.tiles = this.tiles;
        EditorUtility.SetDirty(structureToEdit);
    }

    int TilesIndex(RuleTile tile)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tile == tiles[i].tileData.tile)
            {
                return i;
            }
        }

        print($"Did not identify tile {tile}");
        return 0;
    }


    private void OnDrawGizmos()
    {
        for (int x = 0; x < structureToEdit.width; x++)
            for (int y = 0; y < structureToEdit.height; y++)
            {
                Gizmos.DrawWireCube(new Vector3((x + .5f), (y + .5f)), new Vector3(1, 1));
            }
    }
}
