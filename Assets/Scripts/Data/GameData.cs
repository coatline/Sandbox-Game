using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameData
{
    const string FILE_NAME = "GameData.json";

    public List<string> worldNames;
    public List<string> playerNames;

    protected GameData()
    {
        worldNames = new List<string>();
        playerNames = new List<string>();
    }

    public void Save(/*List<string> worlds, List<string> players*/)
    {
        //playerNames = players;
        //worldNames = worlds;

        string json = JsonUtility.ToJson(this);
        File.WriteAllText(GetFullPath(FILE_NAME), json);
    }

    public static GameData Load()
    {
        if (!File.Exists(GetFullPath(FILE_NAME)))
        {
            Debug.Log("Welcome New Player!");
            var gd = new GameData();
            return gd;
        }

        string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, FILE_NAME));
        var sg = JsonUtility.FromJson<GameData>(json);
        return sg;
    }

    public static string GetFullPath(string filename) => Path.Combine(Application.persistentDataPath, filename);
}
