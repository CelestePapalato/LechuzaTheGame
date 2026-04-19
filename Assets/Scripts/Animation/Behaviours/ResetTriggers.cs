using UnityEngine;

public class ResetTriggers : StateMachineBehaviour
{
    [SerializeField] private string[] triggersToReset;
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (string trigger in triggersToReset)
        {
            animator.ResetTrigger(trigger);
        }
    }
}
