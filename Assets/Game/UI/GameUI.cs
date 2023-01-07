using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private static GameUI instance;
    public static GameUI Instance => instance;


    public TextMeshProUGUI LastUpdate;
    public TextMeshProUGUI Coords;

    public GScreen ScreenInput;
    public GJoystick MovementJoystick;
    public GToggle GPSToggle;

    public GameObject EscapeObject;
    public Button EscapeButton;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

}
