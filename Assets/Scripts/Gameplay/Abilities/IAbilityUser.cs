using UnityEngine;

public interface IAbilityUser
{
    Transform LightAnchorPivot { get; }
    LightAnchorProjectile AnchorPrefab { get; }
    LightReservoir LightReservoir { get; }
    PlatformerMovement Movement { get; }
}