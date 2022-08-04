using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UserData
{
    const string FILE_NAME = "!UserData.json";

    public List<string> worldNames;
    public List<string> playerNames;
    public string version;

    protected UserData()
    {
        worldNames = new List<string>();
        playerNames = new List<string>();
    }

    public void Save()
    {
        if (version != Application.version)
        {
            Debug.Log($"New version detected! old: {version} new: {Application.version}");
            version = Application.version;
        }

        string json = JsonUtility.ToJson(this);
        File.WriteAllText(GetFullPath(FILE_NAME), json);
    }

    public static UserData Load()
    {
        if (!File.Exists(GetFullPath(FILE_NAME)))
        {
            Debug.Log("Welcome New Player!");
            var gd = new UserData();
            return gd;
        }

        string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, FILE_NAME));
        var sg = JsonUtility.FromJson<UserData>(json);
        return sg;
    }

    public static string GetFullPath(string filename) => Path.Combine(Application.persistentDataPath, filename);
}
