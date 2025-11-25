using UnityEngine;
using System.Collections.Generic;


public class EnemySpawner : MonoBehaviour {
    [Header("Lane Setup")]
    public Transform[] lanes = new Transform[5];
    public float laneSpacing = 2f;

    [Header("Enemy Setup")] public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")] public float spawnInterval = 2f;
    public float spawnHeight = 10f;
    public int maxEnemiesPerLane = 3;

    [Header("Multi-Lane Spawning")] [Range(1, 5)]
    public int minSimultaneousSpawns = 1;

    [Range(1, 5)] public int maxSimultaneousSpawns = 3;

    [Tooltip("Chance to spawn enemies in multiple lanes at once (0-1)")] [Range(0f, 1f)]
    public float multiLaneSpawnChance = 0.3f;

    [Header("Spawn Patterns")] public bool usePatterns = true;

    [Tooltip("Spawn enemies in adjacent lanes (wall pattern)")] [Range(0f, 1f)]
    public float wallPatternChance = 0.2f;

    [Tooltip("Spawn enemies in alternating lanes (zigzag pattern)")] [Range(0f, 1f)]
    public float zigzagPatternChance = 0.15f;

    [Tooltip("Spawn enemies only on outer lanes (edges pattern)")] [Range(0f, 1f)]
    public float edgesPatternChance = 0.1f;

    [Header("Difficulty Scaling")] public float difficultyIncreaseRate = 0.05f;
    public float minSpawnInterval = 0.5f;

    [Header("Position Variation")] [Tooltip("Random forward/backward offset range for natural traffic spacing")]
    public float minPositionOffset = -2f;

    public float maxPositionOffset = 2f;

    [Tooltip("Apply offset only when spawning multiple enemies simultaneously")]
    public bool offsetOnlyMultiSpawns = true;

    float _lastSpawnTime;
    List<GameObject>[] _laneEnemies;
    float _gameStartTime;
    bool _isMultiSpawn = false;

    void Start() {
        // Initialize lane tracking
        _laneEnemies = new List<GameObject>[5];
        for (int i = 0; i < 5; i++) {
            _laneEnemies[i] = new List<GameObject>();
        }

        // Auto-create lanes if not set
        if (!lanes[0]) {
            CreateLanes();
        }

        _gameStartTime = Time.time;
    }

    void Update() {
        float currentInterval = GetCurrentSpawnInterval();

        if (!(Time.time - _lastSpawnTime >= currentInterval)) return;
        SpawnEnemies();
        _lastSpawnTime = Time.time;
    }

    float GetCurrentSpawnInterval() {
        float timeSinceStart = Time.time - _gameStartTime;
        float scaledInterval = spawnInterval - timeSinceStart * difficultyIncreaseRate;
        return Mathf.Max(scaledInterval, minSpawnInterval);
    }

    public float GetGameTime() {
        return Time.time - _gameStartTime;
    }

    void CreateLanes() {
        for (int i = 0; i < 5; i++) {
            GameObject lane = new("Lane_" + i) {
                transform = {
                    parent   = transform,
                    position = new Vector3((i - 2) * laneSpacing, spawnHeight, 0)
                }
            };
            
            lanes[i] = lane.transform;
        }
    }

    void SpawnEnemies() {
        if (enemyPrefabs.Length == 0) return;

        // Find available lanes
        List<int> availableLanes = new();
        for (int i = 0; i < 5; i++) {
            CleanupDestroyedEnemies(i);
            if (_laneEnemies[i].Count >= maxEnemiesPerLane) continue;
            availableLanes.Add(i);
        }

        if (availableLanes.Count == 0) return;

        // Determine spawn pattern
        List<int> lanesToSpawn = GetSpawnPattern(availableLanes);

        // Track if this is a multi-spawn for position variation
        _isMultiSpawn = lanesToSpawn.Count > 1;

        // Spawn enemies in selected lanes with position variation
        foreach (int laneIndex in lanesToSpawn) {
            float zOffset = GetPositionVariation();
            SpawnEnemyInLane(laneIndex, zOffset);
        }
    }

    // Apply variation only if enabled and conditions are met
    float GetPositionVariation() => offsetOnlyMultiSpawns && !_isMultiSpawn ? 0f : Random.Range(minPositionOffset, maxPositionOffset);

    List<int> GetSpawnPattern(List<int> availableLanes) {
        List<int> selectedLanes = new();

        if (!usePatterns || availableLanes.Count < 2) {
            // Simple random spawn
            int spawnCount = Random.value < multiLaneSpawnChance
                ? Random.Range(minSimultaneousSpawns, maxSimultaneousSpawns + 1)
                : 1;
            spawnCount = Mathf.Min(spawnCount, availableLanes.Count);

            for (int i = 0; i < spawnCount; i++) {
                int randomIndex = Random.Range(0, availableLanes.Count);
                selectedLanes.Add(availableLanes[randomIndex]);
                availableLanes.RemoveAt(randomIndex);
            }

            return selectedLanes;
        }

        float patternRoll = Random.value;

        // Wall pattern - spawn in consecutive lanes
        if (patternRoll < wallPatternChance) {
            int wallLength = Random.Range(2, 4);
            int startLane  = Random.Range(0, Mathf.Max(1, 6 - wallLength));

            for (int i = 0; i < wallLength && i < availableLanes.Count; i++) {
                int lane = startLane + i;
                if (!availableLanes.Contains(lane)) continue;
                selectedLanes.Add(lane);
            }
        }
        // Zigzag pattern - spawn in alternating lanes
        else if (patternRoll < wallPatternChance + zigzagPatternChance) {
            for (int i = 0; i < 5; i += 2) {
                if (!availableLanes.Contains(i)) continue;
                selectedLanes.Add(i);
            }
        }
        // Edges pattern - spawn only on outer lanes
        else if (patternRoll < wallPatternChance + zigzagPatternChance + edgesPatternChance) {
            if (availableLanes.Contains(0)) selectedLanes.Add(0);
            if (availableLanes.Contains(4)) selectedLanes.Add(4);
        }
        // Default random multi-spawn
        else {
            int spawnCount = Random.value < multiLaneSpawnChance
                ? Random.Range(minSimultaneousSpawns, maxSimultaneousSpawns + 1)
                : 1;
            spawnCount = Mathf.Min(spawnCount, availableLanes.Count);

            for (int i = 0; i < spawnCount; i++) {
                int randomIndex = Random.Range(0, availableLanes.Count);
                selectedLanes.Add(availableLanes[randomIndex]);
                availableLanes.RemoveAt(randomIndex);
            }
        }

        // Fallback if no pattern selected
        if (selectedLanes.Count != 0) return selectedLanes;
        selectedLanes.Add(availableLanes[Random.Range(0, availableLanes.Count)]);

        return selectedLanes;
    }

    void SpawnEnemyInLane(int laneIndex, float yOffset = 0f) {
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector3    spawnPos    = lanes[laneIndex].position;

        // Apply position variation for natural traffic spacing (vertical/Y-axis)
        spawnPos.y += yOffset;

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Set lane and pass game time reference
        EnemyController controller = newEnemy.GetComponent<EnemyController>();
        if (controller) {
            controller.SetLane(laneIndex);
            controller.SetSpawner(this);
        }

        // Track enemy
        _laneEnemies[laneIndex].Add(newEnemy);
    }

    void CleanupDestroyedEnemies(int laneIndex) {
        for (int i = _laneEnemies[laneIndex].Count - 1; i >= 0; i--) {
            if (_laneEnemies[laneIndex][i]) continue;
            _laneEnemies[laneIndex].RemoveAt(i);
        }
    }
}