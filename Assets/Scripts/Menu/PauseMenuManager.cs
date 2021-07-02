using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] InventoryManager im;
    [SerializeField] WorldGenerator wg;
    GameObject settingsHolder;
    GameObject settingsButton;

    void Start()
    {
        settingsHolder = transform.GetChild(0).gameObject;
        settingsButton = transform.GetChild(1).gameObject;
    }

    public void ToggleSettingsButton()
    {
        settingsButton.SetActive(!settingsButton.activeSelf);
    }

    public void TogglePauseMenu()
    {
        settingsHolder.SetActive(!settingsHolder.activeSelf);
    }

    public void SaveAndExit()
    {
        im.Save();
        wg.Save();

        SaveData.currentPlayer = null;
        SaveData.currentWorld = null;

        SceneManager.LoadScene(0);
    }

    public void Resume()
    {
        TogglePauseMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsButton();
        }
    }
}
