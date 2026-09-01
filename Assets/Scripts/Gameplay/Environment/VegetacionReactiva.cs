using UnityEngine;

public class VegetacionReactiva : MonoBehaviour
{
    [Header("Light")]
    [SerializeField]
    private int lightThreshold = 2;

    [Header("Damage")]
    [SerializeField]
    private Collider2D contractedCollider;
    [SerializeField]
    private Collider2D expandedCollider;

    [Header("Visual")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string expandedBoolParam = "IsExpanded";

    [Header("Debug")]
    [SerializeField]
    private bool debug;
    [SerializeField]
    private SpriteRenderer debugRenderer;
    [SerializeField]
    private Sprite contractedSprite;
    [SerializeField]
    private Sprite expandedSprite;

    private int expandedBoolHash;

    private void Awake()
    {
        expandedBoolHash = Animator.StringToHash(expandedBoolParam);
    }

    private void OnEnable()
    {
        PlayerState.OnLightChange += HandleLightChange;

        int current = PlayerState.Instance != null
            ? PlayerState.Instance.light.Current
            : 0;
        HandleLightChange(current, PlayerState.Instance?.light.Max ?? 5);
    }

    private void OnDisable()
    {
        PlayerState.OnLightChange -= HandleLightChange;
    }

    private void HandleLightChange(int currentLight, int maxLight)
    {
        ApplyState(currentLight >= lightThreshold);
    }

    private void ApplyState(bool isExpanded)
    {
        if (contractedCollider != null)
            contractedCollider.enabled = !isExpanded;

        if (expandedCollider != null)
            expandedCollider.enabled = isExpanded;

        if (debug)
        {
            if (animator != null)
                animator.enabled = false;

            if (debugRenderer != null)
                debugRenderer.sprite = isExpanded ? expandedSprite : contractedSprite;
        }
        else
        {
            if (animator != null)
            {
                animator.enabled = true;
                animator.SetBool(expandedBoolHash, isExpanded);
            }
        }
    }
}
