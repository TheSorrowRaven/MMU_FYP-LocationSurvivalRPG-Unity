using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatPlayer : MonoBehaviour
{
    private static CombatPlayer instance;
    public static CombatPlayer Instance => instance;

    private static G G => G.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;

    private static Player Player => Player.Instance;

    public Transform TR;
    public Rigidbody RB;

    public Transform CamTR;
    public float CamRotateSpeed;
    public float CamVerticalMin;
    public float CamVerticalMax;

    private Quaternion originalRotation;
    private Vector2 rotation;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        originalRotation = CamTR.localRotation;
    }

    public void RotateView(Vector2 delta)
    {
        Vector2 rotate = -delta * CamRotateSpeed;
        rotation += rotate;
        Quaternion yQuaternion = Quaternion.AngleAxis(rotation.y, Vector3.left);
        Quaternion xQuaternion = Quaternion.AngleAxis(rotation.x, Vector3.up);
        CamTR.localRotation = originalRotation * yQuaternion;
        TR.localRotation = originalRotation * xQuaternion;
    }

    public void Move(Vector2 delta)
    {
        G.MovementJoystick.ExternalJoystickControl(delta.normalized);
        delta *= Player.CombatMovementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            delta *= Player.CombatMovementSpeedMultiplier;
        }
        TR.localPosition += TR.forward * delta.y + TR.right * delta.x;
    }

    public void HitByZombie(CombatZombie zombie)
    {
        int healthDamage = zombie.HealthDamage;
        int zombificationDamage = zombie.ZombificationDamage;
        Player.Damaged(healthDamage, zombificationDamage);
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle %= 360;
        if ((angle >= -360f) && (angle <= 360f))
        {
            if (angle < -360f)
            {
                angle += 360f;
            }
            if (angle > 360f)
            {
                angle -= 360f;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

}
