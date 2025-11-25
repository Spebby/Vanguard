using UnityEngine;


[CreateAssetMenu(fileName = "Enemy Spawner Config", menuName = "Scriptable Objects/Enemy Spawner Config")]
public class EnemySpawnerConfig : ScriptableObject {
    [Header("Lane Settings")]
    public int Lanes = 5;
    public float LaneWidth = 2f;
    
    [Header("Spawn Config")]
    public float SpawnInterval = 2f;
    public int MaxEnemiesPerLane = 3;
    
    [Header("Multi-Lane Spawning")] 
    [Range(1, 5)] public int MinSimultaneousSpawns = 1;
    [Range(1, 5)] public int MaxSimultaneousSpawns = 3;
    [Tooltip("Chance to spawn enemies in multiple lanes at once (0-1)"), Range(0f, 1f)] public float MultiLaneSpawnChance = 0.3f;

    [Header("Patterns")]
    public bool UsePatterns => WallPatternWeight > 0 || ZigZagPatternWeight > 0 || EdgePatternWeight > 0;
    [Tooltip("Spawn enemies in adjacent lanes (wall pattern)")] public int WallPatternWeight = 20;
    [Tooltip("Spawn enemies in alternating lanes (zigzag pattern)")] public int ZigZagPatternWeight = 15;
    [Tooltip("Spawn enemies only on outer lanes (edges pattern)")] public int EdgePatternWeight = 10;

    [Header("Difficulty Scaling")] 
    public float DifficultyIncreaseRate = 0.05f;
    public float MinSpawnInterval = 0.5f;
    
    [Header("Position Variation")]
    [Tooltip("Random forward/backward offset range for natural traffic spacing")] public float MinPositionOffset = -2f;
    public float MaxPositionOffset = 2f;

    [Tooltip("Apply offset only when spawning multiple enemies simultaneously")]
    public bool OffsetMultiSpawnsOnly = true;
    
    [Header("Misc")]
    public GameObject LaneGuide;
}