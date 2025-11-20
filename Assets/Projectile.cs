using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float speed = 10f;
    [SerializeField] private Vector3 direction = Vector3.up;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool piercing = false;
    [SerializeField] private int maxPierceTargets = 3;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private TrailRenderer trail;
    
    private int pierceCount = 0;
    private Rigidbody2D rb2D;
    
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        
        // Set velocity
        if (rb2D != null)
        {
            rb2D.linearVelocity = direction.normalized * speed;
        }
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
        
        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }
    
    void Update()
    {
        if (rb2D == null)
        {
            // Move using transform if no Rigidbody2D
            transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
        }
    }
    
    public int GetDamage()
    {
        return damage;
    }
    
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rb2D != null)
        {
            rb2D.linearVelocity = direction.normalized * speed;
        }
    }
    
    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection;
        if (rb2D != null)
        {
            rb2D.linearVelocity = direction.normalized * speed;
        }
        
        // Update rotation
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }
    
    private void HandleHit(GameObject hitObject)
    {
        // Check if we hit an enemy
        if (hitObject.CompareTag("Enemy"))
        {
            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Play hit sound
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }
            
            // Handle piercing
            if (piercing)
            {
                pierceCount++;
                if (pierceCount >= maxPierceTargets)
                {
                    DestroyProjectile();
                }
            }
            else
            {
                DestroyProjectile();
            }
        }
        // Hit wall or other obstacle
        else if (hitObject.CompareTag("Wall") || hitObject.CompareTag("Obstacle"))
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            DestroyProjectile();
        }
    }
    
    private void DestroyProjectile()
    {
        // Disable trail renderer to prevent visual artifacts
        if (trail != null)
        {
            trail.enabled = false;
        }
        
        Destroy(gameObject);
    }
}