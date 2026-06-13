using System.Collections;
using UnityEngine;

public class AbilityHandler
{
    private readonly DashAbility dashAbility;
    private readonly MonoBehaviour owner;
    private readonly WaitForSeconds dashCooldown;

    private bool canUseDash = true;

    public AbilityHandler(IAbilityUser user, MonoBehaviour owner, float dashCooldownLength)
    {
        this.owner = owner;
        dashAbility = new DashAbility(user);
        dashCooldown = new WaitForSeconds(dashCooldownLength);
    }

    public void HandleDash()
    {
        if (!canUseDash) return;
        dashAbility.Execute();
        owner.StartCoroutine(ResetFlag(v => canUseDash = v, dashCooldown));
    }

    private IEnumerator ResetFlag(System.Action<bool> setFlag, WaitForSeconds cooldown)
    {
        setFlag(false);
        yield return cooldown;
        setFlag(true);
    }
}
