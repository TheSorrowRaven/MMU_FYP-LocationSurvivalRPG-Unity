using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class CombatCameraController : MonoBehaviour
{
    private static G G => G.Instance;
    private static CombatPlayer CombatPlayer => CombatPlayer.Instance;


    [System.NonSerialized] private bool hasInput;
    [System.NonSerialized] private int lastFrameInput;
    [System.NonSerialized] private Vector2 lastScreenPosition;
    [System.NonSerialized] private Vector2 delta;

    private void Start()
    {
        //G.ScreenInput.AddAsInputAction(ScreenDragInput);
    }

    private void ScreenDragInput(Vector2 screenPosition)
    {
        int currentFrame = Time.frameCount;
        if (currentFrame - 1 != lastFrameInput)
        {
            lastScreenPosition = screenPosition;
        }
        delta = lastScreenPosition - screenPosition;
        lastScreenPosition = screenPosition;
        lastFrameInput = currentFrame;
        hasInput = true;
    }

    private void Update()
    {
        if (!hasInput)
        {
            return;
        }
        hasInput = false;
        CombatPlayer.RotateView(delta);
    }

}
