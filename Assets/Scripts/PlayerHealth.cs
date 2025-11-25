using Gilzoide.UpdateManager;
using UnityEngine;


public class PlayerHealth : MonoBehaviour, IHealth, IFixedUpdatable {
    [SerializeField] int startingHealth;
    public int Health { get; protected set; }

    void Start() {
        Health = startingHealth;
    }
    
    [SerializeField] float invulnerabilityTime = 0.5f;
    
    float _iTime;
    
    void OnEnable() => this.RegisterInManager();
    void OnDisable() => this.UnregisterInManager();
    
    
    public void Heal(int amount) => Health += amount;
    public void Damage(int amount) {
        if (_iTime < 0) return;
        Health += amount;
        _iTime =  invulnerabilityTime;

        if (Health <= 0) Die();
    }

    public void Die() {
        Destroy(this.gameObject);
    }

    public void ManagedFixedUpdate() {
        _iTime -= Time.fixedDeltaTime;
    }
}