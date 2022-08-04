using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [SerializeField] List<GameObject> screens;
    int currentScreen;

    void Start()
    {
        SwitchScreen(currentScreen);
    }

    public void SwitchScreen(int screenNum)
    {
        screens[currentScreen].SetActive(false);

        currentScreen = screenNum;

        screens[currentScreen].SetActive(true);
    }
}
