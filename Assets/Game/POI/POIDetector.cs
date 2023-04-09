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
        poi.InRadiusCanLoot();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out POI poi))
        {
            return;
        }
        poi.OutsideRadiusCannotLoot();
    }

}
