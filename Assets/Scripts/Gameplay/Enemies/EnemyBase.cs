using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField]
    protected TargetDetection targetDetection;

    [Header("Aggressiveness")]
    [SerializeField]
    protected float baseDetectionRadius = 4f;
    [SerializeField]
    protected float aggressiveDetectionBonus = 2f;
    [SerializeField]
    protected float baseSpeed = 3f;
    [SerializeField]
    protected float aggressiveSpeedBonus = 1.5f;
    [SerializeField]
    protected int aggressiveLightThreshold = 2;

    protected Transform target;
    protected float currentSpeed;

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
    }

    protected virtual void UpdateAggressiveness(int currentLight, int maxLight)
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

    protected void ForceLoseTarget()
    {
        if (target == null) return;
        target = null;
        OnTargetLost();
    }

    protected abstract void OnTargetFound(Transform foundTarget);
    protected abstract void OnTargetLost();
}
