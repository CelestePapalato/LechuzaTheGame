using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousDamage : Damage
{
    [SerializeField]
    private float damageInterval = 1f;

    private readonly Dictionary<Rigidbody2D, Health> damagedTargets = new();
    private Coroutine damageCoroutine;
    private WaitForSeconds wait;

    protected override void Awake()
    {
        base.Awake();
        wait = new WaitForSeconds(damageInterval);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null || damagedTargets.ContainsKey(rb))
            return;

        Health health = rb.GetComponentInChildren<Health>();
        if (health == null)
            return;

        damagedTargets[rb] = health;
        health.TakeDamage(damageAmount);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null)
            damagedTargets.Remove(rb);
    }

    private void OnEnable()
    {
        damageCoroutine = StartCoroutine(ApplyDamageOverTime());
    }

    private void OnDisable()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
        damagedTargets.Clear();
    }

    private IEnumerator ApplyDamageOverTime()
    {
        while (true)
        {
            yield return wait;
            foreach (Health health in damagedTargets.Values)
                health.TakeDamage(damageAmount);
        }
    }

    public override void EnableDamage(bool enable)
    {
        base.EnableDamage(enable);
        if (!enable)
            damagedTargets.Clear();
    }
}
