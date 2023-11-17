using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlPanel : MonoBehaviour
{
    public Slider thrustSlider;
    public TMP_Text thrustLabel;

    public void ThrustSliderChange(float value)
    {
        thrustLabel.text = (value / 1000).ToString("F2") + " kN";
    }
}
