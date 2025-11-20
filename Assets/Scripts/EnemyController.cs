using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 3f;  // Starting speed (slower)
    [SerializeField] private float speedIncreasePerSecond = 0.1f;  // Speed increase over time
    [SerializeField] private float maxMoveSpeed = 15f;  // Maximum speed cap
    [SerializeField] private Vector3 moveDirection = Vector3.down;
    [SerializeField] private bool rotateTowardsMovement = false;
    
    [Header("Boundaries")]
    [SerializeField] private float destroyBelowY = -10f; // Auto-destroy when off screen
    
    private float currentMoveSpeed;
    
    // Private variables
    private int assignedLane = -1;
    private Rigidbody2D rb2D;
    private bool isDestroyed = false;
    private EnemySpawner spawner;
    private float spawnTime;
    
    // Events
    public System.Action OnEnemyDestroyed;
    public System.Action<EnemyController> OnEnemyReachedEnd;
    
    void Start()
    {
        Initialize();
    }
    
    void Update()
    {
        UpdateSpeed();
        Move();
        CheckBounds();
    }
    
    private void UpdateSpeed()
    {
        // Get game time from spawner if available, otherwise use local time
        float gameTime = spawner != null ? spawner.GetGameTime() : (Time.time - spawnTime);
        
        // Increase speed based on game time
        currentMoveSpeed = baseMoveSpeed + (gameTime * speedIncreasePerSecond);
        currentMoveSpeed = Mathf.Min(currentMoveSpeed, maxMoveSpeed);
    }
    
    private void Initialize()
    {
        currentMoveSpeed = baseMoveSpeed;
        spawnTime = Time.time;
        rb2D = GetComponent<Rigidbody2D>();
        
        if (rotateTowardsMovement && moveDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }
    
    private void Move()
    {
        if (rb2D != null)
        {
            // Use Rigidbody2D for physics-based movement
            rb2D.linearVelocity = moveDirection.normalized * currentMoveSpeed;
        }
        else
        {
            // Use Transform for simple movement
            transform.Translate(moveDirection.normalized * currentMoveSpeed * Time.deltaTime, Space.World);
        }
    }
    
    private void CheckBounds()
    {
        if (transform.position.y < destroyBelowY)
        {
            ReachedEnd();
        }
    }
    
    public void ReachedEnd()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        
        OnEnemyReachedEnd?.Invoke(this);
        OnEnemyDestroyed?.Invoke();
        
        Destroy(gameObject);
    }
    
    // Called by spawner to assign lane
    public void SetLane(int laneIndex)
    {
        assignedLane = laneIndex;
    }
    
    public int GetLane()
    {
        return assignedLane;
    }
    
    public void SetMoveSpeed(float newSpeed)
    {
        baseMoveSpeed = newSpeed;
        currentMoveSpeed = newSpeed;
    }
    
    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }
    
    public float GetCurrentSpeed()
    {
        return currentMoveSpeed;
    }
    
    public void SetMoveDirection(Vector3 newDirection)
    {
        moveDirection = newDirection;
        
        if (rotateTowardsMovement && moveDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }
}