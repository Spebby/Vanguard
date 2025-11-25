using Gilzoide.UpdateManager;
using UnityEngine;


public class PlayerHealth : MonoBehaviour, IHealth, IFixedUpdatable {
    [SerializeField] int startingHealth;
    public int Health { get; protected set; }
    public byte Colour { get; protected set; } = 0;
    
    void Start() {
        Health = startingHealth;
    }
    
    [SerializeField] float invulnerabilityTime = 0.5f;
    
    float _iTime;
    
    void OnEnable() => this.RegisterInManager();
    void OnDisable() => this.UnregisterInManager();
    
    
    public void Heal(int amount) => Health += amount;
    public void Damage(int amount, byte colour) {
        if (Colour == colour || 0 < _iTime) return;
        
        Health -= amount;
        _iTime =  invulnerabilityTime;

        if (Health <= 0) Die();
    }

    public void Die() {
        Destroy(gameObject);
    }

    public void ManagedFixedUpdate() {
        _iTime -= Time.fixedDeltaTime;
    }
}