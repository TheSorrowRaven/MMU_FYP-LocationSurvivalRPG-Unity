using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GButton : MonoBehaviour
{

    [SerializeField] private Button button;

    public event Action InputAction;

    private void Awake()
    {
        button.onClick.AddListener(ReportInput);
    }

    private void ReportInput()
    {
        InputAction.Invoke();
    }

}
