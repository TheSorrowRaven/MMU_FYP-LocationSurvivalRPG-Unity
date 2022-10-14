using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G : MonoBehaviour
{
    private static G instance;
    public static G Instance => instance;

    [field: SerializeField] public AbstractMap Mapbox { get; private set; }

    public Location Location { get; private set; }

    private void Awake()
    {
        instance = this;
        Location = new(2.923140, 101.639631);
    }

    private void Start()
    {
        InitializeServices();

        Mapbox.Initialize(Location, 18);
        Mapbox.UpdateMap(18f);
    }

    private void Update()
    {
        Location.Update();
        Mapbox.UpdateMap(Location);
    }

    private void InitializeServices()
    {
        LocationProvider.Instance.Initialize();
    }

}
