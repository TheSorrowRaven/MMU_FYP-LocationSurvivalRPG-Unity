using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private static G G => G.Instance;
    private static CombatPlayer CombatPlayer => CombatPlayer.Instance;

    public Camera PACam;
    public LineRenderer SwipeLR;
    public Animator Animator;

    public int SwipePoints;

    [System.NonSerialized] private int swipeIndex;

    [System.NonSerialized] private bool hasInput;
    [System.NonSerialized] private int lastFrameInput;
    [System.NonSerialized] private Vector2 screenPosition;
    [System.NonSerialized] private Vector2 lastScreenPosition;
    [System.NonSerialized] private Vector2 delta;
    [System.NonSerialized] private bool IsNewAction;

    [System.NonSerialized] private CombatZombie zombieTarget;
    [System.NonSerialized] private bool hitZombie;

    [System.NonSerialized] private bool isAnimating;
    [System.NonSerialized] private float animationTimeCount;

    private void Awake()
    {
        SwipeLR.positionCount = 0;
    }

    private void Start()
    {
        G.ScreenInput.AddAsInputAction(ScreenDragInput);
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
        if (isAnimating)
        {
            animationTimeCount -= Time.deltaTime;
            if (animationTimeCount < 0)
            {
                isAnimating = false;
                CombatPlayer.HitZombieWithWeapon(zombieTarget);
                hitZombie = false;
                zombieTarget = null;
            }
        }
        if (hasInput)
        {

            hasInput = false;
            if (IsNewAction)
            {
                IsNewAction = false;
                swipeIndex = 0;
                hitZombie = false;
                zombieTarget = null;
            }

            if (IsNewAction && lastScreenPosition == screenPosition)
            {
                DequeuePoint();
            }
            else
            {
                SetPoint(screenPosition);
                bool hit;
                if (hit = CombatPlayer.TrySphereCastZombie(screenPosition, out CombatZombie zombie))
                {
                    hitZombie = hit;
                    zombieTarget = zombie;

                    //TODO move
                    Animator.SetTrigger("Swing");
                    const float hitTime = 0.1333333333f;

                    animationTimeCount = hitTime;
                    isAnimating = true;
                }
            }
        }
        else
        {
            DequeuePoint();
        }

    }

    private void SetPoint(Vector2 screenPos)
    {

        if (swipeIndex >= SwipePoints)
        {
            DequeuePoint();
            swipeIndex = SwipePoints - 1;
        }

        SwipeLR.positionCount = swipeIndex + 1;
        Vector3 worldPos = PACam.ScreenToWorldPoint(new(screenPos.x, screenPos.y, 1));
        SwipeLR.SetPosition(swipeIndex, worldPos);
        swipeIndex++;

    }

    private void DequeuePoint()
    {
        if (SwipeLR.positionCount == 0)
        {
            return;
        }
        for (int i = 0; i < SwipeLR.positionCount - 1; i++)
        {
            SwipeLR.SetPosition(i, SwipeLR.GetPosition(i + 1));
        }
        SwipeLR.positionCount--;
    }

}
