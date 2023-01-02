using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GScreen : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
{

    [System.NonSerialized] private bool pointerHeld = false;

    [System.NonSerialized] private Vector2 firstDownPos;
    [System.NonSerialized] private Vector2 currentPos;
    [System.NonSerialized] private Vector2 lastPos;


    [SerializeField] private float clickThreshold;

    [SerializeField] private bool suppressZero;
    [SerializeField] private bool reportScreenPosition;
    [SerializeField] private UnityEvent<Vector2> InputAction;
    [SerializeField] private UnityEvent<Vector2> ClickAction;

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerHeld = true;
        currentPos = eventData.position;
        firstDownPos = currentPos;
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

    public void AddAsInputAction(UnityAction<Vector2> inputAction)
    {
        InputAction.AddListener(inputAction);
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
        if (reportScreenPosition)
        {
            InputAction.Invoke(currentPos);
            return;
        }
        Vector2 delta = currentPos - lastPos;
        lastPos = currentPos;

        if (suppressZero && delta.x == 0 && delta.y == 0)
        {
            return;
        }
        InputAction.Invoke(delta);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 currentPos = eventData.position;
        float dist = Vector2.Distance(currentPos, firstDownPos);
        if (dist < clickThreshold)
        {
            ClickAction.Invoke(currentPos);
        }
    }
}
