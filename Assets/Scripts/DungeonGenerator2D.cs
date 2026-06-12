using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator2D : MonoBehaviour
{
    private enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    private struct RoomData
    {
        public Vector2Int gridPosition;
        public Vector2 size;
        public Vector3 worldPosition;
    }

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 1;
    [SerializeField] private int roomCount = 8;
    [SerializeField] private Vector2Int minRoomSize = new Vector2Int(8, 6);
    [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(14, 10);
    [SerializeField] private float roomGap = 2f;

    [Header("Prefabs")]
    [SerializeField] private GameObject startRoomPrefab;
    [SerializeField] private GameObject[] roomPrefabs;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject exitPrefab;

    [Header("Parents")]
    [SerializeField] private Transform roomParent;
    [SerializeField] private Transform doorParent;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private readonly List<RoomData> generatedRooms = new List<RoomData>();

    public IReadOnlyList<GameObject> SpawnedObjects => spawnedObjects;

    private void Start()
    {
        if (generateOnStart)
            Generate();
    }

    [ContextMenu("Generate Dungeon")]
    public void Generate()
    {
        Clear();

        if (!useRandomSeed)
            Random.InitState(seed);

        int roomsToGenerate = Mathf.Max(1, roomCount);
        Dictionary<Vector2Int, RoomData> occupiedRooms = new Dictionary<Vector2Int, RoomData>();
        Vector2Int currentGridPosition = Vector2Int.zero;
        RoomData startRoom = CreateRoomData(currentGridPosition, Vector3.zero);

        generatedRooms.Add(startRoom);
        occupiedRooms.Add(currentGridPosition, startRoom);
        SpawnRoom(startRoom, true);

        Direction lastConnectionDirection = Direction.Right;

        for (int i = 1; i < roomsToGenerate; i++)
        {
            RoomData previousRoom = PickRoomWithFreeDirection(occupiedRooms);
            Direction direction = PickFreeDirection(previousRoom.gridPosition, occupiedRooms);
            currentGridPosition = previousRoom.gridPosition + DirectionToGridOffset(direction);

            RoomData room = CreateConnectedRoom(previousRoom, currentGridPosition, direction);
            generatedRooms.Add(room);
            occupiedRooms.Add(currentGridPosition, room);

            SpawnRoom(room, false);
            SpawnDoor(previousRoom, room, direction);
            lastConnectionDirection = direction;
        }

        SpawnLevelExit(generatedRooms[generatedRooms.Count - 1], lastConnectionDirection);
    }

    [ContextMenu("Clear Dungeon")]
    public void Clear()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
                continue;

            if (Application.isPlaying)
                Destroy(spawnedObjects[i]);
            else
                DestroyImmediate(spawnedObjects[i]);
        }

        spawnedObjects.Clear();
        generatedRooms.Clear();
    }

    private RoomData CreateConnectedRoom(RoomData previousRoom, Vector2Int gridPosition, Direction direction)
    {
        RoomData room = CreateRoomData(gridPosition, previousRoom.worldPosition);
        Vector3 offset = DirectionToWorldOffset(direction, previousRoom.size, room.size);
        room.worldPosition = previousRoom.worldPosition + offset;

        return room;
    }

    private RoomData CreateRoomData(Vector2Int gridPosition, Vector3 worldPosition)
    {
        int width = Random.Range(minRoomSize.x, maxRoomSize.x + 1);
        int height = Random.Range(minRoomSize.y, maxRoomSize.y + 1);

        return new RoomData
        {
            gridPosition = gridPosition,
            size = new Vector2(width, height),
            worldPosition = worldPosition
        };
    }

    private void SpawnRoom(RoomData room, bool isStartRoom)
    {
        GameObject prefab = isStartRoom && startRoomPrefab != null ? startRoomPrefab : GetRandomRoomPrefab();
        GameObject roomObject = Spawn(prefab, room.worldPosition, Quaternion.identity, roomParent, isStartRoom ? "Start Room" : "Room");
        roomObject.transform.localScale = new Vector3(room.size.x, room.size.y, 1f);
    }

    private void SpawnDoor(RoomData fromRoom, RoomData toRoom, Direction direction)
    {
        Vector3 doorPosition = GetDoorPosition(fromRoom, toRoom, direction);
        Quaternion doorRotation = direction == Direction.Left || direction == Direction.Right
            ? Quaternion.identity
            : Quaternion.Euler(0f, 0f, 90f);

        Spawn(doorPrefab, doorPosition, doorRotation, doorParent, "Door");
    }

    private void SpawnLevelExit(RoomData room, Direction exitDirection)
    {
        Vector3 exitPosition = GetWallPosition(room, exitDirection);
        Spawn(exitPrefab, exitPosition, Quaternion.identity, doorParent, "Level Exit");
    }

    private GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, string fallbackName)
    {
        GameObject instance = prefab != null
            ? Instantiate(prefab, position, rotation, parent)
            : new GameObject(fallbackName);

        instance.transform.SetParent(parent);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.name = fallbackName;
        spawnedObjects.Add(instance);

        return instance;
    }

    private GameObject GetRandomRoomPrefab()
    {
        if (roomPrefabs == null || roomPrefabs.Length == 0)
            return null;

        return roomPrefabs[Random.Range(0, roomPrefabs.Length)];
    }

    private RoomData PickRoomWithFreeDirection(Dictionary<Vector2Int, RoomData> occupiedRooms)
    {
        List<RoomData> roomsWithFreeDirections = new List<RoomData>();

        foreach (RoomData room in generatedRooms)
        {
            if (HasFreeDirection(room.gridPosition, occupiedRooms))
                roomsWithFreeDirections.Add(room);
        }

        if (roomsWithFreeDirections.Count == 0)
            return generatedRooms[generatedRooms.Count - 1];

        return roomsWithFreeDirections[Random.Range(0, roomsWithFreeDirections.Count)];
    }

    private Direction PickFreeDirection(Vector2Int fromGridPosition, Dictionary<Vector2Int, RoomData> occupiedRooms)
    {
        List<Direction> freeDirections = new List<Direction>();

        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        {
            Vector2Int targetGridPosition = fromGridPosition + DirectionToGridOffset(direction);

            if (!occupiedRooms.ContainsKey(targetGridPosition))
                freeDirections.Add(direction);
        }

        if (freeDirections.Count == 0)
            return (Direction)Random.Range(0, 4);

        return freeDirections[Random.Range(0, freeDirections.Count)];
    }

    private Vector3 GetDoorPosition(RoomData fromRoom, RoomData toRoom, Direction direction)
    {
        Vector3 wallPosition = GetWallPosition(fromRoom, direction);

        switch (direction)
        {
            case Direction.Up:
                return wallPosition + new Vector3(0f, roomGap * 0.5f, 0f);
            case Direction.Right:
                return wallPosition + new Vector3(roomGap * 0.5f, 0f, 0f);
            case Direction.Down:
                return wallPosition + new Vector3(0f, -roomGap * 0.5f, 0f);
            case Direction.Left:
                return wallPosition + new Vector3(-roomGap * 0.5f, 0f, 0f);
            default:
                return (fromRoom.worldPosition + toRoom.worldPosition) * 0.5f;
        }
    }

    private Vector3 GetWallPosition(RoomData room, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return room.worldPosition + new Vector3(0f, room.size.y * 0.5f, 0f);
            case Direction.Right:
                return room.worldPosition + new Vector3(room.size.x * 0.5f, 0f, 0f);
            case Direction.Down:
                return room.worldPosition + new Vector3(0f, -room.size.y * 0.5f, 0f);
            case Direction.Left:
                return room.worldPosition + new Vector3(-room.size.x * 0.5f, 0f, 0f);
            default:
                return room.worldPosition;
        }
    }

    private bool HasFreeDirection(Vector2Int gridPosition, Dictionary<Vector2Int, RoomData> occupiedRooms)
    {
        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        {
            if (!occupiedRooms.ContainsKey(gridPosition + DirectionToGridOffset(direction)))
                return true;
        }

        return false;
    }

    private Vector3 DirectionToWorldOffset(Direction direction, Vector2 fromSize, Vector2 toSize)
    {
        switch (direction)
        {
            case Direction.Up:
                return new Vector3(0f, fromSize.y * 0.5f + toSize.y * 0.5f + roomGap, 0f);
            case Direction.Right:
                return new Vector3(fromSize.x * 0.5f + toSize.x * 0.5f + roomGap, 0f, 0f);
            case Direction.Down:
                return new Vector3(0f, -(fromSize.y * 0.5f + toSize.y * 0.5f + roomGap), 0f);
            case Direction.Left:
                return new Vector3(-(fromSize.x * 0.5f + toSize.x * 0.5f + roomGap), 0f, 0f);
            default:
                return Vector3.zero;
        }
    }

    private Vector2Int DirectionToGridOffset(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector2Int.up;
            case Direction.Right:
                return Vector2Int.right;
            case Direction.Down:
                return Vector2Int.down;
            case Direction.Left:
                return Vector2Int.left;
            default:
                return Vector2Int.zero;
        }
    }

    private void OnValidate()
    {
        roomCount = Mathf.Max(1, roomCount);
        roomGap = Mathf.Max(0f, roomGap);

        minRoomSize.x = Mathf.Max(1, minRoomSize.x);
        minRoomSize.y = Mathf.Max(1, minRoomSize.y);
        maxRoomSize.x = Mathf.Max(minRoomSize.x, maxRoomSize.x);
        maxRoomSize.y = Mathf.Max(minRoomSize.y, maxRoomSize.y);
    }
}
