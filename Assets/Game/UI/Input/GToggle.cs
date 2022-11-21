using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GToggle : MonoBehaviour
{

    [field: SerializeField] public Toggle ThisToggle { get; private set; }

    [SerializeField] private RectTransform toggleRT;
    [SerializeField] private Image toggleImage;

    [SerializeField] private RectTransform handleRT;

    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;

    [SerializeField] private UnityEvent<bool> ToggleAction;

    private void Start()
    {
        Toggled(ThisToggle.isOn);
    }

    //Unity Event Referenced
    public void Toggled(bool isOn)
    {
        if (isOn)
        {
            toggleImage.color = onColor;
            handleRT.localPosition = new(toggleRT.rect.size.x / 2, -handleRT.rect.size.y);
        }
        else
        {
            toggleImage.color = offColor;
            handleRT.localPosition = new(0, -handleRT.rect.size.y);
        }
        ToggleAction.Invoke(isOn);
    }

}
