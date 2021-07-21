using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerSaveData
{
    const string FILE_NAME = "player_{0}.json";

    public List<ItemPackage> inventoryItems;
    public string _name;

    protected PlayerSaveData()
    {
        inventoryItems = new List<ItemPackage>();
    }

    public void Save(List<ItemPackage> inventoryItems)
    {
        this.inventoryItems = inventoryItems;

        string json = JsonUtility.ToJson(this);
        File.WriteAllText(GetFullPath(string.Format(FILE_NAME, _name)), json);
        //GameData.Load().playerNames.Add(playerName);
    }

    public static PlayerSaveData Load(string playerName)
    {
        string fileName = string.Format(FILE_NAME, playerName);

        if (!File.Exists(GetFullPath(fileName)))
        {
            var gd = new PlayerSaveData();
            return gd;
        }

        string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName));
        var save = JsonUtility.FromJson<PlayerSaveData>(json);

        for (int i = 0; i < save.inventoryItems.Count; i++)
        {
            ItemPackage package = save.inventoryItems[i];

            if (package.count > 0)
            {
                if (package.item == null)
                {
                    Debug.LogError("WHAT HAPPENED TO THIS ITEM");
                    Debug.LogError("IT IS GONE");
                }
            }
        }

        return save;
    }

    public static void Delete(string playerName)
    {
        string fileName = string.Format(FILE_NAME, playerName);

        if (File.Exists(GetFullPath(fileName)))
        {
            File.Delete(GetFullPath(fileName));
        }
    }

    public static string GetFullPath(string filename) => Path.Combine(Application.persistentDataPath, filename);
}
