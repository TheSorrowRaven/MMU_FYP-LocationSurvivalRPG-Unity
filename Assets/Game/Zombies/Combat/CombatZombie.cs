using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CombatZombie : MonoBehaviour
{
    private static CombatPlayer CombatPlayer => CombatPlayer.Instance;
    private static CombatZombieManager CombatZombieManager => CombatZombieManager.Instance;

    public Transform TR;
    public Rigidbody RB;
    public GameObject CLObj;
    [SerializeField] private NavMeshAgent Agent;
    [SerializeField] private Animator Animator;

    public AudioSource GroanAudio;
    public AudioSource DieAudio;
    public AudioSource AttackAudio;

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

    public float SprintChaseTime;
    public float SprintChaseCooldown;
    private float SprintChaseCount;
    private float SprintChaseCooldownCount;
    private bool sprintChasing;

    public float AttackTotalTime;
    public float AttackCooldownTotalTime;
    private bool IsAttacking;
    private bool AttackIsReady;
    private float attackTimeCount;
    private bool attackTriggerSet;

    private float MoveSpeed;
    private float lastMoveSpeed;

    private Vector3 playerLastPos;

    public float AttackHitCheckTime;
    public float AttackHitCheckTime2;
    private int hitCountPerAttack;

    public int HealthDamage;
    public int ZombificationDamage;

    public int health;
    private bool isDead = false;


    public enum State
    {
        None,
        Wander,
        Chasing,
        Attacking,
        Dying,
        Dead,
    }

    public State CurrentState;

    private void Start()
    {
        if (CurrentState == State.None)
        {
            ChangeState(State.Wander);
            Debug.Log("Set to wander");
        }
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
            case State.Dying:
                DyingUpdate();
                break;
            case State.Dead:
                DeadUpdate();
                break;
        }
        if (MoveSpeed != lastMoveSpeed)
        {
            //Set only when different than last
            lastMoveSpeed = MoveSpeed;
            Animator.SetFloat("MoveSpeed", MoveSpeed);
        }
    }

    public void PlayerHit(int damage, Vector3 dir)
    {
        if (isDead)
        {
            return;
        }
        health -= damage;
        if (health < 0)
        {
            DieAudio.Play();
            ChangeState(State.Dying);
            isDead = true;
        }
        else
        {
            GroanAudio.Play();
            RB.AddForce(dir * 3, ForceMode.Impulse);
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
                MoveSpeed = WanderSpeed;
                MoveTowards(wanderPos);
            }
        }
        else
        {
            MoveSpeed = 0;
            Agent.isStopped = true;
        }

        TryDetectPlayer();
    }

    private void ChasingUpdate()
    {
        MoveSpeed = ChaseSpeed;//TODO Run?

        if (sprintChasing)
        {
            SprintChaseCount -= Time.deltaTime;
            if (SprintChaseCount < 0)
            {
                sprintChasing = false;
                SprintChaseCount = SprintChaseTime;
                SprintChaseCooldownCount = SprintChaseCooldown;
                playerLastPos = Vector3.zero;   // Reset player last pos so it updates
            }
            MoveSpeed = ChaseRunSpeed;
        }
        else
        {
            SprintChaseCooldownCount -= Time.deltaTime;
            if (SprintChaseCooldownCount < 0)
            {
                GroanAudio.Play();
                sprintChasing = true;
                SprintChaseCount = SprintChaseTime;
                SprintChaseCooldownCount = SprintChaseCooldown;
                playerLastPos = Vector3.zero;
            }
        }

        Vector3 playerPos = CombatPlayer.TR.localPosition;

        float distance = Vector3.Distance(playerPos, TR.localPosition);
        if (distance < AttackDistance)
        {
            ChangeState(State.Attacking);
        }
        else
        {
            if (playerPos != playerLastPos)
            {
                playerLastPos = playerPos;
                MoveTowards(playerPos);
            }
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
                attackTriggerSet = false;
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

                if (!attackTriggerSet)
                {
                    AttackAudio.Play();
                    Animator.SetTrigger("Attack");
                }

                attackTriggerSet = true;

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

        //RotateTowards(CombatPlayer.TR.localPosition);
    }

    private void DyingUpdate()
    {
        ChangeState(State.Dead);
    }
    private void DeadUpdate()
    {
    }

    private void MoveTowards(Vector3 pos)
    {
        Agent.speed = MoveSpeed;
        Agent.SetDestination(pos);
        Agent.isStopped = false;
    }

    private void RotateTowards(Vector3 pos)
    {
        Vector3 zombiePos = TR.localPosition;
        Vector3 direction = pos - zombiePos;
        direction.Normalize();

        float angle = Vector3.SignedAngle(TR.forward, direction, Vector3.up);
        float currentAngle = TR.localRotation.eulerAngles.y;
        if (angle < 5)
        {
            //TR.localRotation = Quaternion.Euler(0, currentAngle + angle, 0);
            TR.LookAt(new Vector3(pos.x, zombiePos.y, pos.z), Vector3.up);
        }
        else
        {
            float turnMax = Mathf.Min(Time.deltaTime * TurnSpeed, angle);
            TR.localRotation = Quaternion.RotateTowards(TR.localRotation, Quaternion.Euler(0, currentAngle + angle, 0), turnMax);
        }
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

    public void ForceDetectPlayer()
    {
        DetectedPlayer();
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
        ChangeState(State.Wander);
        ChangeState(State.Chasing);
        GroanAudio.Play();
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
            case State.Dying:
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
            case State.Dying:
                EnterDyingState();
                break;
            case State.Dead:
                EnterDeadState();
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
        CombatZombieManager.CombatZombieDetectedPlayer(this);
        Animator.SetBool("Chasing", true);
    }

    private void EnterDyingState()
    {
        RB.useGravity = false;
        CLObj.SetActive(false);
        Animator.SetBool("DieForward", false);
        Animator.SetTrigger("Die");
    }

    private void EnterDeadState()
    {
        CombatZombieManager.CombatZombieDied(this);
        Player.Instance.ZombieKilledGainExperience(this);
        Agent.isStopped = true;
        Agent.enabled = false;
        RB.velocity = Vector3.zero;
        RB.angularVelocity = Vector3.zero;
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
