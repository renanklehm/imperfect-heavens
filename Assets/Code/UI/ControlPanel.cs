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

    private void Start()
    {
        StartCoroutine(FindShipController());
        contentUI.SetActive(false);
    }

    private void Update()
    {
        if (shipController != null)
        {
            if (shipController.freeBody.isBurning)
            {
                foreach (Button button in GetComponentsInChildren<Button>()) button.interactable = false;
            }
            else
            {
                foreach (Button button in GetComponentsInChildren<Button>()) button.interactable = true;
            }
        }
    }

    public void SetBurnDuration(float value)
    {
        shipController.burnDuration = value;
        durationLabel.text = value.ToString("0.00") + " s";
    }

    public void SetBurnStrength(float value)
    {
        shipController.burnStrength = value;
        thrustLabel.text = (value / 1000).ToString("F2") + " kN";
    }

    public void AddThrust(int _direction)
    {
        shipController.burnDirection = (BurnDirection)_direction;
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
