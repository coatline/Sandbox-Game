using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TimeOfDay
{
    morning,
    evening,
    night
}

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] CalculateColorLighting ccl;
    [SerializeField] Sprite morningSky;
    [SerializeField] Sprite nightSky;
    [SerializeField] SpriteRenderer sky;
    [SerializeField] Color nightColor;
    [SerializeField] Color morningColor;
    public TimeOfDay timeOfDay;

    void Start()
    {
        StartCoroutine(time());
    }

    IEnumerator time()
    {
        ccl.ambientColor = morningColor;
        timeOfDay = TimeOfDay.morning;
        sky.sprite = morningSky;
        yield return new WaitForSeconds(10);
        ccl.ambientColor = nightColor;
        timeOfDay = TimeOfDay.night;
        sky.sprite = nightSky;
        yield return new WaitForSeconds(10);
        StartCoroutine(time());
    }
}
