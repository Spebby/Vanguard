using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using PrimeTween; // using tweens for physics movement is the dumbest shit on earth but IDGAF!


public class EnemySpawner : MonoBehaviour {
    [SerializeField] internal EnemySpawnerConfig config;
    [SerializeField] internal EnemySpawnCollection collection;

    bool[] _blockedLanes;
    Transform[] _laneHeads;
    GameObject[] _laneGapObjects;

    float _screenTop;
    const float SPAWN_HEIGHT_OFFSET = 2f;
    float SpawnPosition => _screenTop + SPAWN_HEIGHT_OFFSET;
    static float _startTime;

    /** SOA (Structure of Arrays)
     *  It can be more efficient to store the data of objects similar to the columns of a database, where one column
     *  represents data for every ID. The data of a given object can be thought of as the cumulative membership of
     *  the entries in each column. So, the index of each array represents a variable for a given entity.
     *  SOA is key to Data-Oriented-Design, and Entity-Component Systems, but is generally a good optimisation step
     *  (this is not the reason I did this) as it improves cache efficiency by making better use of cache lines.
     * 
     *  Here, lane membership is dictated by ID. I didn't bother to extract all the per-object data out of Enemy Controller,
     *  it would be most proper to do that here, but I feel it's acceptable not to in the case.
     *  Querying lane membership is a lot quicker this way than iterating through each object individually and checking
     *  a value.
     */
    const int INIT_SIZE = 64;
    int[] _enemy2Lane = new int[INIT_SIZE];
    EnemyController[] _enemies = new EnemyController[INIT_SIZE];
    int _capacity = INIT_SIZE;
    int _top = 0;
    
    BossController _boss;

    void Awake() {
        _startTime = Time.time;


        // Assemble lane guides & spaces at runtime, so we can add more-or-less lanes easily.
        _screenTop = Camera.main!.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
        float totalWidth = config.Lanes * config.LaneWidth;
        float startX     = -totalWidth / 2;

        _blockedLanes = new bool[config.Lanes];
        _laneHeads = new Transform[config.Lanes];
        for (int i = 0; i < config.Lanes; i++) {
            float x = startX + i * config.LaneWidth;
            GameObject lane = new($"Lane_{i}") {
                transform = {
                    parent   = transform,
                    position = new Vector3(x + config.LaneWidth / 2, SpawnPosition, 0)
                }
            };
            _laneHeads[i] = lane.transform;
        }

        Vector3 visGapScaling = new(config.LaneGuide.transform.localScale.x, _screenTop * 2,
                                    config.LaneGuide.transform.localScale.z);
        _laneGapObjects = new GameObject[config.Lanes + 1];
        for (int i = 0; i < config.Lanes + 1; i++) {
            GameObject vis = Instantiate(config.LaneGuide, transform);
            vis.name                 = $"Vis_{i}";
            vis.transform.position   = new Vector3(startX + i * config.LaneWidth, 0, 0);
            vis.transform.localScale = visGapScaling;
            _laneGapObjects[i]       = vis;
        }

        // Add a slight gap in spawn time so boss doesn't spawn immediately.
        _lastBossSpawnTime = config.BossSpawnInitDelay;
    }
    
    static float GameTime => Time.time - _startTime;

    static float CurrentSpawnInterval(float spawnInterval,
                                      float difficultyIncreaseRate,
                                      float minSpawnInterval = 0.5f) {
        float scaledInterval = spawnInterval - GameTime * difficultyIncreaseRate;
        return Mathf.Max(scaledInterval, minSpawnInterval);
    }

    float _lastSpawnTime;
    float _lastBossSpawnTime;
    void FixedUpdate() {
        UpdateEnemies(_enemies, _enemy2Lane);
        
        float currentInterval = CurrentSpawnInterval(config.SpawnInterval,
                                                     config.DifficultyIncreaseRate,
                                                     config.MinSpawnInterval);
        
        if (!(Time.time - _lastSpawnTime >= currentInterval)) return;
        SpawnEnemies();
        _lastSpawnTime = Time.time;
        
        if (_boss || !(Time.time - _lastBossSpawnTime >= config.BossSpawnDelay)) return;
        SpawnBoss();
    }

