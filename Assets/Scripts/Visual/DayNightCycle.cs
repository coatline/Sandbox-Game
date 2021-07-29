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
        if (GD.currentWorld != null && GD.wd.elapsedTime > 0)
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
        years = GD.wd.td.years;
        days = GD.wd.td.days;
        hours = GD.wd.td.hours;
        minutes = GD.wd.td.minutes;
        backgroundOverlay.color = GD.wd.td.currentOverlayColor;
        ccl.ambientColor = GD.wd.td.currentLightingColor;
        //skyBackground.color = GD.wD.currentOverlayColor;
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

        if (hours >= 8 && hours < 19)
        {
            timeOfDay = TimeOfDay.day;

            targetLightColor = dayLightColor;
            targetSkyColor = skyDayColor;
        }
        else if (hours >= 19 && hours < 20)
        {
            timeOfDay = TimeOfDay.dusk;

            targetLightColor = duskLightColor;
            targetSkyColor = skyDuskColor;
        }
        else if ((hours >= 20 || hours < 7))
        {
            timeOfDay = TimeOfDay.night;

            targetLightColor = nightLightColor;
            targetSkyColor = skyNightColor;
        }
        else if (hours >= 7 && hours < 8)
        {
            timeOfDay = TimeOfDay.dawn;

            targetLightColor = dawnLightColor;
            targetSkyColor = skyDawnColor;
        }
    }


    void DisplaySky()
    {
        GD.wd.td.currentLightingColor = Color.Lerp(ccl.ambientColor, targetLightColor, (Time.fixedDeltaTime / 5000) * timeScale);
        GD.wd.td.currentOverlayColor = Color.Lerp(backgroundOverlay.color, targetSkyColor/* + new Color(0, 0, 0, 1)*/, (Time.fixedDeltaTime / 5000) * timeScale);
        backgroundOverlay.color = GD.wd.td.currentOverlayColor;
        ccl.ambientColor = GD.wd.td.currentLightingColor;
        //skyBackground.color = backgroundOverlay.color;
        //backgroundOverlay.color = new Color(0, 0, 0, 1 - skyBackground.r);
    }
}

[System.Serializable]
public class TimeData
{
    public Color currentOverlayColor;
    public Color currentBackgroundColor;
    public Color currentLightingColor;

    public int years;
    public int days;
    public int hours;
    public int minutes;

    public TimeData(int days, int hours)
    {
        this.days = days;
        this.hours = hours;
    }
}
