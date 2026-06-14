using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelSettings
    {
        public string levelName = "Wave";
        public float duration = 30f;
        public int dungeonRoomCount;
    }

    [Header("Levels")]
    [SerializeField] private List<LevelSettings> levels = new List<LevelSettings>();
    [SerializeField] private bool advanceLevelsByTimer = true;
    [SerializeField] private float timeBetweenLevels = 5f;
    [SerializeField] private bool startOnAwake = true;

    [Header("Dungeon")]
    [SerializeField] private DungeonGenerator2D dungeonGenerator;
    [SerializeField] private AbstractDungeonGenerator tilemapDungeonGenerator;
    [SerializeField] private bool generateDungeonOnLevelStart = true;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private bool spawnPlayerInStartRoom = true;
    [SerializeField] private WeaponManager.PlayerClass selectedPlayerClass = WeaponManager.PlayerClass.Katana;

    [Header("Enemies")]
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private bool spawnEnemiesOnLevelStart = true;

    [Header("Bonus Items")]
    [SerializeField] private BonusItemSpawner bonusItemSpawner;
    [SerializeField] private bool spawnBonusItemsOnLevelStart = true;

    [Header("Scene UI")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraFollowPlayer cameraFollowPlayer;

    private int currentLevelIndex;
    private float levelTimer;
    private bool levelRunning;
    private Coroutine levelRoutine;

    public int CurrentLevel => currentLevelIndex + 1;
    public float LevelTimeRemaining => Mathf.Max(0f, levelTimer);
    public bool IsLevelRunning => levelRunning;

    private void Awake()
    {
        if (levels.Count == 0)
            CreateDefaultLevels();

        ResolveDungeonGenerator();
        ResolveEnemyManager();
        ResolveBonusItemSpawner();
        ResolveSceneBindings();

        if (generateDungeonOnLevelStart && dungeonGenerator != null)
            dungeonGenerator.GenerateOnStart = false;
    }

    private void Start()
    {
        if (startOnAwake)
            StartRun();
    }

    private void Update()
    {
        if (!levelRunning)
            return;

        if (!advanceLevelsByTimer)
            return;

        LevelSettings level = levels[currentLevelIndex];
        levelTimer -= Time.deltaTime;

        if (levelTimer <= 0f)
            CompleteCurrentLevel();
    }

    public void StartRun()
    {
        currentLevelIndex = 0;
        StartCurrentLevel();
    }

    public void StartRun(WeaponManager.PlayerClass playerClass)
    {
        selectedPlayerClass = playerClass;
        StartRun();
    }

    public void SetPlayerClass(WeaponManager.PlayerClass playerClass)
    {
        selectedPlayerClass = playerClass;

        if (playerTransform != null)
            ApplySelectedWeapon(playerTransform);
    }

    public void RestartRun()
    {
        StopLevelRoutine();
        StartRun();
    }

    public void StartLevel(int levelNumber)
    {
        currentLevelIndex = Mathf.Clamp(levelNumber - 1, 0, levels.Count - 1);
        StartCurrentLevel();
    }

    public void ReportLevelCleared()
    {
        if (levelRunning)
            CompleteCurrentLevel();
    }

    private void StartCurrentLevel()
    {
        StopLevelRoutine();

        if (currentLevelIndex >= levels.Count)
        {
            Debug.Log("All levels complete.");
            levelRunning = false;
            return;
        }

        LevelSettings level = levels[currentLevelIndex];
        levelTimer = level.duration;
        levelRunning = true;

        Debug.Log($"Starting level {CurrentLevel}: {level.levelName}");
        GenerateDungeonForLevel(level);
        SpawnPlayerInStartRoom();
        SpawnBonusItemsForLevel(level);
        StartEnemiesForLevel(level);
    }

    private void CompleteCurrentLevel()
    {
        levelRunning = false;
        Debug.Log($"Level {CurrentLevel} complete.");
        StopEnemiesForLevel();
        ClearBonusItemsForLevel();

        currentLevelIndex++;
        levelRoutine = StartCoroutine(StartNextLevelAfterDelay());
    }

    private IEnumerator StartNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(timeBetweenLevels);
        StartCurrentLevel();
    }

    private void StartEnemiesForLevel(LevelSettings level)
    {
        if (!spawnEnemiesOnLevelStart)
            return;

        ResolveEnemyManager();

        if (enemyManager == null)
        {
            Debug.LogWarning("LevelManager could not create an EnemyManager for enemy spawning.");
            return;
        }

        if (tilemapDungeonGenerator == null || !tilemapDungeonGenerator.HasGeneratedDungeon)
        {
            Debug.LogWarning("LevelManager could not spawn enemies because no tilemap dungeon room list exists.");
            return;
        }

        enemyManager.SpawnEnemies(tilemapDungeonGenerator.Rooms, tilemapDungeonGenerator, playerTransform);
        HandleEnemyCountChanged(enemyManager.AliveEnemyCount);
    }

    private void SpawnBonusItemsForLevel(LevelSettings level)
    {
        if (!spawnBonusItemsOnLevelStart)
            return;

        ResolveBonusItemSpawner();

        if (bonusItemSpawner == null)
        {
            Debug.LogWarning("LevelManager could not create a BonusItemSpawner for item spawning.");
            return;
        }

        if (tilemapDungeonGenerator == null || !tilemapDungeonGenerator.HasGeneratedDungeon)
        {
            Debug.LogWarning("LevelManager could not spawn bonus items because no tilemap dungeon room list exists.");
            return;
        }

        bonusItemSpawner.SpawnItems(tilemapDungeonGenerator.Rooms, tilemapDungeonGenerator);
    }

    private void GenerateDungeonForLevel(LevelSettings level)
    {
        if (!generateDungeonOnLevelStart)
            return;

        ResolveDungeonGenerator();

        if (tilemapDungeonGenerator != null)
        {
            tilemapDungeonGenerator.GenerateDungeon();
            return;
        }

        if (dungeonGenerator == null)
        {
            Debug.LogWarning("LevelManager could not find a dungeon generator for level generation.");
            return;
        }

        if (level.dungeonRoomCount > 0)
            dungeonGenerator.GenerateForLevel(CurrentLevel, level.dungeonRoomCount);
        else
            dungeonGenerator.GenerateForLevel(CurrentLevel);
    }

    private void StopEnemiesForLevel()
    {
        if (enemyManager != null)
            enemyManager.ClearEnemies();
    }

    private void ClearBonusItemsForLevel()
    {
        if (bonusItemSpawner != null)
            bonusItemSpawner.ClearItems();
    }

    private void SpawnPlayerInStartRoom()
    {
        if (!spawnPlayerInStartRoom)
            return;

        ResolveDungeonGenerator();

        bool hasRoomDungeon = dungeonGenerator != null && dungeonGenerator.HasGeneratedDungeon;
        bool hasTilemapDungeon = tilemapDungeonGenerator != null && tilemapDungeonGenerator.HasGeneratedDungeon;

        if (!hasRoomDungeon && !hasTilemapDungeon)
        {
            Debug.LogWarning("LevelManager could not spawn the player because no generated start room exists.");
            return;
        }

        ResolvePlayer();

        if (playerTransform == null)
        {
            if (playerPrefab == null)
            {
                Debug.LogWarning("LevelManager could not find a player to spawn and no player prefab is assigned.");
                return;
            }

            GameObject player = Instantiate(playerPrefab);
            player.name = playerPrefab.name;
            playerTransform = player.transform;
        }

        ApplySelectedWeapon(playerTransform);

        Vector3 spawnPosition = hasTilemapDungeon
            ? tilemapDungeonGenerator.StartRoomCenter
            : dungeonGenerator.StartRoomCenter;
        spawnPosition.z = playerTransform.position.z;
        playerTransform.position = spawnPosition;

        Rigidbody2D playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRigidbody != null)
        {
            playerRigidbody.gravityScale = 0f;
            playerRigidbody.freezeRotation = true;
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        ResolveEnemyManager();
        if (enemyManager != null)
            enemyManager.SetPlayerTarget(playerTransform);

        BindSceneObjectsToPlayer();
    }

    private void StopLevelRoutine()
    {
        if (levelRoutine == null)
            return;

        StopCoroutine(levelRoutine);
        levelRoutine = null;
    }

    private void ResolveDungeonGenerator()
    {
        if (dungeonGenerator == null)
            dungeonGenerator = FindAnyObjectByType<DungeonGenerator2D>();

        if (tilemapDungeonGenerator == null)
            tilemapDungeonGenerator = FindAnyObjectByType<CorridorFirstDungeonGenerator>();
    }

    private void ResolveEnemyManager()
    {
        if (enemyManager != null)
        {
            BindEnemyManagerEvents();
            return;
        }

        enemyManager = GetComponent<EnemyManager>();

        if (enemyManager == null)
            enemyManager = FindAnyObjectByType<EnemyManager>();

        if (enemyManager == null)
            enemyManager = gameObject.AddComponent<EnemyManager>();

        BindEnemyManagerEvents();
    }

    private void BindEnemyManagerEvents()
    {
        if (enemyManager == null)
            return;

        enemyManager.EnemyCountChanged -= HandleEnemyCountChanged;
        enemyManager.EnemyCountChanged += HandleEnemyCountChanged;
    }

    private void ResolveBonusItemSpawner()
    {
        if (bonusItemSpawner != null)
            return;

        bonusItemSpawner = GetComponent<BonusItemSpawner>();

        if (bonusItemSpawner == null)
            bonusItemSpawner = FindAnyObjectByType<BonusItemSpawner>();

        if (bonusItemSpawner == null)
            bonusItemSpawner = gameObject.AddComponent<BonusItemSpawner>();
    }

    private void ResolveSceneBindings()
    {
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();

        if (cameraFollowPlayer == null)
            cameraFollowPlayer = FindAnyObjectByType<CameraFollowPlayer>();
    }

    private void BindSceneObjectsToPlayer()
    {
        ResolveSceneBindings();

        if (uiManager != null)
            uiManager.BindPlayer(playerTransform);

        if (cameraFollowPlayer != null)
            cameraFollowPlayer.SetTarget(playerTransform);
    }

    private void ApplySelectedWeapon(Transform player)
    {
        if (player == null)
            return;

        WeaponManager weaponManager = player.GetComponent<WeaponManager>();
        if (weaponManager != null)
            weaponManager.SetSelectedClass(selectedPlayerClass);
    }

    private void HandleEnemyCountChanged(int count)
    {
        ResolveSceneBindings();

        if (uiManager != null)
            uiManager.SetEnemiesCount(count);
    }

    private void ResolvePlayer()
    {
        if (playerTransform != null)
            return;

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            playerTransform = taggedPlayer.transform;
            return;
        }

        GameObject namedPlayer = GameObject.Find("Player");
        if (namedPlayer != null)
            playerTransform = namedPlayer.transform;
    }

    private void CreateDefaultLevels()
    {
        levels.Add(new LevelSettings
        {
            levelName = "Wave 1",
            duration = 30f
        });

        levels.Add(new LevelSettings
        {
            levelName = "Wave 2",
            duration = 45f
        });

        levels.Add(new LevelSettings
        {
            levelName = "Wave 3",
            duration = 60f
        });
    }

    private void OnValidate()
    {
        timeBetweenLevels = Mathf.Max(0f, timeBetweenLevels);

        foreach (LevelSettings level in levels)
        {
            level.duration = Mathf.Max(1f, level.duration);
            level.dungeonRoomCount = Mathf.Max(0, level.dungeonRoomCount);
        }
    }
}
