using UnityEngine;


[CreateAssetMenu(fileName = "VehicleConfig", menuName = "Scriptable Objects/Vehicle Config")]
public class VehicleConfig : ScriptableObject {
    public float Speed = 2f;
    
    [Header("Dodges")]
    [Min(0)] public int Dodges;
    [Tooltip("Time the unit is invulnerable to damage.")] public float DodgeInvulnerability = 0.75f;
    [Tooltip("Time for a dodge to regenerate.")] public float DodgeRegen = 2f;
    [Tooltip("Time between consecutive dodges.")] public float DodgeCooldown = 0.5f;
    [Tooltip("Disabled shooting timer.")] public float DisableShootingTimer = 0f;
}