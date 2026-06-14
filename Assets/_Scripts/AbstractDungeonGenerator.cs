using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [Serializable]
    public class DungeonRoom
    {
        [SerializeField] private Vector2Int center;
        [SerializeField] private List<Vector2Int> tiles;

        public Vector2Int Center => center;
        public IReadOnlyList<Vector2Int> Tiles => tiles;

        public DungeonRoom(Vector2Int center, IEnumerable<Vector2Int> tiles)
        {
            this.center = center;
            this.tiles = new List<Vector2Int>(tiles);
        }
    }

    [SerializeField]
    protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField]
    protected Vector2Int startPosition = Vector2Int.zero;
    [SerializeField]
    private List<DungeonRoom> rooms = new List<DungeonRoom>();

    public Vector3 StartRoomCenter => tilemapVisualizer != null
        ? tilemapVisualizer.GetCellCenterWorld(startPosition)
        : new Vector3(startPosition.x, startPosition.y, 0f);
    public Vector2Int StartPosition => startPosition;
    public bool HasGeneratedDungeon { get; private set; }
    public IReadOnlyList<DungeonRoom> Rooms => rooms;

    public Vector3 GetCellCenterWorld(Vector2Int position)
    {
        return tilemapVisualizer != null
            ? tilemapVisualizer.GetCellCenterWorld(position)
            : new Vector3(position.x, position.y, 0f);
    }

    public Vector2Int WorldToCell(Vector3 position)
    {
        return tilemapVisualizer != null
            ? tilemapVisualizer.WorldToCell(position)
            : new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    public void GenerateDungeon()
    {
        HasGeneratedDungeon = false;
        rooms.Clear();
        tilemapVisualizer.Clear();
        RunProceduralGeneration();
        HasGeneratedDungeon = true;
    }

    protected void RegisterRoom(IEnumerable<Vector2Int> roomTiles)
    {
        List<Vector2Int> tiles = new List<Vector2Int>(roomTiles);
        if (tiles.Count == 0)
        {
            return;
        }

        Vector2 total = Vector2.zero;
        foreach (Vector2Int tile in tiles)
        {
            total += new Vector2(tile.x, tile.y);
        }

        Vector2 average = total / tiles.Count;
        Vector2Int center = new Vector2Int(Mathf.RoundToInt(average.x), Mathf.RoundToInt(average.y));
        rooms.Add(new DungeonRoom(center, tiles));
    }

    protected abstract void RunProceduralGeneration();
}
