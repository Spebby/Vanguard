using System;
using UnityEngine;


public class EnemyController : MonoBehaviour {
    [Header("Movement Settings")] [SerializeField]
    internal float BaseMoveSpeed = 3f;
    [SerializeField] internal float SpeedIncreasePerSecond = 0.1f;
    [SerializeField] internal float MaxMoveSpeed = 15f;
    public Vector3 MoveDirection = Vector3.down;

    const byte Colour = 1;
    
    [Header("Boundaries")]
    public float CurrentMoveSpeed { get; internal set; }
    
    internal Rigidbody2D Rb;
    bool _isDestroyed;
    
    internal Action OnDeath;
    
    void OnDestroy() {
        // do any effects here
        OnDeath?.Invoke();
    }

    internal void Initialise() {
        CurrentMoveSpeed = BaseMoveSpeed;
        Rb = GetComponent<Rigidbody2D>();

        if (MoveDirection == Vector3.zero) return;
        float angle = Mathf.Atan2(MoveDirection.y, MoveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D other) {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        health?.Damage(1, 1);
        if (health) Destroy(gameObject); 
    }
}