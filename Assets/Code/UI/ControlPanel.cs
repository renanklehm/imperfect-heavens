using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion.Sockets;
using System;

public class ControlPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject contentUI;
    [SerializeField]
    private Slider durationSlider;
    [SerializeField]
    private TMP_Text durationLabel;
    [SerializeField]
    private Slider thrustSlider;
    [SerializeField]
    private TMP_Text thrustLabel;

    private ShipController shipController;
    private float burnDuration;
    private float burnStrength;

    private void Start()
    {
        StartCoroutine(FindShipController());
        contentUI.SetActive(false);
    }

    public void SetBurnDuration(float value)
    {
        burnDuration = value;
        durationLabel.text = value.ToString("0.00") + " s";
    }

    public void SetBurnStrength(float value)
    {
        burnStrength = value;
        thrustLabel.text = (value / 1000).ToString("F2") + " kN";
    }

    public void AddThrust(int _direction)
    {
        shipController.RPC_AddManeuver((BurnDirection)_direction, burnStrength, burnDuration, shipController.Object.InputAuthority);
    }

    IEnumerator FindShipController()
    {
        while (true)
        {
            if (shipController == null)
            {
                foreach (ShipController x in FindObjectsOfType<ShipController>())
                {
                    if (x.HasInputAuthority)
                    {
                        shipController = x;
                        contentUI.SetActive(true);
                        break;
                    }
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