    void UpdateEnemies(EnemyController[] enemies, int[] enemy2Lane) {
        // mass enemies
        Span<int> dead    = stackalloc int[_top];
        int       deadTop = 0;
        
        for (int i = 0; i < _top; i++) {
            EnemyController enemy = enemies[i];
            enemy.Rb.linearVelocity = enemy.MoveDirection.normalized * enemy.CurrentMoveSpeed;
            
            if (enemy.transform.position.y < -(_screenTop * 2f)) {
                dead[deadTop++] = i;
            }
        }

        // destroy in bulk
        foreach (int i in dead[..deadTop]) {
            Unsubscribe(i);
        }
        
        // Handle boss movement
        Array.Clear(_blockedLanes, 0, _blockedLanes.Length);
        if (!_boss) return;
        
        Bounds bounds = _boss.GetComponent<BoxCollider2D>().bounds;
        float bossLeft  =  bounds.max.x;
        float bossRight = -bounds.min.x;
        
        // Update blockedLanes based on boss position
        for (int lane = 0; lane < config.Lanes; lane++) {
            float laneCenter = _laneHeads[lane].position.x;
            float halfWidth  = config.LaneWidth * 0.5f;
            float laneLeft   = laneCenter - halfWidth;
            float laneRight  = laneCenter + halfWidth;

            // Axis overlap test
            bool leftInRange    = laneLeft  <= bossLeft  && bossLeft  <= laneRight;
            bool rightInRange   = laneRight <= bossRight && bossRight <= laneLeft;
            _blockedLanes[lane] = leftInRange || rightInRange;
        }
    }

    void SpawnEnemies() {
        // Find available lanes
        Span<int> laneCounts     = stackalloc int[config.Lanes];
        List<int> availableLanes = new();
        for (int i = 0; i < _top; i++) {
            int lane = _enemy2Lane[i];
            laneCounts[lane]++;
        }
        
        for (int lane = 0; lane < config.Lanes; lane++) {
            if (laneCounts[lane] < config.MaxEnemiesPerLane && !_blockedLanes[lane]) availableLanes.Add(lane);
        }

        if (availableLanes.Count == 0) return;
        List<int> lanesToSpawn = GetSpawnPattern(availableLanes, config);

        // resize membership arrays if we're under-allocated
        if (_capacity <= _top + lanesToSpawn.Count) {
            _capacity <<= 1;
            int[] temp1 = new int[_capacity];
            EnemyController[] temp2 = new EnemyController[_capacity];
            
            Array.Copy(_enemy2Lane, temp1, _top);
            Array.Copy(_enemies, temp2, _top);

            _enemy2Lane = temp1;
            _enemies = temp2;
        }
        
        bool isMultiSpawn = lanesToSpawn.Count > 1;
        foreach (int laneIndex in lanesToSpawn) {
            float zOffset = GetPositionVariation(isMultiSpawn);
            SpawnEnemyInLane(laneIndex, zOffset);
        }
    }

    void SpawnBoss() {
        _lastBossSpawnTime = GameTime + config.BossSpawnDelay;
        if (Random.value > config.BossSpawnChance) return;
        
        int            lane     = Random.Range(0, config.Lanes);
        BossController boss     = collection.Bosses[Random.Range(0, collection.Bosses.Length)];
        Vector3        spawnPos = _laneHeads[lane].position;
        spawnPos.y += GetPositionVariation();
        
        _boss = Instantiate(boss, spawnPos, Quaternion.identity);
        _boss.Initialise();
        
        _boss.CurrentMoveSpeed = Mathf.Min(_boss.BaseMoveSpeed + _boss.SpeedIncreasePerSecond * GameTime, _boss.MaxMoveSpeed);
        _boss.OnDeath += () => {
            _lastBossSpawnTime = GameTime + config.BossSpawnDelay;
        }; // pad so next boss doesn't try to spawn immediately
        
        // Animate into frame.
        // This is a bad way to do this, but acceptable for prototype
        Sequence.Create()
                .Chain(Tween.PositionY(_boss.transform, _screenTop - _screenTop * 0.2f, 4f, Ease.OutSine))
                .ChainCallback(() => _boss.GetComponentInChildren<Gun>().StartShooting());
    }
    
    float GetPositionVariation(bool isMultiSpawn = true) =>
        config.OffsetMultiSpawnsOnly && !isMultiSpawn
            ? 0f
            : Random.Range(config.MinPositionOffset,
                           config.MaxPositionOffset);
    
