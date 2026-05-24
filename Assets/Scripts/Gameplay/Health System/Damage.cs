using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField]
    protected int damageAmount = 10;
    
    protected int multiplier = 1;
    protected Collider2D damageCollider;

    public int DamageAmount => damageAmount * multiplier;

    public int Multiplier
    {
        get => multiplier;
        set => multiplier = Mathf.Max(1, value);
    }

    protected virtual void Awake()
    {
        damageCollider = GetComponent<Collider2D>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.attachedRigidbody.GetComponentInChildren<Health>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
        }
    }

    public virtual void EnableDamage(bool enable)
    {
        damageCollider.enabled = enable;
    }
}
