using Mapbox.Utils;

public class Location
{
    private static GLocationService GLocationProvider => GLocationService.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;

    private double x;
    private double y;

    private double actualX;
    private double actualY;

    public bool smoothen = true;

    public Location(double x, double y)
    {
        this.x = x;
        this.y = y;
        actualX = x;
        actualY = y;
    }
    public Location() : this(0, 0)
    {
    }

    public double X
    {
        get
        {
            return x;
        }
        set
        {
            actualX = value;
        }
    }
    public double Y
    {
        get
        {
            return y;
        }
        set
        {
            actualY = value;
        }
    }
    public double ActualX => actualX;
    public double ActualY => actualY;

    public void Update()
    {
        if (GLocationProvider.IsInitialized)
        {
            if (GLocationProvider.TryGetLocation(false, out double currentX, out double currentY))
            {
                actualX = currentX;
                actualY = currentY;
            }
        }

        if (smoothen)
        {
            x = Mathd.Lerp(x, actualX, GameSettings.LocationSmoothingTime);
            y = Mathd.Lerp(y, actualY, GameSettings.LocationSmoothingTime);
        }
        else
        {
            Snap();
        }
        //TODO no location access
    }

    public void Snap()
    {
        x = actualX;
        y = actualY;
    }

    public static implicit operator Vector2d(Location location)
    {
        return new(location.x, location.y);
    }
    public static implicit operator Location(Vector2d vector2d)
    {
        return new(vector2d.x, vector2d.y);
    }

    public override string ToString()
    {
        return $"{x}, {y}";
    }

}
