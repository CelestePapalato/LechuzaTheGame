using System.Collections;
using UnityEngine;

public class PolillaSombra : EnemyBase
{
    private enum State
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("References")]
    [SerializeField]
    private MothMovement movement;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private AnimationEventHandler animationEventHandler;
    [SerializeField]
    private Damage damage;

    [Header("Light Thresholds")]
    [SerializeField]
    private int brightLightThreshold = 4;
    [SerializeField]
    private int invisibleLightMin = 2;
    [SerializeField]
    private int invisibleLightMax = 3;

    [Header("Movement")]
    [SerializeField]
    private float patrolMaxSpeed = 1.5f;
    [SerializeField]
    private float patrolRange = 3f;
    [SerializeField]
    private float patrolCycleSpeed = 0.5f;

    [Header("Chase")]
    [SerializeField]
    private float alignThreshold = 0.5f;
    [SerializeField]
    private float attackCooldown = 1.5f;

    [Header("Animation Timeouts")]
    [SerializeField]
    private float attackTimeout = 2f;
    [SerializeField]
    private float stunTimeout = 1.5f;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private State state = State.Patrol;
    private Vector2 homePosition;
    private bool isBrightLight;
    private bool attackMovementLocked;
    private bool canAttack = true;
    private Coroutine attackTimeoutCoroutine;
    private Coroutine stunTimeoutCoroutine;

    protected void Awake()
    {
        homePosition = transform.position;

        if (movement == null)
            movement = GetComponent<MothMovement>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (animationEventHandler == null)
            animationEventHandler = GetComponentInChildren<AnimationEventHandler>();
        if (damage == null)
            damage = GetComponentInChildren<Damage>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        movement?.Stop();
        CancelAttack();
        FinishStunTracking();
        CancelInvoke(nameof(EnableAttack));
        canAttack = true;
        StopAllCoroutines();
    }

    private void Start()
    {
        damage?.EnableDamage(false);
    }

    protected override void UpdateAggressiveness(int currentLight, int maxLight)
    {
        bool wasBright = isBrightLight;
        bool isInvisible = currentLight >= invisibleLightMin && currentLight <= invisibleLightMax;
        isBrightLight = currentLight >= brightLightThreshold;

        if (isInvisible)
        {
            targetDetection?.SetDetectionActive(false);
            currentSpeed = baseSpeed;
            ForceLoseTarget();
        }
        else if (isBrightLight)
        {
            targetDetection?.SetDetectionActive(true);
            targetDetection?.SetDetectionRadius(baseDetectionRadius + aggressiveDetectionBonus);
            currentSpeed = baseSpeed + aggressiveSpeedBonus;
            TryAcquireTargetFromDetection();
        }
        else
        {
            targetDetection?.SetDetectionActive(true);
            targetDetection?.SetDetectionRadius(baseDetectionRadius);
            currentSpeed = baseSpeed;
            ForceLoseTarget();
        }

        if (wasBright && !isBrightLight && state == State.Attack)
            CancelAttack();
    }

    private void Update()
    {
        UpdateMovement();
        UpdateStateLogic();
    }

    private void TryAcquireTargetFromDetection()
    {
        if (isStunned || target != null || targetDetection == null) return;

        Transform[] detected = targetDetection.Targets;
        if (detected.Length == 0) return;

        target = detected[0];
        OnTargetFound(target);
    }

    //---- TARGET DETECTION

    protected override void OnTargetFound(Transform foundTarget)
    {
        if (isStunned || !isBrightLight) return;
        SetState(State.Chase);
    }

    protected override void OnTargetLost()
    {
        if (isStunned) return;
        if (state == State.Chase || state == State.Attack)
            SetState(State.Patrol);
    }

    protected override void OnStunned()
    {
        CancelAttack();
        movement?.Stop();
        BeginStunTracking();
    }

    protected override void OnStunEnded()
    {
        FinishStunTracking();
        SetState(target != null && isBrightLight ? State.Chase : State.Patrol);
    }

    //---- MOVEMENT

    private void UpdateMovement()
    {
        if (movement == null || isStunned) return;

        switch (state)
        {
            case State.Patrol:
                movement.SetDestination(GetPatrolDestination(), patrolMaxSpeed);
                break;
            case State.Chase:
                if (target != null)
                    movement.SetDestination(target.position, currentSpeed);
                break;
            case State.Attack:
                if (!attackMovementLocked && target != null)
                    movement.SetDestination(target.position, currentSpeed);
                break;
        }
    }

    private Vector2 GetPatrolDestination()
    {
        float x = homePosition.x + Mathf.Sin(Time.time * patrolCycleSpeed) * patrolRange;
        float y = homePosition.y + Mathf.Cos(Time.time * patrolCycleSpeed * 0.7f) * patrolRange * 0.5f;
        return new Vector2(x, y);
    }

    //---- STATE LOGIC

    private void UpdateStateLogic()
    {
        if (state != State.Chase) return;

        if (target == null || !isBrightLight)
        {
            SetState(State.Patrol);
            return;
        }

        if (canAttack
            && Vector2.Distance(transform.position, target.position) <= alignThreshold)
            SetState(State.Attack);
    }

    private void SetState(State newState)
    {
        if (newState == State.Attack && !isBrightLight)
            return;

        if (newState != State.Attack && state == State.Attack)
            CancelAttack();

        state = newState;

        switch (newState)
        {
            case State.Chase:
                attackMovementLocked = false;
                break;
            case State.Patrol:
                attackMovementLocked = false;
                break;
            case State.Attack:
                attackMovementLocked = false;
                animator?.SetTrigger(AttackHash);
                BeginAttackTracking();
                break;
        }
    }

    //---- ATTACK

    private void BeginAttackTracking()
    {
        FinishAttackTracking();

        if (animationEventHandler != null)
        {
            animationEventHandler.onAnimationStart += HandleAttackStart;
            animationEventHandler.onAnimationComplete += HandleAttackComplete;
        }
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

        if (isBrightLight)
            damage?.EnableDamage(true);
        else
            CancelAttack();

        StartAttackTimeout();
    }

    private void HandleAttackComplete(EnemyAnimatorState animState)
    {
        if (animState != EnemyAnimatorState.ATTACK || state != State.Attack) return;
        FinishAttack();
    }

    private void FinishAttack()
    {
        damage?.EnableDamage(false);
        FinishAttackTracking();
        attackMovementLocked = false;
        BeginAttackCooldown();

        if (state != State.Attack) return;
        SetState(target != null && isBrightLight ? State.Chase : State.Patrol);
    }

    private void CancelAttack()
    {
        damage?.EnableDamage(false);
        FinishAttackTracking();
        attackMovementLocked = false;

        if (state != State.Attack) return;

        state = target != null && isBrightLight ? State.Chase : State.Patrol;
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

    private IEnumerator AttackTimeoutRoutine()
    {
        yield return new WaitForSeconds(attackTimeout);
        attackTimeoutCoroutine = null;

        if (state == State.Attack)
            FinishAttack();
    }

    //---- STUN HANDLER | No hay animación ni se encuentra implementado aún.
    // Tendría que ir en EnemyBase.

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
