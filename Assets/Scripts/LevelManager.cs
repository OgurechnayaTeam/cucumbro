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
    [SerializeField] private float timeBetweenLevels = 5f;
    [SerializeField] private bool startOnAwake = true;

    [Header("Dungeon")]
    [SerializeField] private DungeonGenerator2D dungeonGenerator;
    [SerializeField] private bool generateDungeonOnLevelStart = true;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private bool spawnPlayerInStartRoom = true;

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
        StartEnemiesForLevel(level);
    }

    private void CompleteCurrentLevel()
    {
        levelRunning = false;
        Debug.Log($"Level {CurrentLevel} complete.");
        StopEnemiesForLevel();

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
        // EnemyManager interaction placeholder:
        // enemyManager.StartLevel(CurrentLevel, level);
    }

    private void GenerateDungeonForLevel(LevelSettings level)
    {
        if (!generateDungeonOnLevelStart)
            return;

        ResolveDungeonGenerator();

        if (dungeonGenerator == null)
        {
            Debug.LogWarning("LevelManager could not find a DungeonGenerator2D for level generation.");
            return;
        }

        if (level.dungeonRoomCount > 0)
            dungeonGenerator.GenerateForLevel(CurrentLevel, level.dungeonRoomCount);
        else
            dungeonGenerator.GenerateForLevel(CurrentLevel);
    }

    private void StopEnemiesForLevel()
    {
        // EnemyManager interaction placeholder:
        // enemyManager.StopLevel();
    }

    private void SpawnPlayerInStartRoom()
    {
        if (!spawnPlayerInStartRoom)
            return;

        ResolveDungeonGenerator();

        if (dungeonGenerator == null || !dungeonGenerator.HasGeneratedDungeon)
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

        Vector3 spawnPosition = dungeonGenerator.StartRoomCenter;
        spawnPosition.z = playerTransform.position.z;
        playerTransform.position = spawnPosition;

        Rigidbody2D playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRigidbody != null)
            playerRigidbody.linearVelocity = Vector2.zero;
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
