using UnityEngine;


public class EnemyHealth : MonoBehaviour, IHealth {
    [SerializeField] int startingHealth = 20;

    void Start() {
        Health = startingHealth;
    }
    
    public int Health { get; protected set; }
    public byte Colour { get; protected set; } = 1;
    
    public void Heal(int amount) => Health += amount;

    public void Damage(int amount, byte colour) {
        if (Colour == colour) return;
        Health -= amount;
        if (Health <= 0) Die();
    }

    public void Die() {
        Destroy(gameObject);
    }
}