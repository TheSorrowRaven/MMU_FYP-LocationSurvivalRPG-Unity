using JetBrains.Annotations;
using Mapbox.Utils;
using System;
using UnityEngine;

public class PlayerLocation
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

    public PlayerLocation(double x, double y)
    {
        this.x = x;
        this.y = y;
        actualX = x;
        actualY = y;
    }
    public PlayerLocation() : this(0, 0)
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

    private Vector2 movement;
    private bool usedWASD;



    /// <summary>
    /// Disables GPS Location. Opposite to EnableLocationProvider
    /// </summary>
    public void EnableJoystickMovement()
    {
        UseJoystickMovement = true;

        //TODO update button/indicator/toggle
        ActivateMovementJoystickObject(true);
    }
    /// <summary>
    /// Disables Joystick Movement. Opposite to EnableJoystickMovement
    /// </summary>
    public void EnableLocationProvider()
    {
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
            //This will ask for location permission again (unless device says no)
            if (SetLocationBasedOnLocationProvider())
            {
                ForceEnableGPSToggle();
                return;
            }
            Debug.LogError("Location Provider Denied, Switching to Joystick");
            ForceDisableGPSToggle();
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

        if (smoothen && G.Haversine(x, y, actualX, actualY) < GameSettings.LocationSnappingDistance)
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
        if (movement.x == 0 && movement.y == 0)
        {
            return;
        }
        //joystickMovement.Normalize();

        double rotation = (G.MainCameraTR.localRotation.eulerAngles.y) * Mathf.Deg2Rad;
        //y and x is swapped because Geo coordinates start with latitude (y) then longitude (x)
        double movementSpeed = GameSettings.MovementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Player.Instance.TryConsumeStaminaToRun())
            {
                movementSpeed *= GameSettings.MovementSpeedMultiplier;
            }
        }
        double xChange = movement.y * movementSpeed;
        double yChange = movement.x * movementSpeed;

        double xRotated = xChange * Math.Cos(rotation) - yChange * Math.Sin(rotation);
        double yRotated = xChange * Math.Sin(rotation) + yChange * Math.Cos(rotation);

        actualX += xRotated;
        actualY += yRotated;

        Snap();

        movement = Vector2.zero;
    }


    private void ActivateMovementJoystickObject(bool active)
    {
        G.MovementJoystick.gameObject.SetActive(active);
    }


    public void PlayerReportMovement(Vector2 movementDelta)
    {
        movement = movementDelta;
    }

    public void UIReportGPSToggle(bool gpsOn)
    {
        if (gpsOn)
        {
            EnableLocationProvider();
        }
        else
        {
            EnableJoystickMovement();
        }
    }
    private void ForceDisableGPSToggle()
    {
        G.GPSToggle.ThisToggle.isOn = false;
    }
    private void ForceEnableGPSToggle()
    {
        if (G.GPSToggle.ThisToggle.isOn)
        {
            return;
        }
        G.GPSToggle.ThisToggle.isOn = true;
    }









    public static implicit operator Vector2d(PlayerLocation location)
    {
        return new(location.x, location.y);
    }
    public static implicit operator PlayerLocation(Vector2d vector2d)
    {
        return new(vector2d.x, vector2d.y);
    }
    public static implicit operator Vector2ds(PlayerLocation location)
    {
        return new(location.x, location.y);
    }

    public override string ToString()
    {
        return $"{x}, {y}";
    }

}
