using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class References : MonoBehaviour
{
    private static References instance;
    public static References Instance => instance;

    public AbstractMap Mapbox;
    public Camera Camera;
    public Transform CameraTR;
    public LineRenderer PlayerRadiusLR;

    private void Awake()
    {
        instance = this;
    }


}
