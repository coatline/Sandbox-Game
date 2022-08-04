using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveUIManager : MonoBehaviour
{
    [SerializeField] GameObject playerProfilePrefab;
    [SerializeField] SaveManager sm;
    [SerializeField] WorldGenerator wg;
    [SerializeField] GameObject worldProfilePrefab;
    [SerializeField] RectTransform playerProfileHolder;
    [SerializeField] RectTransform worldProfileHolder;
    [SerializeField] ScreenManager screenManager;
    [SerializeField] TMP_InputField worldWidthText;
    [SerializeField] TMP_InputField worldHeightText;
    [SerializeField] TMP_InputField worldNameText;
    [SerializeField] Button createWorldButton;
    [SerializeField] Vector2Int defaultWorldSize;
    UserData userData;

    void Start()
    {
        GD.currentWorld = null;
        GD.currentPlayer = null;

        userData = UserData.Load();
        userData.Save();
        
        for (int i = 0; i < userData.playerNames.Count; i++)
        {
            PlayerSaveData.Load(userData.playerNames[i]);
            AddPlayerSlot(userData.playerNames[i]);
        }

        for (int j = 0; j < userData.worldNames.Count; j++)
        {
            WorldSaveData.Load(userData.worldNames[j]);
            AddWorldSlot(userData.worldNames[j]);
        }

        worldHeightText.text = defaultWorldSize.y.ToString();
        worldWidthText.text = defaultWorldSize.x.ToString();

        createWorldButton.onClick.AddListener(() => CreateWorld(worldNameText.text, short.Parse(worldWidthText.text), short.Parse(worldHeightText.text)));
    }

    public void CheckForInvalidCharacters(TMP_InputField textg)
    {
        if (textg.text == "")
        {
            textg.text = "casper";
        }
    }

    public void CheckForIsNumber(TMP_InputField text)
    {
        bool parseAble = int.TryParse(text.text, out int num);

        if (parseAble)
        {
        }
        else
        {
            if (text == worldHeightText)
            {
                text.text = defaultWorldSize.y.ToString();
            }
            else
            {

                text.text = defaultWorldSize.x.ToString();
            }
        }
    }

    void AddWorldSlot(string _name)
    {
        var worldProfile = Instantiate(worldProfilePrefab, worldProfileHolder);
        worldProfileHolder.sizeDelta += new Vector2(0, worldProfileHolder.GetComponent<GridLayoutGroup>().cellSize.y);
        var worldNameText = worldProfile.transform.GetChild(0).GetComponent<TMP_Text>();

        worldNameText.text = _name;

        worldProfile.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => DeleteWorld(_name, worldProfile));
        worldProfile.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => StartWithWorld(_name));
    }

    void AddPlayerSlot(string _name)
    {
        var playerProfile = Instantiate(playerProfilePrefab, playerProfileHolder);
        playerProfileHolder.sizeDelta += new Vector2(0, worldProfileHolder.GetComponent<GridLayoutGroup>().cellSize.y);
        var playerNameText = playerProfile.transform.GetChild(0).GetComponent<TMP_Text>();

        playerNameText.text = _name;

        playerProfile.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => DeletePlayer(_name, playerProfile));
        playerProfile.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => SelectPlayer(_name));
    }

    public void DeletePlayer(string _name, GameObject saveSlot)
    {
        PlayerSaveData.Delete(_name);
        userData.playerNames.Remove(_name);
        userData.Save();
        Destroy(saveSlot);
        playerProfileHolder.sizeDelta -= new Vector2(0, worldProfileHolder.GetComponent<GridLayoutGroup>().cellSize.y);
    }

    public void DeleteWorld(string _name, GameObject saveSlot)
    {
        WorldSaveData.Delete(_name);
        userData.worldNames.Remove(_name);
        userData.Save();
        Destroy(saveSlot);
        worldProfileHolder.sizeDelta -= new Vector2(0, worldProfileHolder.GetComponent<GridLayoutGroup>().cellSize.y);
    }

    public void SelectPlayer(string _name)
    {
        GD.currentPlayer = PlayerSaveData.Load(_name);
        screenManager.SwitchScreen(3);
    }

    public void StartWithWorld(string _name)
    {
        GD.currentWorld = WorldSaveData.Load(_name);
        SceneManager.LoadScene(1);
    }

    public void CreateWorld(string _name, short width, short height)
    {
        int increment = 0;
        string originalName = _name;

        while (userData.worldNames.Contains(_name))
        {
            increment++;
            _name = originalName + $" {increment}";
        }

        AddWorldSlot(_name);

        var newWorldSave = WorldSaveData.Load(_name);

        Random.InitState(Random.Range(0, 99999999));

        newWorldSave.worldName = _name;

        wg.GenerateNewBlockData(width, height);

        newWorldSave.data = GD.wd;

        print($"Creating world with a width of {width} and a height of {height}.");
        sm.TrySaveWorld(newWorldSave);

        userData.worldNames.Add(_name);
        userData.Save();

        screenManager.SwitchScreen(3);
    }

    public void CreatePlayer(TMP_Text text)
    {
        string _name = text.text;

        int increment = 0;
        string originalName = _name;

        while (userData.playerNames.Contains(_name))
        {
            increment++;
            _name = originalName + $" {increment}";
        }

        AddPlayerSlot(_name);

        var newPlayerSave = PlayerSaveData.Load(_name);

        newPlayerSave._name = _name;
        newPlayerSave.Save(new List<string>());

        userData.playerNames.Add(_name);
        userData.Save();

        screenManager.SwitchScreen(1);
        //PlayerSaveData.Load(_name);
    }
}
