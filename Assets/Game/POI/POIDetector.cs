using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIDetector : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out POI poi))
        {
            return;
        }
        poi.Seen();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out POI poi))
        {
            return;
        }
        poi.Hidden();
    }

}
