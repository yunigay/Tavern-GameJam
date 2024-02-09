using UnityEngine;

[CreateAssetMenu(fileName = "New Base Stats", menuName = "SO/Base Stats")]
public class BaseStatsContainer : ScriptableObject
{
    public float Damage;
    public float AttackSpeed;
    public float MaxHealth;
    public float CurrentHealth;
    public float Speed;
    public float dashSpeed;
    public float dashCooldown;
    public float jumpForce;

}

