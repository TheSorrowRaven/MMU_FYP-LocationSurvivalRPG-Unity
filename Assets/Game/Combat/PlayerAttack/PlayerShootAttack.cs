using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerShootAttack : MonoBehaviour
{
    private static PlayerShootAttack instance;
    public static PlayerShootAttack Instance => instance;

    private static G G => G.Instance;
    private static CombatPlayer CombatPlayer => CombatPlayer.Instance;

    public Camera PACam;
    public Camera PlayerCam;
    public Animator Animator;
    public RectTransform Crosshair;
    public float crosshairYOffset;

    [System.NonSerialized] private bool hasInput;
    [System.NonSerialized] private int lastFrameInput;
    [System.NonSerialized] private Vector2 screenPosition;
    [System.NonSerialized] private Vector2 lastScreenPosition;
    [System.NonSerialized] private bool IsNewAction;

    [System.NonSerialized] private CombatZombie zombieTarget;
    [System.NonSerialized] private bool hitZombie;


    [System.NonSerialized] private bool isHolding;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        G.ScreenInput.InputAction += ScreenDragInput;
    }


    private void OnDestroy()
    {
        G.ScreenInput.InputAction -= ScreenDragInput;
    }


    private void ScreenDragInput(Vector2 screenPosition)
    {
        int currentFrame = Time.frameCount;
        if (currentFrame - 1 != lastFrameInput)
        {
            IsNewAction = true;
            lastScreenPosition = screenPosition;
        }
        lastScreenPosition = screenPosition;
        this.screenPosition = screenPosition;
        lastFrameInput = currentFrame;
        hasInput = true;
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
        {
            IsNewAction = false;
            isHolding = false;
            hasInput = false;
            return;
        }
        if (IsNewAction)
        {
            IsNewAction = false;
            isHolding = true;
            Crosshair.gameObject.SetActive(true);
        }
        if (isHolding)
        {
            Crosshair.anchoredPosition = new(screenPosition.x, screenPosition.y + crosshairYOffset);

            if (!hasInput)
            {
                isHolding = false;
                Shoot();
                Crosshair.gameObject.SetActive(false);

            }

        }


        if (hasInput)
        {
            isHolding = true;
            hasInput = false;
        }




    }


    private void Shoot()
    {
        Animator.SetTrigger("Shoot");

        float distance = 1000f;

        Ray ray = PlayerCam.ScreenPointToRay(new(screenPosition.x, screenPosition.y + crosshairYOffset));
        if (Physics.Raycast(ray, out RaycastHit hit, distance, 1 << 8))
        {
            Transform tr = hit.collider.transform.parent;
            if (tr.TryGetComponent(out CombatZombie zombie))
            {
                CombatPlayer.HitZombieWithWeapon(zombie, Player.Instance.RangedDamage);
                Debug.Log($"Shot a zombie");
            }
            else
            {
                Debug.LogError("error??");
            }
        }
        else
        {
            Debug.Log("Miss");
        }
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.cyan, 1);

    }


}
