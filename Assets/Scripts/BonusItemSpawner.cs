using System.Collections.Generic;
using UnityEngine;

public class BonusItemSpawner : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private List<GameObject> itemPrefabs = new List<GameObject>();
    [SerializeField, Range(0f, 1f)] private float roomSpawnChance = 0.35f;
    [SerializeField, Min(0)] private int minPerRoom = 0;
    [SerializeField, Min(0)] private int maxPerRoom = 1;
    [SerializeField] private bool spawnInStartRoom = false;
    [SerializeField, Min(0)] private int edgePaddingTiles = 1;

    [Header("Hierarchy")]
    [SerializeField] private Transform itemParent;

    private readonly List<GameObject> spawnedItems = new List<GameObject>();

    public void SpawnItems(IReadOnlyList<AbstractDungeonGenerator.DungeonRoom> rooms, AbstractDungeonGenerator dungeonGenerator)
    {
        ClearItems();

        if (rooms == null || rooms.Count == 0 || dungeonGenerator == null || itemPrefabs.Count == 0)
        {
            return;
        }

        EnsureItemParent();

        foreach (AbstractDungeonGenerator.DungeonRoom room in rooms)
        {
            if (!spawnInStartRoom && ContainsTile(room, dungeonGenerator.StartPosition))
            {
                continue;
            }

            if (Random.value > roomSpawnChance)
            {
                continue;
            }

            SpawnItemsInRoom(room, dungeonGenerator);
        }
    }

    public void ClearItems()
    {
        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            if (spawnedItems[i] != null)
            {
                Destroy(spawnedItems[i]);
            }
        }

        spawnedItems.Clear();
    }

    private void SpawnItemsInRoom(AbstractDungeonGenerator.DungeonRoom room, AbstractDungeonGenerator dungeonGenerator)
    {
        List<Vector2Int> spawnTiles = GetSpawnTiles(room);
        if (spawnTiles.Count == 0)
        {
            return;
        }

        int min = Mathf.Max(0, minPerRoom);
        int max = Mathf.Max(min, maxPerRoom);
        int count = Random.Range(min, max + 1);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Count)];
            if (prefab == null)
            {
                continue;
            }

            Vector2Int tile = spawnTiles[Random.Range(0, spawnTiles.Count)];
            Vector3 spawnPosition = dungeonGenerator.GetCellCenterWorld(tile);
            GameObject item = Instantiate(prefab, spawnPosition, Quaternion.identity, itemParent);
            SetLayer(item, "Pickup");
            spawnedItems.Add(item);
        }
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

    private void EnsureItemParent()
    {
        if (itemParent != null)
        {
            return;
        }

        GameObject parent = new GameObject("Bonus Items");
        itemParent = parent.transform;
    }

    private void SetLayer(GameObject item, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            return;
        }

        SetLayerRecursively(item, layer);
    }

    private void SetLayerRecursively(GameObject item, int layer)
    {
        item.layer = layer;

        foreach (Transform child in item.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void OnValidate()
    {
        minPerRoom = Mathf.Max(0, minPerRoom);
        maxPerRoom = Mathf.Max(minPerRoom, maxPerRoom);
        edgePaddingTiles = Mathf.Max(0, edgePaddingTiles);
    }
}
