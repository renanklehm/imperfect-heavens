using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool isHovering;
    public bool isRotatingCamera;
    public bool isPlanningManeuver;
    public bool isInteractingUI;

    public static GameManager Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void OnGUI()
    {
        int fps = Mathf.RoundToInt(1.0f / Time.deltaTime);
        string text = $"FPS: {fps}";

        GUI.Label(new Rect(10, 10, 100, 20), text);
    }
}
