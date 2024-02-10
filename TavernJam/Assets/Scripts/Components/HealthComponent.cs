using UnityEngine;

public delegate void OnDeathDelegate(GameObject dead);

public class HealthComponent : MonoBehaviour
{
    [Min(0.0f)]
    [SerializeField]
    private float CurrentHealth;
    [Min(1.0f)]
    [SerializeField]
    private float MaxHealth;

    public OnDeathDelegate OnDeath;
    public BaseStatsContainer baseStats;

    private void Awake()
    {
        if (!baseStats)
        {
            Debug.LogError("No Base Stats attached to HealthComponent");
            return;
        }

        MaxHealth = baseStats.MaxHealth;
        CurrentHealth = baseStats.MaxHealth;
    }

    public void SetCurrentHealth(float CurrentHealth_)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth_, 0.0f, MaxHealth);
        if (CurrentHealth <= 0.0f)
        {
            OnDeath?.Invoke(gameObject);
        }
    }

    public void ReceiveDamage(float damage)
    {
        SetCurrentHealth(CurrentHealth - damage);
    }

    public float GetCurrentHealth()
    {
        return CurrentHealth;
    }
}
