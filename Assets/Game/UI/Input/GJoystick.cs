using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{

    [SerializeField] private RectTransform thisRT;
    [SerializeField] private RectTransform joystickRT;
    [SerializeField] private RectTransform baseRT;

    [NonSerialized] private bool pointerHeld = false;
    [NonSerialized] private Vector2 lastPos;
    [NonSerialized] private float size;

    public event Action<Vector2> InputAction;

    private void Awake()
    {
        size = baseRT.sizeDelta.x / 2f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerHeld = true;
        lastPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerHeld = false;
        AlignJoystickWithBase();
    }

    public void OnDrag(PointerEventData eventData)
    {
        lastPos = eventData.position;
    }

    private void Update()
    {
        if (!pointerHeld)
        {
            return;
        }
        ReportInput(lastPos);
    }

    private void ReportInput(Vector2 position)
    {
        Vector2 thisPos = thisRT.position;
        Vector2 relativeDelta = position - thisPos;
        float distance = relativeDelta.magnitude;

        if (distance > size)
        {
            //Limit to size
            relativeDelta = relativeDelta / distance * size;
            distance = size;
        }

        //RelativeDelta is the distance within the size
        AlignJoystickWithBase(relativeDelta);

        //This will limit the delta to within 1 magnitude
        Vector2 delta = relativeDelta / distance;
        if (distance < size)
        {
            delta *= distance / size;
        }
        InputAction.Invoke(delta);
    }

    private void AlignJoystickWithBase(Vector2 delta = default)
    {
        joystickRT.anchoredPosition = delta;
    }

    public void ExternalJoystickControl(Vector2 delta = default)
    {
        joystickRT.anchoredPosition = delta * size;
    }

}
