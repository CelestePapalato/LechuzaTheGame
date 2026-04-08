using UnityEngine;

public class HealthbarIcon : MonoBehaviour
{
    [SerializeField]
    private bool startsOn = false;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string animationBoolean = "on";

    private bool isOn = false;
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
        if (animator)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Start()
    {
        IsOn = startsOn;
    }

    void UpdateIconState()
    {
        animator.SetBool(animationBoolean, isOn);
    }
}
