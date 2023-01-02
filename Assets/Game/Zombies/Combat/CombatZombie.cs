using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CombatZombie : MonoBehaviour
{
    private static CombatPlayer CombatPlayer => CombatPlayer.Instance;

    public Transform TR;
    public Rigidbody RB;
    [SerializeField] private Animator Animator;

    //Relative to Dot Product
    [Tooltip("0 is 180 vision, 0.5 is 90 vision")]
    public float ViewCone;

    //Detects player if within this distance regardless of view cone
    public float CloseDistance;
    public float ViewDistance;

    public Bounds WanderBounds;


    public float WanderPauseTime;
    public float WanderMoveTime;
    private bool WanderIsMoving;
    private float wanderTimeCount;

    private Vector3 wanderPos;



    public float TurnSpeed;

    public float WanderSpeed;

    public float ChaseSpeed;
    public float ChaseRunSpeed;

    public float AttackDistance;

    public float AttackTotalTime;
    public float AttackCooldownTotalTime;
    private bool IsAttacking;
    private bool AttackIsReady;
    private float attackTimeCount;

    private float MoveSpeed;
    private float lastMoveSpeed;

    public float AttackHitCheckTime;
    public float AttackHitCheckTime2;
    private int hitCountPerAttack;


    public int HealthDamage;
    public int ZombificationDamage;


    public enum State
    {
        None,
        Wander,
        Chasing,
        Attacking,
    }

    public State CurrentState;

    private void Start()
    {
        ChangeState(State.Wander);
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case State.Wander:
                WanderUpdate();
                break;
            case State.Chasing:
                ChasingUpdate();
                break;
            case State.Attacking:
                AttackingUpdate();
                break;
        }
        if (MoveSpeed != lastMoveSpeed)
        {
            //Set only when different than last
            lastMoveSpeed = MoveSpeed;
            Animator.SetFloat("MoveSpeed", MoveSpeed);
        }
    }

    private void WanderUpdate()
    {
        wanderTimeCount -= Time.deltaTime;
        bool wasMoving = WanderIsMoving;
        if (wanderTimeCount < 0)
        {
            WanderIsMoving = !WanderIsMoving;
            if (WanderIsMoving)
            {
                wanderTimeCount = Random.Range(WanderMoveTime / Random.Range(1f, 3f), WanderMoveTime);
            }
            else
            {
                wanderTimeCount = Random.Range(WanderPauseTime / Random.Range(1f, 3f), WanderPauseTime);
            }
        }

        if (WanderIsMoving)
        {
            if (!wasMoving)
            {
                // Determine wander position
                wanderPos = G.RandomPointInBounds(WanderBounds);
                wanderPos.y = 0;
            }
            MoveSpeed = WanderSpeed;
            MoveTowards(wanderPos);
        }
        else
        {
            MoveSpeed = 0;
        }

        TryDetectPlayer();
    }

    private void ChasingUpdate()
    {
        MoveSpeed = ChaseSpeed;//TODO Run?

        Vector3 playerPos = CombatPlayer.TR.localPosition;

        float distance = Vector3.Distance(playerPos, TR.localPosition);
        if (distance < AttackDistance)
        {
            ChangeState(State.Attacking);
        }
        else
        {
            MoveTowards(playerPos);
        }

    }

    private void AttackingUpdate()
    {
        MoveSpeed = 0;

        attackTimeCount -= Time.deltaTime;


        if (attackTimeCount < 0)
        {
            if (IsAttacking)
            {
                //Change to Cooldown
                attackTimeCount = AttackCooldownTotalTime;
                AttackIsReady = false;
            }
            else
            {
                AttackIsReady = true;
            }
            IsAttacking = false;
        }

        if (AttackIsReady)
        {
            Vector3 playerPos = CombatPlayer.TR.localPosition;
            float distance = Vector3.Distance(playerPos, TR.localPosition);

            if (distance < AttackDistance)
            {
                attackTimeCount = AttackTotalTime;
                Animator.SetTrigger("Attack");

                IsAttacking = true;
                AttackIsReady = false;
                hitCountPerAttack = 0;
            }
            else
            {
                ChangeState(State.Chasing);
            }
        }

        if (IsAttacking)
        {
            float timeCheck = AttackTotalTime - attackTimeCount;
            if (hitCountPerAttack == 0 && timeCheck > AttackHitCheckTime)
            {
                TryHitPlayer();
                hitCountPerAttack++;
            }
            else if (hitCountPerAttack == 1 && timeCheck > AttackHitCheckTime2)
            {
                TryHitPlayer();
                hitCountPerAttack++;
            }
        }

    }

    private void MoveTowards(Vector3 pos)
    {
        Vector3 zombiePos = TR.localPosition;
        Vector3 direction = pos - zombiePos;
        direction.Normalize();

        float angle = Vector3.SignedAngle(TR.forward, direction, Vector3.up);
        if (angle < 1)
        {
            //Prevent Jitter
            angle = 0;
        }
        float currentAngle = TR.localRotation.eulerAngles.y;

        TR.localRotation = Quaternion.RotateTowards(TR.localRotation, Quaternion.Euler(0, currentAngle + angle, 0), Time.deltaTime * TurnSpeed);
        Vector3 movement = TR.forward * (Time.deltaTime * MoveSpeed);
        TR.localPosition += movement;
    }


    private void TryDetectPlayer()
    {
        Vector3 playerPos = CombatPlayer.TR.localPosition;
        Vector3 zombiePos = TR.localPosition;

        Vector3 direction = playerPos - zombiePos;
        float distance = direction.magnitude;
        direction /= distance;

        if (distance < CloseDistance)
        {
            Debug.Log($"Detected Player within Close Distance, Dist: {distance}");
            DetectedPlayer();
        }
        else if (distance < ViewDistance)
        {
            float dot = Vector3.Dot(TR.forward, direction);

            if (dot > ViewCone)
            {
                //Seen player
                Debug.Log($"Detected Player with Cone: {dot}, Dist: {distance}");
                DetectedPlayer();
            }
        }
    }

    private void TryHitPlayer()
    {
        Vector3 playerPos = CombatPlayer.TR.localPosition;
        float distance = Vector3.Distance(playerPos, TR.localPosition);

        if (distance < AttackDistance)
        {
            //hit Player
            HitPlayer();
        }

    }

    private void HitPlayer()
    {
        CombatPlayer.HitByZombie(this);
    }

    private void DetectedPlayer()
    {
        ChangeState(State.Chasing);
    }

    private void ChangeState(State state)
    {
        switch (CurrentState)
        {
            case State.Wander:
                ExitWanderState();
                break;
            case State.Chasing:
                ExitChasingState();
                break;
            case State.Attacking:
                ExitAttackingState();
                break;
        }
        CurrentState = state;
        switch (state)
        {
            case State.Wander:
                EnterWanderState();
                break;
            case State.Chasing:
                EnterChasingState();
                break;
            case State.Attacking:
                EnterAttackingState();
                break;
        }
    }

    private void EnterWanderState()
    {
        Animator.SetBool("Wandering", true);
        wanderTimeCount = Random.value * WanderPauseTime;
        WanderIsMoving = false;
    }
    
    private void ExitWanderState()
    {
        Animator.SetBool("Wandering", false);
    }

    private void EnterChasingState()
    {
        Animator.SetBool("Chasing", true);
    }

    private void ExitChasingState()
    {
        Animator.SetBool("Chasing", false);
    }

    private void EnterAttackingState()
    {
        Animator.SetBool("Attacking", true);
        IsAttacking = false;
        attackTimeCount = 0;
    }

    private void ExitAttackingState()
    {
        Animator.SetBool("Attacking", false);
        IsAttacking = false;
    }


}
