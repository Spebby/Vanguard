interface IHealth {
    public int Health { get; }
    
    void Heal(int amount);
    void Damage(int amount);
    void Die();
}