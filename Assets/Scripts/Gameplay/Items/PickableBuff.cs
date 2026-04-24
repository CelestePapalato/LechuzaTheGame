using UnityEngine;

public class PickableBuff : MonoBehaviour
{
    [SerializeField]
    private bool regenerateLight = false;
    [SerializeField]
    [Range(0,5)]
    private int healAmount = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(healAmount > 0)
        {
            TryHeal(collision);
        }
        if(regenerateLight)
        {
            TryRegenerateLight(collision);
        }
    }

    private void TryHeal(Collider2D collision)
    {
        Health health = collision.attachedRigidbody.GetComponentInChildren<Health>();
        if (health != null)
        {
            health.Heal(healAmount);
            Destroy(gameObject);
        }
    }

    private void TryRegenerateLight(Collider2D collision)
    {
        LightReservoir lightReservoir = collision.attachedRigidbody.GetComponentInChildren<LightReservoir>();
        if (lightReservoir != null)
        {
            lightReservoir.Regenerate();
            Destroy(gameObject);
        }
    }
}
