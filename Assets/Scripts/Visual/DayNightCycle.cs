using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TimeOfDay
{
    none,
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
    [SerializeField] EnemySpawner es;

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

        timeOfDay = TimeOfDay.none;
    }

    void LoadLight()
    {
        backgroundOverlay.color = GD.wd.td.currentOverlayColor;
        ccl.ambientColor = GD.wd.td.currentLightingColor;
        //skyBackground.color = GD.wD.currentOverlayColor;
    }

    void InitializeLight()
    {
        // This is to set the target colors
        timeOfDay = TimeOfDay.night;

        backgroundOverlay.color = skyDayColor;
        ccl.ambientColor = dayLightColor;
        GD.wd.td.hours = 8;
    }

    //change sorting order of backgrounds and alpha value of them to fade them in and out

    float seconds;

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
            timeText.text = $"Day {GD.wd.td.days}\n{GD.wd.td.hours}:{GD.wd.td.minutes.ToString("00")}";
            seconds = 0;
            GD.wd.td.minutes++;
        }
        if (GD.wd.td.minutes >= 60)
        {
            GD.wd.td.minutes = 0;
            GD.wd.td.hours++;
        }
        if (GD.wd.td.hours >= 24)
        {
            GD.wd.td.hours = 1;
            GD.wd.td.days++;
        }
        if (GD.wd.td.days >= 365)
        {
            GD.wd.td.years++;
        }

        if (GD.wd.td.hours >= 8 && GD.wd.td.hours < 19)
        {
            if (timeOfDay == TimeOfDay.day) { return; }

            timeOfDay = TimeOfDay.day;
            es.StopAllCoroutines();

            targetLightColor = dayLightColor;
            targetSkyColor = skyDayColor;
        }
        else if (GD.wd.td.hours >= 19 && GD.wd.td.hours < 20)
        {
            if (timeOfDay == TimeOfDay.dusk) { return; }

            timeOfDay = TimeOfDay.dusk;

            targetLightColor = duskLightColor;
            targetSkyColor = skyDuskColor;
        }
        else if ((GD.wd.td.hours >= 20 || GD.wd.td.hours < 7))
        {
            if (timeOfDay == TimeOfDay.night) { return; }

            timeOfDay = TimeOfDay.night;
            es.Night();

            targetLightColor = nightLightColor;
            targetSkyColor = skyNightColor;
        }
        else if (GD.wd.td.hours >= 7 && GD.wd.td.hours < 8)
        {
            if (timeOfDay == TimeOfDay.dawn) { return; }

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
