using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnRule
    {
        public GameObject enemyPrefab;
        [Range(0f, 1f)] public float roomSpawnChance = 1f;
        [Min(0)] public int minPerRoom = 1;
        [Min(0)] public int maxPerRoom = 2;
    }

    [Header("Room Spawning")]
    [SerializeField] private List<EnemySpawnRule> spawnRules = new List<EnemySpawnRule>();
    [SerializeField] private bool spawnInStartRoom = false;
    [SerializeField, Min(0)] private int edgePaddingTiles = 1;

    [Header("Hierarchy")]
    [SerializeField] private Transform enemyParent;

    [Header("Activation")]
    [SerializeField] private Transform playerTransform;

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    private readonly List<RoomEnemyGroup> roomEnemyGroups = new List<RoomEnemyGroup>();
    private AbstractDungeonGenerator activeDungeonGenerator;
    private int lastReportedEnemyCount = -1;

    public event Action<int> EnemyCountChanged;
    public int AliveEnemyCount => CountAliveEnemies();

    private class RoomEnemyGroup
    {
        public HashSet<Vector2Int> Tiles;
        public List<GameObject> Enemies = new List<GameObject>();
        public bool Activated;
    }

    private void Update()
    {
        ActivateEnemiesForPlayerRoom();
        ReportEnemyCountIfChanged();
    }

    public void SetPlayerTarget(Transform target)
    {
        playerTransform = target;

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy == null)
                continue;

            EnemyDarya enemyDarya = enemy.GetComponent<EnemyDarya>();
            if (enemyDarya != null)
                enemyDarya.SetTarget(playerTransform);

            Enemy enemyBase = enemy.GetComponent<Enemy>();
            if (enemyBase != null)
                enemyBase.SetTarget(playerTransform);
        }
    }

    public void SpawnEnemies(IReadOnlyList<AbstractDungeonGenerator.DungeonRoom> rooms, AbstractDungeonGenerator dungeonGenerator)
    {
        ClearEnemies();

        if (rooms == null || rooms.Count == 0 || dungeonGenerator == null)
        {
            ReportEnemyCountIfChanged();
            return;
        }

        EnsureEnemyParent();
        activeDungeonGenerator = dungeonGenerator;
        ResolvePlayer();

        for (int roomIndex = 0; roomIndex < rooms.Count; roomIndex++)
        {
            if (!spawnInStartRoom && ContainsTile(rooms[roomIndex], dungeonGenerator.StartPosition))
            {
                continue;
            }

            RoomEnemyGroup group = new RoomEnemyGroup
            {
                Tiles = new HashSet<Vector2Int>(rooms[roomIndex].Tiles)
            };
            roomEnemyGroups.Add(group);

            SpawnEnemiesInRoom(rooms[roomIndex], dungeonGenerator, group);
        }

        ReportEnemyCountIfChanged();
    }

    public void SpawnEnemies(IReadOnlyList<AbstractDungeonGenerator.DungeonRoom> rooms, AbstractDungeonGenerator dungeonGenerator, Transform playerTarget)
    {
        SetPlayerTarget(playerTarget);
        SpawnEnemies(rooms, dungeonGenerator);
    }

    public void ClearEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] != null)
            {
                Destroy(spawnedEnemies[i]);
            }
        }

        spawnedEnemies.Clear();
        roomEnemyGroups.Clear();
        activeDungeonGenerator = null;
        ReportEnemyCountIfChanged();
    }

    private void SpawnEnemiesInRoom(AbstractDungeonGenerator.DungeonRoom room, AbstractDungeonGenerator dungeonGenerator, RoomEnemyGroup group)
    {
        List<Vector2Int> spawnTiles = GetSpawnTiles(room);
        if (spawnTiles.Count == 0)
        {
            return;
        }

        foreach (EnemySpawnRule rule in spawnRules)
        {
            if (rule.enemyPrefab == null || Random.value > rule.roomSpawnChance)
            {
                continue;
            }

            int min = Mathf.Max(0, rule.minPerRoom);
            int max = Mathf.Max(min, rule.maxPerRoom);
            int count = Random.Range(min, max + 1);

            for (int i = 0; i < count; i++)
            {
                Vector2Int tile = spawnTiles[Random.Range(0, spawnTiles.Count)];
                Vector3 spawnPosition = dungeonGenerator.GetCellCenterWorld(tile);
                GameObject enemy = Instantiate(rule.enemyPrefab, spawnPosition, Quaternion.identity, enemyParent);
                ConfigureEnemy(enemy, false);
                spawnedEnemies.Add(enemy);
                group.Enemies.Add(enemy);
            }
        }
    }

    private void ActivateEnemiesForPlayerRoom()
    {
        if (activeDungeonGenerator == null)
        {
            return;
        }

        ResolvePlayer();
        if (playerTransform == null)
        {
            return;
        }

        Vector2Int playerTile = activeDungeonGenerator.WorldToCell(playerTransform.position);
        foreach (RoomEnemyGroup group in roomEnemyGroups)
        {
            if (group.Activated || !group.Tiles.Contains(playerTile))
            {
                continue;
            }

            group.Activated = true;
            foreach (GameObject enemy in group.Enemies)
            {
                ConfigureEnemy(enemy, true);
            }
        }
    }

    private void ConfigureEnemy(GameObject enemy, bool movementEnabled)
    {
        if (enemy == null)
        {
            return;
        }

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
            SetLayerRecursively(enemy.transform, enemyLayer);

        EnemyDarya enemyDarya = enemy.GetComponent<EnemyDarya>();
        Enemy enemyBase = enemy.GetComponent<Enemy>();

        if (enemyDarya == null && enemyBase == null)
            enemyDarya = enemy.AddComponent<EnemyDarya>();

        if (enemyDarya != null)
        {
            enemyDarya.SetTarget(playerTransform);
            enemyDarya.SetMovementEnabled(movementEnabled);
        }

        if (enemyBase != null)
        {
            enemyBase.SetTarget(playerTransform);
            enemyBase.SetMovementEnabled(movementEnabled);
        }

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = enemy.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Dynamic;

        Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
        if (enemyCollider == null)
            enemyCollider = enemy.AddComponent<BoxCollider2D>();

        enemyCollider.isTrigger = false;

        if (!movementEnabled)
            rb.linearVelocity = Vector2.zero;
    }

    private int CountAliveEnemies()
    {
        int count = 0;
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
                continue;
            }

            count++;
        }

        return count;
    }

    private void ReportEnemyCountIfChanged()
    {
        int aliveCount = CountAliveEnemies();
        if (aliveCount == lastReportedEnemyCount)
        {
            return;
        }

        lastReportedEnemyCount = aliveCount;
        EnemyCountChanged?.Invoke(aliveCount);
    }

    private List<Vector2Int> GetSpawnTiles(AbstractDungeonGenerator.DungeonRoom room)
    {
        List<Vector2Int> spawnTiles = new List<Vector2Int>();
        IReadOnlyList<Vector2Int> tiles = room.Tiles;

        if (edgePaddingTiles <= 0)
        {
            spawnTiles.AddRange(tiles);
            return spawnTiles;
        }

        HashSet<Vector2Int> tileSet = new HashSet<Vector2Int>(tiles);
        foreach (Vector2Int tile in tiles)
        {
            if (HasPadding(tile, tileSet))
            {
                spawnTiles.Add(tile);
            }
        }

        if (spawnTiles.Count == 0)
        {
            spawnTiles.Add(room.Center);
        }

        return spawnTiles;
    }

    private bool HasPadding(Vector2Int tile, HashSet<Vector2Int> tileSet)
    {
        for (int x = -edgePaddingTiles; x <= edgePaddingTiles; x++)
        {
            for (int y = -edgePaddingTiles; y <= edgePaddingTiles; y++)
            {
                if (!tileSet.Contains(tile + new Vector2Int(x, y)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool ContainsTile(AbstractDungeonGenerator.DungeonRoom room, Vector2Int tile)
    {
        foreach (Vector2Int roomTile in room.Tiles)
        {
            if (roomTile == tile)
            {
                return true;
            }
        }

        return false;
    }

    private void EnsureEnemyParent()
    {
        if (enemyParent != null)
        {
            return;
        }

        GameObject parent = new GameObject("Enemies");
        enemyParent = parent.transform;
    }

    private void ResolvePlayer()
    {
        if (playerTransform != null)
        {
            return;
        }

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

    private void OnValidate()
    {
        edgePaddingTiles = Mathf.Max(0, edgePaddingTiles);

        foreach (EnemySpawnRule rule in spawnRules)
        {
            rule.minPerRoom = Mathf.Max(0, rule.minPerRoom);
            rule.maxPerRoom = Mathf.Max(rule.minPerRoom, rule.maxPerRoom);
        }
    }

    private void SetLayerRecursively(Transform root, int layer)
    {
        if (root == null)
            return;

        root.gameObject.layer = layer;

        for (int i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layer);
        }
    }
}
