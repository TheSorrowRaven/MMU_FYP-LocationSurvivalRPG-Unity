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
    public GButton AttackButton;
    public GToggle GPSToggle;


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
