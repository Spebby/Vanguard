using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 3f;  // Starting speed (slower)
    [SerializeField] private float speedIncreasePerSecond = 0.1f;  // Speed increase over time
    [SerializeField] private float maxMoveSpeed = 15f;  // Maximum speed cap
    [SerializeField] private Vector3 moveDirection = Vector3.down;
    [SerializeField] private bool rotateTowardsMovement = false;
    
    private float currentMoveSpeed;
    
    [Header("Health & Combat")]
    [SerializeField] private int health = 1;
    [SerializeField] private int pointValue = 10;
    [SerializeField] private float destroyBelowY = -10f; // Auto-destroy when off screen
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private bool flashOnHit = true;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    
    // Private variables
    private int currentHealth;
    private int assignedLane = -1;
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDestroyed = false;
    private EnemySpawner spawner;
    private float spawnTime;
    
    // Events
    public System.Action OnEnemyDestroyed;
    public System.Action<EnemyController, int> OnEnemyKilled; // Enemy, Points
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
        currentHealth = health;
        currentMoveSpeed = baseMoveSpeed;
        spawnTime = Time.time;
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
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
    
    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;
        
        currentHealth -= damage;
        
        if (flashOnHit && spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private System.Collections.IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;
        
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
    
    public void Die()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        
        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        
        // Fire events
        OnEnemyKilled?.Invoke(this, pointValue);
        OnEnemyDestroyed?.Invoke();
        
        // Destroy game object
        Destroy(gameObject);
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
    
    public int GetHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return health;
    }
    
    public int GetPointValue()
    {
        return pointValue;
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
    
    // For projectile collisions
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerProjectile"))
        {
            Projectile projectile = other.GetComponent<Projectile>();
            int damage = projectile != null ? projectile.GetDamage() : 1;
            
            TakeDamage(damage);
            
            // Destroy projectile
            Destroy(other.gameObject);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerProjectile"))
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            int damage = projectile != null ? projectile.GetDamage() : 1;
            
            TakeDamage(damage);
            
            // Destroy projectile
            Destroy(collision.gameObject);
        }
    }
}