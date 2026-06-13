public class DashAbility : AbilityBase
{
    public DashAbility(IAbilityUser user) : base(user, lightCost: 1) { }

    public override void Execute()
    {
        if (!TryConsumeLight()) return;
        user.Movement.TriggerDash();
    }
}