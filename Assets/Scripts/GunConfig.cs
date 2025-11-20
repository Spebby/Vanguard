using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/Gun Config")]
public class GunConfig : ScriptableObject {
    [Min(0f)] public float FiringSpeed = 5f;
    [Min(1f)] public float TurnSpeed = 5f;
}