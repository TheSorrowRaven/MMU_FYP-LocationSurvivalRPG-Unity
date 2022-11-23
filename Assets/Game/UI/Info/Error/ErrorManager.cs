using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI errorText;

    private void Awake()
    {
        Application.logMessageReceived += LogMessageReceived;
    }

    private void LogMessageReceived(string condition, string stackTrace, LogType type)
    {
        Error(condition);
    }

    public void Error(string err)
    {
        errorText.SetText(err);
    }

}
