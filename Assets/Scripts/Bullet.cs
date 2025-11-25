using Gilzoide.UpdateManager;
using UnityEngine;


public class Bullet : MonoBehaviour, IFixedUpdatable {
    float _speed;
    float _lifetime;
    int _damage;
    int _pierce;
    byte _colour;
    
    public void Init(float speed, float lifetime, int damage, int pierce, byte colour = 0) {
        _speed = speed;
        _lifetime = lifetime;
        _damage = damage;
        _pierce = pierce;
        _colour = colour;
    }

    void OnEnable() => this.RegisterInManager();
    void OnDisable() => this.UnregisterInManager();

    public void ManagedFixedUpdate() {
        transform.position += transform.up * (_speed * Time.fixedDeltaTime);
        _lifetime -= Time.fixedDeltaTime;
        
        if (!(_lifetime <= 0)) return;
        Destroy(gameObject);
    }

    // damage mfs
    void OnTriggerEnter2D(Collider2D other) {
        IHealth health = other.GetComponent<IHealth>();
        health?.Damage(_damage, _colour);
        if (health?.Colour != _colour && --_pierce <= 0) Destroy(gameObject);
    }
}
