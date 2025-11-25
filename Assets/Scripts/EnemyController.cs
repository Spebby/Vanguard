using System;
using System.Runtime.CompilerServices;
using Gilzoide.UpdateManager;
using UnityEngine;
using UnityEngine.Serialization;


public class EnemyController : MonoBehaviour {
    [Header("Movement Settings")]
    [SerializeField]
    internal float BaseMoveSpeed = 3f;                             // Starting speed (slower)
    [SerializeField] internal float SpeedIncreasePerSecond = 0.1f; // Speed increase over time
    [FormerlySerializedAs("maxMoveSpeed")] [SerializeField] internal float MaxMoveSpeed = 15f;        // Maximum speed cap
    public Vector3 MoveDirection = Vector3.down;
    [SerializeField] bool rotateTowardsMovement;
    
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

        if (!rotateTowardsMovement || MoveDirection == Vector3.zero) return;
        float angle = Mathf.Atan2(MoveDirection.y, MoveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }

    public void SetMoveDirection(Vector3 newDirection) {
        MoveDirection = newDirection;

        if (!rotateTowardsMovement || MoveDirection == Vector3.zero) return;
        float angle = Mathf.Atan2(MoveDirection.y, MoveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }
}