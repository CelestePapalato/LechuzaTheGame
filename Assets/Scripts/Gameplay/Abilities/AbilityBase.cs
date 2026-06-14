public abstract class AbilityBase : IAbility
{
    protected readonly IAbilityUser user;
    protected readonly int lightCost;

    protected AbilityBase(IAbilityUser user, int lightCost)
    {
        this.user = user;
        this.lightCost = lightCost;
    }

    public virtual bool CanExecute()
    {
        return user?.LightReservoir != null && user.LightReservoir.HasLight(lightCost);
    }

    public abstract void Execute();

    protected bool TryConsumeLight()
    {
        return user?.LightReservoir != null && user.LightReservoir.TryConsume(lightCost);
    }
}