using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField]
    private TargetDetection targetDetection;

    [Header("Aggressiveness")]
    [SerializeField]
    private float baseDetectionRadius = 4f;
    [SerializeField]
    private float aggressiveDetectionBonus = 2f;
    [SerializeField]
    private float baseSpeed = 3f;
    [SerializeField]
    private float aggressiveSpeedBonus = 1.5f;
    [SerializeField]
    private int aggressiveLightThreshold = 2;

    protected Transform target;
    protected float currentSpeed;
    protected bool isStunned;

    private Coroutine stunCoroutine;

    protected virtual void OnEnable()
    {
        if (targetDetection != null)
        {
            targetDetection.TargetFound.AddListener(HandleTargetFound);
            targetDetection.TargetLost.AddListener(HandleTargetLost);
        }

        PlayerState.OnLightChange += UpdateAggressiveness;

        int currentLight = PlayerState.Instance != null
            ? PlayerState.Instance.light.Current
            : aggressiveLightThreshold + 1;
        UpdateAggressiveness(currentLight, PlayerState.Instance?.light.Max ?? 5);
    }

    protected virtual void OnDisable()
    {
        if (targetDetection != null)
        {
            targetDetection.TargetFound.RemoveListener(HandleTargetFound);
            targetDetection.TargetLost.RemoveListener(HandleTargetLost);
        }

        PlayerState.OnLightChange -= UpdateAggressiveness;

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }
    }

    public void Stun(float duration)
    {
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    protected void UpdateAggressiveness(int currentLight, int maxLight)
    {
        bool isAggressive = currentLight >= aggressiveLightThreshold;

        if (targetDetection != null)
        {
            targetDetection.SetDetectionRadius(isAggressive
                ? baseDetectionRadius + aggressiveDetectionBonus
                : baseDetectionRadius);
        }

        currentSpeed = isAggressive
            ? baseSpeed + aggressiveSpeedBonus
            : baseSpeed;
    }

    private void HandleTargetFound(Transform foundTarget)
    {
        target = foundTarget;
        OnTargetFound(foundTarget);
    }

    private void HandleTargetLost(Transform lostTarget)
    {
        if (target != lostTarget) return;

        target = targetDetection != null && targetDetection.Targets.Length > 0
            ? targetDetection.Targets[0]
            : null;

        if (target == null)
            OnTargetLost();
    }

/*
    Stun and health not really implemented.
    Maybe Light Anchor could stun enemies, 
    but LechucitaSHSH can't really kill enemies.
*/

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        OnStunned();

        yield return new WaitForSeconds(duration);

        isStunned = false;
        stunCoroutine = null;
        OnStunEnded();
    }

    protected abstract void OnTargetFound(Transform foundTarget);
    protected abstract void OnTargetLost();
    protected abstract void OnStunned();
    protected abstract void OnStunEnded();
}
