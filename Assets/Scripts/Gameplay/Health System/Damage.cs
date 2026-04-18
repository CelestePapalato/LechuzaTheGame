using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField]
    private int damageAmount = 10;
    
    private int multiplier = 1;
    private Collider2D damageCollider;

    public int DamageAmount => damageAmount * multiplier;

    public int Multiplier
    {
        get => multiplier;
        set => multiplier = Mathf.Max(1, value);
    }

    private void Awake()
    {
        damageCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.attachedRigidbody.GetComponentInChildren<Health>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
        }
    }

    public void EnableDamage(bool enable)
    {
        damageCollider.enabled = enable;
    }
}
