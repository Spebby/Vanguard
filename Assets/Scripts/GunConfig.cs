using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/Gun Config")]
public class GunConfig : ScriptableObject {
    [Header("Gun Settings")]
    public Bullet bulletPrefab;
    [Min(0f), Tooltip("Rounds/s")] public float FiringSpeed = 5f;
    [Min(1f), Tooltip("Arbitrary number")] public float TurnSpeed = 35f;

    [Header("Bullet Config")]
    public float Speed = 2f;
    [Min(0)] public float Lifetime = 5f;
    [Min(0)] public int Damage = 1;
}