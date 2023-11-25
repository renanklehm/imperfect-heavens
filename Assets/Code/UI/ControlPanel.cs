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
        Vector3 direction;
        switch (_direction)
        {
            case 1:
                direction = shipController.transform.forward;
                break;
            case -1:
                direction = -shipController.transform.forward;
                break;
            case 2:
                direction = shipController.transform.up;
                break;
            case -2:
                direction = -shipController.transform.up;
                break;
            case 3:
                direction = shipController.transform.right;
                break;
            case -3:
                direction = -shipController.transform.right;
                break;
            default:
                direction = Vector3.zero;
                break;
        }

        shipController.burnDirection = direction;
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
