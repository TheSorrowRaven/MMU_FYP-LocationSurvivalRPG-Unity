using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatExit : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        Player.Instance.ExitReachedInCombat();
    }

}
