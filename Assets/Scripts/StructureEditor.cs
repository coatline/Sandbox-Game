using System.Collections;
using System.Collections.Generic;
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
    List<RuleTile> tiles;
    Camera cam;

    void LoadStructureData()
    {
        for (int x = 0; x < structureToEdit.width; x++)
            for (int y = 0; y < structureToEdit.height; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), structureToEdit.tiles[structureToEdit.structure[x + (y * structureToEdit.width)]]);
            }
    }

    private void ClearMap()
    {
        tilemap.ClearAllTiles();
    }

    void Start()
    {
        cam = Camera.main;
    }

    void SaveStructureData()
    {
        structureToEdit.structure = new int[structureToEdit.width * structureToEdit.height];
        tiles = new List<RuleTile>();
        structureToEdit.tiles.Clear();

        for (int x = 0; x < structureToEdit.width; x++)
            for (int y = 0; y < structureToEdit.height; y++)
            {
                var tile = (RuleTile)tilemap.GetTile(new Vector3Int(x, y, 0));

                if (!tiles.Contains(tile))
                {
                    tiles.Add(tile);
                }
            }

        for (int x = 0; x < structureToEdit.width; x++)
            for (int y = 0; y < structureToEdit.height; y++)
            {
                structureToEdit.structure[x + (y * structureToEdit.width)] = TilesIndex((RuleTile)tilemap.GetTile(new Vector3Int(x, y, 0)));
            }

        structureToEdit.tiles = this.tiles;
    }

    int TilesIndex(RuleTile tile)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tile == tiles[i])
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
