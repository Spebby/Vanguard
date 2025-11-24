using System;
using Gilzoide.UpdateManager;
using UnityEngine;

public class Bullet : MonoBehaviour, IFixedUpdatable {
    float _speed;
    float _lifetime;
    int _damage;
    byte _colour;

    public void Init(float speed, float lifetime, int damage, byte colour = 0) {
        _speed = speed;
        _lifetime = lifetime;
        _damage = damage;
        _colour = colour;
    }

    void OnEnable() => this.RegisterInManager();
    void OnDisable() => this.UnregisterInManager();

    public void ManagedFixedUpdate() {
        transform.position += transform.up * (_speed * Time.fixedDeltaTime);
        _lifetime -= Time.fixedDeltaTime;
        if (_lifetime <= 0) {
            Destroy(gameObject);
        }
    }

    // New collision detection code
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
