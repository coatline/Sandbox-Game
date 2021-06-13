using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPS_Display : MonoBehaviour
{
    [SerializeField] TMP_Text display_Text;
    float avgFrameRate;

    void Update()
    {
        avgFrameRate = (Time.frameCount / Time.unscaledTime);
        display_Text.text = avgFrameRate.ToString() + " FPS";
    }

}
