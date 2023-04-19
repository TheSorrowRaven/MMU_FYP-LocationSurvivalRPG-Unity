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

    public Camera Cam;
    public Transform CamTR;
    public float CamRotateSpeed;
    public float CamVerticalMin;
    public float CamVerticalMax;

    private Quaternion originalRotation;
    private Vector2 rotation;

    public WeaponItem UsingWeaponSO => Player.UsingWeapon;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        originalRotation = CamTR.localRotation;

        PlayerShootAttack.Instance.gameObject.SetActive(false);
        PlayerSwingAttack.Instance.gameObject.SetActive(false);
        if (UsingWeaponSO == null)
        {
            MeleeItem melee = UIInventory.Instance.GetFirstWeaponItem();
            if (melee != null)
            {
                melee.Use();
            }
            else
            {
                //fists?
            }
        }
        else
        {
            UsingWeaponSO.Use();
        }

    }

    public void WeaponChanged()
    {
        if (UsingWeaponSO is RangedItem ranged)
        {
            PlayerShootAttack.Instance.gameObject.SetActive(true);
            PlayerSwingAttack.Instance.gameObject.SetActive(false);
        }
        else
        {
            PlayerShootAttack.Instance.gameObject.SetActive(false);
            PlayerSwingAttack.Instance.gameObject.SetActive(true);
        }
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
        if (TR == null)
        {
            return;
        }
        G.MovementJoystick.ExternalJoystickControl(delta.normalized);
        delta *= Player.CombatMovementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Player.Instance.TryConsumeStaminaToRun())
            {
                delta *= Player.CombatMovementSpeedMultiplier;
            }
        }
        TR.localPosition += TR.forward * delta.y + TR.right * delta.x;
    }

    public bool TrySphereCastZombie(Vector2 screenPosition, out CombatZombie zombie)
    {
        Vector3 fromPosition = CamTR.position;
        Vector3 toPosition = Cam.ScreenToWorldPoint(new(screenPosition.x, screenPosition.y, 1));
        Vector3 direction = toPosition - fromPosition;
        direction.Normalize();
        float radius = 1f;
        float distance = 2f;
        if (Physics.SphereCast(fromPosition, radius, direction, out RaycastHit hit, distance, 1 << 8))
        {
            Transform tr = hit.collider.transform.parent;
            if (tr.TryGetComponent(out zombie))
            {
                //Debug.Log("Hit Zombie");
                return true;
            }
        }
        //Debug.DrawRay(fromPosition, direction * distance, Color.yellow);
        zombie = null;
        return false;
    }

    public void HitZombieWithWeapon(CombatZombie zombie, int damage)
    {
        zombie.PlayerHit(UsingWeaponSO.Damage + damage, (zombie.TR.position - Player.ThisTR.position).normalized);    //TODO
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
