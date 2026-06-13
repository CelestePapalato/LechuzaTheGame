using UnityEngine;

public abstract class AbilityBase : MonoBehaviour, IAbility
{
    [SerializeField] protected LightReservoir lightReservoir;
    [SerializeField] protected int lightCost = 1;

    public virtual bool CanExecute()
    {
        return lightReservoir != null && lightReservoir.HasLight(lightCost);
    }

    public abstract void Execute();

    protected bool TryConsumeLight()
    {
        return lightReservoir != null && lightReservoir.TryConsume(lightCost);
    }
}