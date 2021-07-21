using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WorldSaveData
{
    const string SAVE_GAME_NAME = "world_{0}.json";
    public string worldName = "New World";

    public WorldData data;
    public SerializableDictionary<Vector2Int, ChestData> chests;
    public SerializableDictionary<Vector2Int, ItemDataContainer> multiTiles;
    public short[] blockData;
    //public List<ChestData> chests;
    //public short worldHeight;
    //public short worldWidth;
    //public float[] timeData;
    //public short[] highestTiles;
    //public short[] blockData;

    protected WorldSaveData()
    {
        //blockData = new short[worldWidth * worldHeight];
        //highestTiles = new short[worldWidth];
        //timeData = new float[0];
        //multiTiles = new SerializableMultitileDict(new Dictionary<Vector2Int, ItemDataContainer>());
    }

    public void Save(WorldData data)
    {
        this.data = data;

        blockData = new short[data.worldWidth * data.worldHeight * 3];
        blockData = MultiToSingle(data.blockMap);

        multiTiles = new SerializableDictionary<Vector2Int, ItemDataContainer>(data.multiTileItems);
        chests = new SerializableDictionary<Vector2Int, ChestData>(data.chests);

        string json = JsonUtility.ToJson(this);
        string fileName = string.Format(SAVE_GAME_NAME, worldName);
        File.WriteAllText(GetFullPath(fileName), json);
    }

    short[] MultiToSingle(short[,,] array)
    {
        int index = 0;
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        int z = array.GetLength(2);
        short[] single = new short[width * height * z];

        for (int i = 0; i < z; i++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    single[index] = array[x, y, i];
                    index++;
                }
            }
        }

        return single;
    }
    short[,,] SingleToMulti(short[] array)
    {
        short[,,] multi = new short[data.worldWidth, data.worldHeight, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int x = 0; x < data.worldWidth; x++)
            {
                for (int y = 0; y < data.worldHeight; y++)
                {
                    multi[x, y, i] = array[x + (y * data.worldWidth) + (i * (data.worldWidth * data.worldHeight))];
                }
            }
        }

        return multi;
    }

    //public void Save(short worldWidth, short worldHeight, float[] time, short[] blockData, short[] highestTiles, SerializableMultitileDict multiTiles, List<ChestData> chests)
    //{
    //    this.highestTiles = highestTiles;
    //    this.worldWidth = worldWidth;
    //    this.worldHeight = worldHeight;
    //    this.multiTiles = multiTiles;
    //    this.blockData = blockData;
    //    this.timeData = time;
    //    this.chests = chests;

    //    string json = JsonUtility.ToJson(this);
    //    string fileName = string.Format(SAVE_GAME_NAME, worldName);
    //    File.WriteAllText(GetFullPath(fileName), json);
    //}

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

        sg.data.blockMap = sg.SingleToMulti(sg.blockData);
        sg.data.multiTileItems = sg.multiTiles.ToDictionary();
        sg.data.chests = sg.chests.ToDictionary();

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

[System.Serializable]
public struct SerializableKeyValuePair<T, U>
{
    public T key; public U value;
    public SerializableKeyValuePair(T key, U data) { this.key = key; this.value = data; }
}


[System.Serializable]
public struct SerializableDictionary<T, U>
{
    public SerializableKeyValuePair<T, U>[] serializableDict;
    //KeyValuePair<T, U> fdsf;

    public SerializableDictionary(Dictionary<T, U> dictionary)
    {
        serializableDict = new SerializableKeyValuePair<T, U>[dictionary.Count];

        int index = 0;

        foreach (KeyValuePair<T, U> d in dictionary)
        {
            this.serializableDict[index] = new SerializableKeyValuePair<T, U>(d.Key, d.Value);
            index++;
        }
    }

    public Dictionary<T, U> ToDictionary()
    {
        var d = new Dictionary<T, U>();

        for (int i = 0; i < serializableDict.Length; i++)
        {
            d.Add(serializableDict[i].key, serializableDict[i].value);
        }

        return d;
    }
}