    void SpawnEnemyInLane(int laneIndex, float yOffset = 0f) {
        EnemyController enemyPrefab = collection.Enemies[Random.Range(0, collection.Enemies.Length)];
        Vector3         spawnPos    = _laneHeads[laneIndex].position;
        spawnPos.y += yOffset;

        int index = _top;   // capture stable index
        EnemyController e = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        e.Initialise(); // Execution order is important, must go before others.
       
        e.CurrentMoveSpeed = Mathf.Min(e.BaseMoveSpeed + e.SpeedIncreasePerSecond * GameTime, e.MaxMoveSpeed);
        e.OnDeath += () => Unsubscribe(index); // for when enemies are destroyed by bullets
        _enemies[index] = e;
        _enemy2Lane[index] = laneIndex;
        
        _top++;
    }

    // Remmove standard enemies from the enemy list. Used as a delegate.
    void Unsubscribe(int i) {
        // Source to remove
        EnemyController toRemove = _enemies[i];
        toRemove.OnDeath = null;

        _top--;
        if (i != _top) {
            // swapback
            _enemies[i]    = _enemies[_top];
            _enemy2Lane[i] = _enemy2Lane[_top];

            // Rebind the death callback to the new index
            int newIndex = i;
            _enemies[i].OnDeath = null;
            _enemies[i].OnDeath += () => Unsubscribe(newIndex);
        }

        Destroy(toRemove.gameObject);
    }

    #region Pattern Selection
    static List<int> GetSpawnPattern(List<int> availableLanes, EnemySpawnerConfig config) {
        List<int> selectedLanes = new();

        // If pattern system disabled or not enough lanes, fall back to random
        if (!config.UsePatterns || availableLanes.Count < 2) {
            RandomMultiPattern(availableLanes, selectedLanes, config);
            return selectedLanes;
        }

        int wall   = config.WallPatternWeight;
        int zigzag = config.ZigZagPatternWeight;
        int edges  = config.EdgePatternWeight;

        int maxWeight = wall + zigzag + edges;
        int roll = Random.Range(0, maxWeight);

        if (roll < wall) {
            WallPattern(availableLanes, selectedLanes, config);
        } else if (roll - wall < zigzag) {
            ZigZagPattern(availableLanes, selectedLanes, config);
        } else if (roll - wall - zigzag < edges) {
            EdgesPattern(availableLanes, selectedLanes, config);
        } else {
            RandomMultiPattern(availableLanes, selectedLanes, config);
        }

        return selectedLanes;
    }
    
    static void RandomMultiPattern(List<int> availableLanes, List<int> selectedLanes, EnemySpawnerConfig config) {
        int spawnCount = Random.value < config.MultiLaneSpawnChance
            ? Random.Range(config.MinSimultaneousSpawns, config.MaxSimultaneousSpawns + 1)
            : 1;
        spawnCount = Mathf.Min(spawnCount, availableLanes.Count);

        for (int i = 0; i < spawnCount; i++) {
            int randomIndex = Random.Range(0, availableLanes.Count);
            selectedLanes.Add(availableLanes[randomIndex]);
            availableLanes.RemoveAt(randomIndex);
        }
    }
    static void EdgesPattern(List<int> availableLanes, List<int> selectedLanes, EnemySpawnerConfig config) {
        if (availableLanes.Contains(0)) selectedLanes.Add(0);
        if (availableLanes.Contains(config.Lanes - 1)) selectedLanes.Add(config.Lanes - 1);
    }
    static void ZigZagPattern(List<int> availableLanes, List<int> selectedLanes, EnemySpawnerConfig config) {
        for (int i = 0; i < config.Lanes; i += 2) {
            if (!availableLanes.Contains(i)) continue;
            selectedLanes.Add(i);
        }
    }
    static void WallPattern(List<int> availableLanes, List<int> selectedLanes, EnemySpawnerConfig config) {
        int wallLength = Random.Range(2, config.Lanes - 1);
        int startLane  = Random.Range(0, Mathf.Max(1, 6 - wallLength));

        for (int i = 0; i < wallLength && i < availableLanes.Count; i++) {
            int lane = startLane + i;
            if (!availableLanes.Contains(lane)) continue;
            selectedLanes.Add(lane);
        }
    }
    #endregion
}