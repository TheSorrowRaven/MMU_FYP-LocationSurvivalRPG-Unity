using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationDensityMapper : MonoBehaviour
{
    private static PopulationDensityMapper instance;
    public static PopulationDensityMapper Instance => instance;
    private static G G => G.Instance;

    public Texture2D MapTexture;
    public float MinPopulationDensity;
    public float MaxPopulationDensity;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Debug.Log(GetDensity(G.Location));
    }

    public float GetFloat(Vector2d geo)
    {
        Vector2Int pixelPos = G.EquirectangularProjection(geo.x, geo.y, MapTexture.width, MapTexture.height);
        Color color = MapTexture.GetPixel(pixelPos.x, pixelPos.y);
        if (color.a == 0)
        {
            return 0;
        }
        return 1 - color.r;
    }

    public float GetDensity(Vector2d geo)
    {
        float val = GetFloat(geo);
        float density = MinPopulationDensity + (val * (MaxPopulationDensity - MinPopulationDensity));
        return density;
    }


}
