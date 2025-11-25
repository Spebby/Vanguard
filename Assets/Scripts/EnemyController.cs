using Gilzoide.UpdateManager;
using UnityEngine;


public class EnemyController : MonoBehaviour, IFixedUpdatable {
    [Header("Movement Settings")]
    [SerializeField]
    float baseMoveSpeed = 3f;                             // Starting speed (slower)
    [SerializeField] float speedIncreasePerSecond = 0.1f; // Speed increase over time
    [SerializeField] float maxMoveSpeed = 15f;            // Maximum speed cap
    [SerializeField] Vector3 moveDirection = Vector3.down;
    [SerializeField] bool rotateTowardsMovement = false;
    
    [Header("Boundaries")]
    [SerializeField]
    float destroyBelowY = -10f; // Auto-destroy when off screen

    public float CurrentMoveSpeed { get; private set; }
    
    // Private variables
    public int Lane { get; set; } = -1;
    Rigidbody2D _rb;
    bool _isDestroyed;
    EnemySpawner _spawner;
    float _spawnTime;
    
    // Events
    public System.Action OnEnemyDestroyed;
    public System.Action<EnemyController> OnEnemyReachedEnd;

    void OnEnable() => this.RegisterInManager();
    void OnDisable() => this.UnregisterInManager();
    
    void Start() {
        Initialize();
    }
    
    public void ManagedFixedUpdate() {
        UpdateSpeed();
        Move();
        CheckBounds();
    }

    
    void UpdateSpeed() {
        // Get game time from spawner if available, otherwise use local time
        float gameTime = _spawner ? _spawner.GetGameTime() : Time.time - _spawnTime;
        
        // Increase speed based on game time
        CurrentMoveSpeed = baseMoveSpeed + (gameTime * speedIncreasePerSecond);
        CurrentMoveSpeed = Mathf.Min(CurrentMoveSpeed, maxMoveSpeed);
    }

    void Initialize() {
        CurrentMoveSpeed = baseMoveSpeed;
        _spawnTime = Time.time;
        _rb = GetComponent<Rigidbody2D>();

        if (!rotateTowardsMovement || moveDirection == Vector3.zero) return;
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }

    void Move() {
        if (_rb) {
            _rb.linearVelocity = moveDirection.normalized * CurrentMoveSpeed;
        } else {
            // Use Transform for simple movement
            transform.Translate(moveDirection.normalized * (CurrentMoveSpeed * Time.deltaTime), Space.World);
        }
    }

    void CheckBounds() {
        if (transform.position.y < destroyBelowY) {
            ReachedEnd();
        }
    }
    
    public void ReachedEnd() {
        if (_isDestroyed) return;
        _isDestroyed = true;
        
        OnEnemyReachedEnd?.Invoke(this);
        OnEnemyDestroyed?.Invoke();
        
        Destroy(gameObject);
    }
    
    // Called by spawner to assign lane
    public void SetLane(int laneIndex) {
        Lane = laneIndex;
    }
    
    public void SetMoveSpeed(float newSpeed) {
        baseMoveSpeed = newSpeed;
        CurrentMoveSpeed = newSpeed;
    }
    
    public void SetSpawner(EnemySpawner enemySpawner) {
        _spawner = enemySpawner;
    }
    
    public float GetCurrentSpeed() {
        return CurrentMoveSpeed;
    }
    
    public void SetMoveDirection(Vector3 newDirection) {
        moveDirection = newDirection;

        if (!rotateTowardsMovement || moveDirection == Vector3.zero) return;
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }
}