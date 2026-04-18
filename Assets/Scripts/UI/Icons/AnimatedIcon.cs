using UnityEngine;

public class AnimatedIcon : MonoBehaviour, IIcon
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string animationBoolean = "on";

    private bool isOn = true;
    public bool IsOn
    {
        get => isOn;
        set
        {
            if (value != isOn)
            {
                isOn = value;
                UpdateIconState();
            }
        }
    }

    private void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        UpdateIconState();
    }

    void UpdateIconState()
    {
        animator.SetBool(animationBoolean, isOn);
    }
}
