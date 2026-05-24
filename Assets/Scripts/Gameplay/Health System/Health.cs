using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 100;
    [SerializeField]
    private float invincibilityTime = 0.8f;

    private int currentHealth;
    private float invincibilityTimer = 0f;
    private Coroutine invincibilityCoroutine;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public bool IsInvincible { get; private set; } = false;

    public UnityAction<int, int> OnHealthChanged;
    public UnityAction<int, int> OnDamage;
    public UnityAction<int, int> OnHeal;
    public UnityAction OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnDisable()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }
    }
    public void TakeDamage(int damage)
    {
        if (IsInvincible || currentHealth <= 0) return;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamage?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            return;
        }
        invincibilityCoroutine = StartCoroutine(InvincibilityTimer());
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHeal?.Invoke(currentHealth, maxHealth);
    }

    private IEnumerator InvincibilityTimer()
    {
        invincibilityTimer = invincibilityTime;
        IsInvincible = true;
        while (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
            yield return null;
        }
        IsInvincible = false;
    }
}
