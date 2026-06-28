using System;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public event Action<EnemyAnimatorState> onAnimationStart;
    public event Action<EnemyAnimatorState> onAnimationComplete;

    public void AnimationStartHandler(EnemyAnimatorState state)
    {
        onAnimationStart?.Invoke(state);
    }

    public void AnimationCompleteHandler(EnemyAnimatorState state)
    {
        onAnimationComplete?.Invoke(state);
    }
}
