using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WorldSaveData
{
    const string SAVE_GAME_NAME = "world_{0}.json";
    public string worldName = "New World";
    public short worldHeight;
    public short worldWidth;
    public float[] timeData;
    public short[] highestTiles;
    public short[] blockData;

    protected WorldSaveData()
    {
        blockData = new short[worldWidth * worldHeight];
        highestTiles = new short[worldWidth];
        timeData = new float[12];
    }

    public void Save(short worldWidth, short worldHeight, float[] time, short[] blockData, short[] highestTiles)
    {
        this.highestTiles = highestTiles;
        this.worldWidth = worldWidth;
        this.worldHeight = worldHeight;
        this.blockData = blockData;
        this.timeData = time;

        string json = JsonUtility.ToJson(this);
        string fileName = string.Format(SAVE_GAME_NAME, worldName);
        File.WriteAllText(GetFullPath(fileName), json);
    }

    public static WorldSaveData Load(string worldName)
    {
        string fileName = string.Format(SAVE_GAME_NAME, worldName);

        if (!File.Exists(GetFullPath(fileName)))
        {
            var gd = new WorldSaveData();
            return gd;
        }

        string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName));
        var sg = JsonUtility.FromJson<WorldSaveData>(json);
        return sg;
    }

    public void Delete() => Delete(worldName);

    public static void Delete(string game)
    {
        string fileName = string.Format(SAVE_GAME_NAME, game);

        if (File.Exists(GetFullPath(fileName)))
        {
            File.Delete(GetFullPath(fileName));
        }
    }

    public static string GetFullPath(string filename) => Path.Combine(Application.persistentDataPath, filename);
}

//[System.Serializable]
//public class SerializableTree
//{
//    public Vector2Int key; public TreeData data;
//    public SerializableTree(Vector2Int key, TreeData data) { this.key = key; this.data = data; }
//}


//[System.Serializable]
//public class SerializableTreeDict
//{
//    public SerializableTree[] treesDictionary;

//    public SerializableTreeDict(Dictionary<Vector2Int, TreeData> dictionary)
//    {
//        treesDictionary = new SerializableTree[dictionary.Count];

//        int index = 0;

//        foreach (KeyValuePair<Vector2Int, TreeData> d in dictionary)
//        {
//            treesDictionary[index] = new SerializableTree(d.Key, d.Value);
//            index++;
//        }
//    }
//}