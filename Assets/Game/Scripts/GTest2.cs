using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GTest2 : MonoBehaviour
{

    public Vector2d location;

    void Update()
    {
        transform.position = G.Instance.GeoToWorld(location);
    }
}
