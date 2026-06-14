using UnityEngine;
using UnityEngine.Events;

public class LightReservoir : MonoBehaviour
{
    [SerializeField]
    private int startingLight = 0;
    [SerializeField]
    private int maxLight = 5;
    [SerializeField]
    private float regenRate = 0f;
    [SerializeField]
    private bool useRegen = false;

    private int currentLight = 0;
    private float regenTimer = 0;

    public UnityAction<int, int> OnLightChanged;

    private void Awake()
    {
        currentLight = startingLight;
    }

    private void Start()
    {
        OnLightChanged?.Invoke(currentLight, maxLight);
    }

    private void Update()
    {
        if (useRegen)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenRate)
            {
                Regenerate();
                regenTimer = 0;
            }
        }
    }

    public void Consume(int amount)
    {
        currentLight = Mathf.Max(currentLight - amount, 0);
        OnLightChanged?.Invoke(currentLight, maxLight);
    }

    public void Regenerate()
    {
        currentLight = Mathf.Min(currentLight + 1, maxLight);
        OnLightChanged?.Invoke(currentLight, maxLight);
    }

    public bool HasLight(int amount)
    {
        return currentLight >= amount;
    }

    public bool TryConsume(int amount)
    {
        if (HasLight(amount))
        {
            Consume(amount);
            return true;
        }
        return false;
    }
}
