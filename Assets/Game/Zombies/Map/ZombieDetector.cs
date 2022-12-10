using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieDetector : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out MapZombie zombie))
        {
            return;
        }
        zombie.Seen();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out MapZombie zombie))
        {
            return;
        }
        zombie.Hidden();
    }
}
