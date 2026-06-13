using UnityEngine;

// Estaría bueno que consuma luz recién en el pull
// Tipo, hacemos que sea una linterna que lanza el jugador
// y, si quiere, puede atraerse a ella
// Hay que modificar el shader de luz para que "corte" también en las secciones con ancla.
public class LightAnchorAbility : AbilityBase
{
    private readonly LightAnchorProjectile anchorPrefab;
    private LightAnchorProjectile activeAnchor;
    private void HandleAnchorExpired() => activeAnchor = null;

    public LightAnchorAbility(IAbilityUser user)
        : base(user, lightCost: 1)
    {
        this.anchorPrefab = user.AnchorPrefab;
    }

    public override bool CanExecute() =>
        activeAnchor != null || base.CanExecute();

    public override void Execute()
    {
        if (activeAnchor != null)
            PullToAnchor();
        else
            LaunchAnchor();
    }

    private void LaunchAnchor()
    {
        if (!TryConsumeLight()) return;

        Vector2 spawnPos  = user.LightAnchorPivot.position;
        Vector2 launchDir = new Vector2(user.Movement.Facing, 0f);

        LightAnchorProjectile instance = Object.Instantiate(anchorPrefab, spawnPos, Quaternion.identity);
        instance.Initialize(launchDir);
        instance.OnExpired += HandleAnchorExpired;
        activeAnchor = instance;
    }

    private void PullToAnchor()
    {
        if (activeAnchor == null) return;

        activeAnchor.OnExpired -= HandleAnchorExpired;
        Vector2 targetPos = activeAnchor.Pull(); // destruye el projéctil, devuelve posición
        user.Movement.Teleport(targetPos);
        activeAnchor = null;
    }

    // llamada desde AbilityHandler.OnDisable
    public void Cleanup()
    {
        if (activeAnchor == null) return;
        activeAnchor.OnExpired -= HandleAnchorExpired;
        Object.Destroy(activeAnchor.gameObject);
        activeAnchor = null;
    }

}
