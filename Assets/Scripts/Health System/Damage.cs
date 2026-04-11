using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField]
    private int damageAmount = 10;
    
    private int multiplier = 1;
    private Collider damageCollider;

    public int DamageAmount => damageAmount * multiplier;

    public int Multiplier
    {
        get => multiplier;
        set => multiplier = Mathf.Max(1, value);
    }

    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponent<Health>();
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
