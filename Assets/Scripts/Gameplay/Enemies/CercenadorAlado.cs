using System.Collections;
using UnityEngine;

public class CercenadorAlado : EnemyBase
{
    private enum State
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("References")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private AnimationEventHandler animationEventHandler;
    [SerializeField]
    private BirdMovement movement;

    [Header("Movement")]
    [SerializeField]
    private float patrolMaxSpeed = 1.5f;
    [SerializeField]
    private float patrolRange = 3f;
    [SerializeField]
    private float patrolCycleSpeed = 0.5f;

    [Header("Chase")]
    [SerializeField]
    private float positionOffset = 2f;
    [SerializeField]
    private float alignThreshold = 0.5f;
    [SerializeField, Range(0f, 1f)]
    private float attackFromAboveProbability = 0.75f;
    [SerializeField]
    private float attackCooldown = 1.5f;

    [Header("Animation Timeouts")]
    [SerializeField]
    private float attackTimeout = 2f;
    [SerializeField]
    private float stunTimeout = 1.5f;

    private static readonly int TargetBelowHash = Animator.StringToHash("Target Below");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private State state = State.Patrol;
    private Vector2 homePosition;
    private bool attackFromAbove;
    private bool attackMovementLocked;
    private bool canAttack = true;
    private Coroutine attackTimeoutCoroutine;
    private Coroutine stunTimeoutCoroutine;

    protected void Awake()
    {
        homePosition = transform.position;

        if (movement == null)
            movement = GetComponent<BirdMovement>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (animationEventHandler == null)
            animationEventHandler = GetComponentInChildren<AnimationEventHandler>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        movement?.Stop();
        FinishAttackTracking();
        FinishStunTracking();
        CancelInvoke(nameof(EnableAttack));
        canAttack = true;
        StopAllCoroutines();
    }

    private void Update()
    {
        UpdateMovement();
        UpdateStateLogic();
    }

    //---- TARGET DETECTION

    protected override void OnTargetFound(Transform foundTarget)
    {
        if (isStunned) return;
        SetState(State.Chase);
    }

    protected override void OnTargetLost()
    {
        if (isStunned) return;
        if (state == State.Chase)
            SetState(State.Patrol);
    }

    protected override void OnStunned()
    {
        FinishAttackTracking();
        movement?.Stop();
        BeginStunTracking();
    }

    protected override void OnStunEnded()
    {
        FinishStunTracking();
        SetState(target != null ? State.Chase : State.Patrol);
    }

    //---- MOVEMENT

    private void UpdateMovement()
    {
        if (movement == null) return;

        switch (state)
        {
            case State.Patrol:
                movement.SetDestination(GetPatrolDestination(), patrolMaxSpeed);
                break;
            case State.Chase:
                if (target != null)
                    movement.SetDestination(GetAttackPosition(), currentSpeed);
                break;
            case State.Attack:
                if (!attackMovementLocked && target != null)
                    movement.SetDestination(GetAttackPosition(), currentSpeed);
                break;
        }
    }

    private Vector2 GetPatrolDestination()
    {
        float x = homePosition.x + Mathf.Sin(Time.time * patrolCycleSpeed) * patrolRange;
        return new Vector2(x, homePosition.y);
    }

    private Vector2 GetAttackPosition()
    {
        float desiredY = target.position.y + (attackFromAbove ? positionOffset : -positionOffset);
        return new Vector2(target.position.x, desiredY);
    }

    //---- STATE LOGIC

    private void UpdateStateLogic()
    {
        if (state != State.Chase) return;

        if (target == null)
        {
            SetState(State.Patrol);
            return;
        }

        UpdateTargetBelow();

        if (canAttack
            && Vector2.Distance(transform.position, GetAttackPosition()) <= alignThreshold)
            SetState(State.Attack);
    }

    private void UpdateTargetBelow()
    {
        if (animator == null || target == null) return;
        animator.SetBool(TargetBelowHash, target.position.y < transform.position.y);
    }

    private void SetState(State newState)
    {
        state = newState;

        switch (newState)
        {
            case State.Chase:
                attackFromAbove = Random.value < attackFromAboveProbability;
                attackMovementLocked = false;
                break;
            case State.Patrol:
                attackMovementLocked = false;
                break;
            case State.Attack:
                attackMovementLocked = false;
                UpdateTargetBelow();
                animator?.SetTrigger(AttackHash);
                BeginAttackTracking();
                break;
        }
    }

    //---- ANIMATION HANDLER

    private void BeginAttackTracking()
    {
        FinishAttackTracking();

        if (animationEventHandler != null)
        {
            animationEventHandler.onAnimationStart += HandleAttackStart;
            animationEventHandler.onAnimationComplete += HandleAttackComplete;
        }
    }

    private void StartAttackTimeout()
    {
        StopAttackTimeout();
        attackTimeoutCoroutine = StartCoroutine(AttackTimeoutRoutine());
    }

    private void StopAttackTimeout()
    {
        if (attackTimeoutCoroutine == null) return;
        StopCoroutine(attackTimeoutCoroutine);
        attackTimeoutCoroutine = null;
    }

    private void FinishAttackTracking()
    {
        if (animationEventHandler != null)
        {
            animationEventHandler.onAnimationStart -= HandleAttackStart;
            animationEventHandler.onAnimationComplete -= HandleAttackComplete;
        }

        StopAttackTimeout();
    }

    private void HandleAttackStart(EnemyAnimatorState animState)
    {
        if (animState != EnemyAnimatorState.ATTACK || state != State.Attack) return;

        attackMovementLocked = true;
        movement?.Stop();
        StartAttackTimeout();
    }

    private void HandleAttackComplete(EnemyAnimatorState animState)
    {
        if (animState != EnemyAnimatorState.ATTACK || state != State.Attack) return;
        FinishAttack();
    }

    private void FinishAttack()
    {
        FinishAttackTracking();
        attackMovementLocked = false;
        BeginAttackCooldown();

        if (state != State.Attack) return;
        SetState(target != null ? State.Chase : State.Patrol);
    }

    private void BeginAttackCooldown()
    {
        canAttack = false;
        CancelInvoke(nameof(EnableAttack));
        Invoke(nameof(EnableAttack), attackCooldown);
    }

    private void EnableAttack()
    {
        canAttack = true;
    }

    private IEnumerator AttackTimeoutRoutine()
    {
        yield return new WaitForSeconds(attackTimeout);
        attackTimeoutCoroutine = null;

        if (state == State.Attack)
            FinishAttack();
    }

    //---- STUN HANDLER | No hay animaci�n ni se encuentra implementado a�n

    private void BeginStunTracking()
    {
        FinishStunTracking();

        if (animationEventHandler != null)
            animationEventHandler.onAnimationComplete += HandleStunComplete;

        stunTimeoutCoroutine = StartCoroutine(StunTimeoutRoutine());
    }

    private void StopStunTimeout()
    {
        if (stunTimeoutCoroutine == null) return;
        StopCoroutine(stunTimeoutCoroutine);
        stunTimeoutCoroutine = null;
    }

    private void FinishStunTracking()
    {
        if (animationEventHandler != null)
            animationEventHandler.onAnimationComplete -= HandleStunComplete;

        StopStunTimeout();
    }

    private void HandleStunComplete(EnemyAnimatorState animState)
    {
        if (animState != EnemyAnimatorState.STUN || !isStunned) return;
        FinishStunTracking();
    }

    private IEnumerator StunTimeoutRoutine()
    {
        yield return new WaitForSeconds(stunTimeout);
        stunTimeoutCoroutine = null;

        if (isStunned)
            FinishStunTracking();
    }
}
