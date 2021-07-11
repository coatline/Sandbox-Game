using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TimeOfDay
{
    day,
    dusk,
    dawn,
    night
}

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] SpriteRenderer backgroundOverlay;
    [SerializeField] SpriteRenderer skyBackground;
    [SerializeField] CalculateColorLighting ccl;
    [SerializeField] float timeScale;
    [SerializeField] TMP_Text timeText;

    // Animation curves?

    [Header("Background Color")]
    [SerializeField] Color skyNightColor;
    [SerializeField] Color skyDayColor;
    [SerializeField] Color skyDuskColor;
    [SerializeField] Color skyDawnColor;
    [Header("Ambient Light Color")]
    [SerializeField] Color dayLightColor;
    [SerializeField] Color nightLightColor;
    [SerializeField] Color duskLightColor;
    [SerializeField] Color dawnLightColor;

    [Header("Debug")]
    public TimeOfDay timeOfDay;
    [SerializeField] Color targetSkyColor;
    public Color targetLightColor;

    void Start()
    {
        if (SaveData.currentWorld != null && SaveData.currentWorld.timeData.Length > 11)
        {
            // Not a new world load time
            LoadLight();
        }
        else
        {
            InitializeLight();
        }
    }

    void LoadLight()
    {
        float[] timeData = SaveData.currentWorld.timeData;
        years = (int)timeData[0];
        days = (int)timeData[1];
        hours = (int)timeData[2];
        minutes = (int)timeData[3];
        backgroundOverlay.color = new Color(timeData[4], timeData[5], timeData[6], timeData[7]);
        ccl.ambientColor = new Color(timeData[8], timeData[9], timeData[10], timeData[11]);
    }

    void InitializeLight()
    {
        // This is to set the target colors
        timeOfDay = TimeOfDay.dawn;

        backgroundOverlay.color = skyDayColor;
        ccl.ambientColor = dayLightColor;
        hours = 8;
    }

    //change sorting order of backgrounds and alpha value of them to fade them in and out

    float seconds;
    int minutes;
    int hours;
    int days;
    int years;

    private void Update()
    {
        ProgressTime();
        DisplaySky();
    }

    void ProgressTime()
    {
        seconds += Time.fixedDeltaTime * timeScale;

        if (seconds >= 60)
        {
            timeText.text = $"Day {days}\n{hours}:{minutes.ToString("00")}";
            seconds = 0;
            minutes++;
        }
        if (minutes >= 60)
        {
            minutes = 0;
            hours++;
        }
        if (hours >= 24)
        {
            hours = 1;
            days++;
        }
        if (days >= 365)
        {
            years++;
        }

        if (timeOfDay != TimeOfDay.day && hours >= 8 && hours < 19)
        {
            timeOfDay = TimeOfDay.day;

            targetLightColor = dayLightColor;
            targetSkyColor = skyDayColor;
        }
        else if (timeOfDay != TimeOfDay.dusk && hours >= 19 && hours < 20)
        {
            timeOfDay = TimeOfDay.dusk;

            targetLightColor = duskLightColor;
            targetSkyColor = skyDuskColor;
        }
        else if (timeOfDay != TimeOfDay.night && (hours >= 20 || hours < 7))
        {
            timeOfDay = TimeOfDay.night;

            targetLightColor = nightLightColor;
            targetSkyColor = skyNightColor;
        }
        else if (timeOfDay != TimeOfDay.dawn && hours >= 7 && hours < 8)
        {
            timeOfDay = TimeOfDay.dawn;

            targetLightColor = dawnLightColor;
            targetSkyColor = skyDawnColor;
        }
    }

    void DisplaySky()
    {
        ccl.ambientColor = Color.Lerp(ccl.ambientColor, targetLightColor, Time.fixedDeltaTime / 22 * timeScale);
        backgroundOverlay.color = Color.Lerp(backgroundOverlay.color, targetSkyColor/* + new Color(0, 0, 0, 1)*/, Time.fixedDeltaTime / 25 * timeScale);
        //backgroundOverlay.color = new Color(0, 0, 0, 1 - skyBackground.r);
    }

    public float[] GetTime()
    {
        return new float[12] { years, days, hours, minutes, backgroundOverlay.color.r, backgroundOverlay.color.g, backgroundOverlay.color.b, backgroundOverlay.color.a, ccl.ambientColor.r, ccl.ambientColor.g, ccl.ambientColor.b, ccl.ambientColor.a };
    }
}
