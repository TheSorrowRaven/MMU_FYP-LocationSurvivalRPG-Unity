using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GScreen : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{

    [System.NonSerialized] private bool pointerHeld = false;

    [System.NonSerialized] private Vector2 currentPos;
    [System.NonSerialized] private Vector2 lastPos;

    [SerializeField] private bool suppressZero;
    [SerializeField] private UnityEvent<Vector2> InputAction;

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerHeld = true;
        currentPos = eventData.position;
        lastPos = currentPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerHeld = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        currentPos = eventData.position;
    }

    private void Update()
    {
        if (!pointerHeld)
        {
            return;
        }
        ReportInput();
    }

    private void ReportInput()
    {
        Vector2 delta = currentPos - lastPos;
        lastPos = currentPos;

        if (suppressZero && delta.x == 0 && delta.y == 0)
        {
            return;
        }
        InputAction.Invoke(delta);
    }


}
