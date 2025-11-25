using UnityEngine;


[CreateAssetMenu(fileName = "Enemy Spawn Collection", menuName = "Scriptable Objects/Enemy Spawn Collection")]
public class EnemySpawnCollection : ScriptableObject {
    public EnemyController[] Enemies;
}