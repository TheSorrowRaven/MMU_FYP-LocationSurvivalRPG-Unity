using JetBrains.Annotations;
using Mapbox.Utils;
using UnityEngine;

public class Location
{
    private static G G => G.Instance;
    private static GLocationService GLocationProvider => GLocationService.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;

    private double x;
    private double y;

    private double actualX;
    private double actualY;

    public bool smoothen = true;

    [field: SerializeField] public bool UseJoystickMovement { get; private set; }

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

    public void EnableJoystickMovement()
    {
        if (UseJoystickMovement)
        {
            return;
        }
        UseJoystickMovement = true;

        //TODO update button/indicator/toggle
        ActivateMovementJoystickObject(true);
    }
    public void EnableLocationProvider()
    {
        if (!UseJoystickMovement)
        {
            return;
        }
        UseJoystickMovement = false;

        //TODO Initialize LocationProvider?
        GLocationProvider.Initialize();
        ActivateMovementJoystickObject(false);
    }


    public void Snap()
    {
        x = actualX;
        y = actualY;
    }



    public void Update()
    {
        if (!UseJoystickMovement)
        {
            if (SetLocationBasedOnLocationProvider())
            {
                return;
            }
            Debug.LogError("Location Provider Denied, Switching to Joystick");
            EnableJoystickMovement();
        }

        SetLocationBasedOnJoystickMovement();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>If Location is Retrieved</returns>
    private bool SetLocationBasedOnLocationProvider()
    {
        if (GLocationProvider.IsInitialized)
        {
            if (GLocationProvider.TryGetLocation(false, out double currentX, out double currentY))
            {
                actualX = currentX;
                actualY = currentY;
            }
            else
            {
                //TODO no location access
                return false;
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
        return true;
    }

    private void SetLocationBasedOnJoystickMovement()
    {

    }


    private void ActivateMovementJoystickObject(bool active)
    {
        G.MovementJoystick.gameObject.SetActive(active);
    }


    public void PlayerReportJoystickMovment(Vector2 movementDelta)
    {
        Debug.Log("Player Moving: " + movementDelta);
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
