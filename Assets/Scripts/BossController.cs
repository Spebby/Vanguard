using System;
using UnityEngine;


public class BossController : MonoBehaviour {
    [Header("Movement Settings")] [SerializeField]
    internal float BaseMoveSpeed = 1f;
    [SerializeField] internal float SpeedIncreasePerSecond = 0.1f;
    [SerializeField] internal float MaxMoveSpeed = 15f;
    public Vector3 MoveDirection = Vector3.down;
    
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
        Rb               = GetComponent<Rigidbody2D>();

        if (MoveDirection == Vector3.zero) return;
        float angle = Mathf.Atan2(MoveDirection.y, MoveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }
}