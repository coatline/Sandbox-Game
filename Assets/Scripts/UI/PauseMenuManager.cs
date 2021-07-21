using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    //[SerializeField] InventoryManager im;
    //[SerializeField] WorldGenerator wg;
    [SerializeField] SaveManager sm;
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
        sm.TrySaveAll();

        GD.currentPlayer = null;
        GD.currentWorld = null;

        SceneManager.LoadScene(0);
    }

    public void Resume()
    {
        TogglePauseMenu();
    }
}
