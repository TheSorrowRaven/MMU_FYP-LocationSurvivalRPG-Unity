using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private static G G => G.Instance;

    //Unity Event Referenced
    public void ReceiveJoystickMovement(Vector2 movementDelta)
    {
        G.Location.PlayerReportJoystickMovment(movementDelta);
    }


}
