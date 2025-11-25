interface IHealth {
    public int Health { get; }
    public byte Colour { get; }
    
    void Heal(int amount);
    void Damage(int amount, byte colour);
    void Die();
}