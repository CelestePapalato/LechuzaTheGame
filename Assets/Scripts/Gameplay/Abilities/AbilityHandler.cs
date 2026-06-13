using System.Collections;
using UnityEngine;

public class AbilityHandler
{
    private readonly DashAbility dashAbility;
    private readonly LightAnchorAbility lightAnchorAbility;
    private readonly MonoBehaviour owner;
    private readonly WaitForSeconds dashCooldown;
    private readonly WaitForSeconds anchorCooldown;

    private bool canUseDash = true;
    private bool canUseAnchor = true;

    public AbilityHandler(IAbilityUser user, MonoBehaviour owner, float dashCooldownLength, float anchorCooldownLength)
    {
        this.owner = owner;
        dashAbility = new DashAbility(user);
        lightAnchorAbility = new LightAnchorAbility(user);
        dashCooldown = new WaitForSeconds(dashCooldownLength);
        anchorCooldown = new WaitForSeconds(anchorCooldownLength);
    }

    public void HandleDash()
    {
        if (!canUseDash) return;
        dashAbility.Execute();
        owner.StartCoroutine(ResetFlag(v => canUseDash = v, dashCooldown));
    }

    public void HandleAnchor()
    {
        if (!canUseAnchor) return;
        lightAnchorAbility.Execute();
        owner.StartCoroutine(ResetFlag(v => canUseDash = v, anchorCooldown));
    }

    public void Cleanup()
    {
        lightAnchorAbility.Cleanup();
    }

    private IEnumerator ResetFlag(System.Action<bool> setFlag, WaitForSeconds cooldown)
    {
        setFlag(false);
        yield return cooldown;
        setFlag(true);
    }
}
