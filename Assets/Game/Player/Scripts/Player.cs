using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private static G G => G.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;

    public Transform ThisTR;

    [System.NonSerialized] private Vector2d lastPos;


    //Unity Event Referenced
    public void ReceiveJoystickMovement(Vector2 movementDelta)
    {
        G.Location.PlayerReportJoystickMovment(movementDelta);
    }


    private void Update()
    {
        Vector2d currentPos = new(G.Instance.Location.Y, G.Instance.Location.X);
        Vector2d direction = (currentPos - lastPos) / GameSettings.MovementSpeed;

        Vector3 lookAt = new Vector3((float)direction.x, 0, (float)direction.y) + ThisTR.localPosition;
        ThisTR.LookAt(lookAt, Vector3.up);

        lastPos = currentPos;
    }

}
