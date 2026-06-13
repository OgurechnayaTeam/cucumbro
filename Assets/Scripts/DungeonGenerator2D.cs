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
        public Vector3 localPosition;
    }

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 1;
    [SerializeField] private int roomCount = 8;
    [SerializeField] private Vector2Int minRoomSize = new Vector2Int(3, 3);
    [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(6, 5);
    [SerializeField] private float roomGap = 1f;
    [SerializeField] private float roomPadding = 0.1f;

    [Header("Prefabs")]
    [SerializeField] private GameObject startRoomPrefab;
    [SerializeField] private GameObject[] roomPrefabs;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject exitPrefab;

    [Header("Parent")]
    [SerializeField] private Transform dungeonParent;
    [SerializeField] private string dungeonObjectName = "Generated Dungeon";

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private readonly List<RoomData> generatedRooms = new List<RoomData>();
    private GameObject currentDungeonObject;
    private Transform currentDungeonTransform;

    public IReadOnlyList<GameObject> SpawnedObjects => spawnedObjects;
    public bool GenerateOnStart
    {
        get => generateOnStart;
        set => generateOnStart = value;
    }

    public int CurrentGeneratedLevel { get; private set; } = 1;
    public Vector3 StartRoomCenter { get; private set; } = Vector3.zero;
    public bool HasGeneratedDungeon => generatedRooms.Count > 0;

    private void Start()
    {
        if (generateOnStart)
            Generate();
    }

    [ContextMenu("Generate Dungeon")]
    public void Generate()
    {
        CurrentGeneratedLevel = 1;
        Generate(roomCount);
    }

    public void GenerateForLevel(int levelNumber)
    {
        GenerateForLevel(levelNumber, roomCount);
    }

    public void GenerateForLevel(int levelNumber, int roomsToGenerate)
    {
        CurrentGeneratedLevel = Mathf.Max(1, levelNumber);
        Generate(roomsToGenerate);
    }

    private void Generate(int roomsToGenerate)
    {
        Clear();

        if (!useRandomSeed)
            Random.InitState(seed + CurrentGeneratedLevel - 1);

        roomsToGenerate = Mathf.Max(1, roomsToGenerate);
        CreateDungeonParent(GetScreenCenterWorldPosition());

        Dictionary<Vector2Int, RoomData> occupiedRooms = new Dictionary<Vector2Int, RoomData>();
        Vector2Int currentGridPosition = Vector2Int.zero;
        RoomData startRoom = CreateRoomData(currentGridPosition, Vector3.zero);
        StartRoomCenter = currentDungeonTransform.TransformPoint(startRoom.localPosition);

        generatedRooms.Add(startRoom);
        occupiedRooms.Add(currentGridPosition, startRoom);
        SpawnRoom(startRoom, true);

        Direction lastConnectionDirection = Direction.Right;

        for (int i = 1; i < roomsToGenerate; i++)
        {
            if (!TryCreateConnectedRoom(occupiedRooms, out RoomData previousRoom, out RoomData room, out Direction direction))
            {
                Debug.LogWarning($"Dungeon generation stopped at {generatedRooms.Count} rooms because no non-overlapping room position was found.");
                break;
            }

            currentGridPosition = room.gridPosition;
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
        if (currentDungeonObject != null)
        {
            if (Application.isPlaying)
                Destroy(currentDungeonObject);
            else
                DestroyImmediate(currentDungeonObject);
        }

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
        currentDungeonObject = null;
        currentDungeonTransform = null;
    }

    private RoomData CreateConnectedRoom(RoomData previousRoom, Vector2Int gridPosition, Direction direction)
    {
        RoomData room = CreateRoomData(gridPosition, previousRoom.localPosition);
        Vector3 offset = DirectionToLocalOffset(direction, previousRoom.size, room.size);
        room.localPosition = previousRoom.localPosition + offset;

        return room;
    }

    private RoomData CreateRoomData(Vector2Int gridPosition, Vector3 localPosition)
    {
        int width = Random.Range(minRoomSize.x, maxRoomSize.x + 1);
        int height = Random.Range(minRoomSize.y, maxRoomSize.y + 1);

        return new RoomData
        {
            gridPosition = gridPosition,
            size = new Vector2(width, height),
            localPosition = localPosition
        };
    }

    private void SpawnRoom(RoomData room, bool isStartRoom)
    {
        GameObject prefab = isStartRoom && startRoomPrefab != null ? startRoomPrefab : GetRandomRoomPrefab();
        GameObject roomObject = Spawn(prefab, room.localPosition, Quaternion.identity, isStartRoom ? "Start Room" : "Room");
        FitRoomToSize(roomObject, room.size);
    }

    private void SpawnDoor(RoomData fromRoom, RoomData toRoom, Direction direction)
    {
        Vector3 doorPosition = GetDoorPosition(fromRoom, toRoom, direction);
        Quaternion doorRotation = direction == Direction.Left || direction == Direction.Right
            ? Quaternion.identity
            : Quaternion.Euler(0f, 0f, 90f);

        Spawn(doorPrefab, doorPosition, doorRotation, "Door");
    }

    private void SpawnLevelExit(RoomData room, Direction exitDirection)
    {
        Vector3 exitPosition = GetWallPosition(room, exitDirection);
        Spawn(exitPrefab, exitPosition, Quaternion.identity, "Level Exit");
    }

    private GameObject Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, string fallbackName)
    {
        GameObject instance = prefab != null
            ? Instantiate(prefab, currentDungeonTransform)
            : new GameObject(fallbackName);

        instance.transform.SetParent(currentDungeonTransform, false);
        instance.transform.SetLocalPositionAndRotation(localPosition, localRotation);
        instance.name = fallbackName;
        spawnedObjects.Add(instance);

        return instance;
    }

    private void CreateDungeonParent(Vector3 position)
    {
        currentDungeonObject = new GameObject($"{dungeonObjectName} {CurrentGeneratedLevel}");
        currentDungeonTransform = currentDungeonObject.transform;
        currentDungeonTransform.SetParent(dungeonParent);
        currentDungeonTransform.SetPositionAndRotation(position, Quaternion.identity);
    }

    private Vector3 GetScreenCenterWorldPosition()
    {
        Camera camera = Camera.main;

        if (camera == null)
            return Vector3.zero;

        if (camera.orthographic)
            return new Vector3(camera.transform.position.x, camera.transform.position.y, 0f);

        Plane worldPlane = new Plane(Vector3.forward, Vector3.zero);
        Ray centerRay = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (worldPlane.Raycast(centerRay, out float distance))
            return centerRay.GetPoint(distance);

        return Vector3.zero;
    }

    private GameObject GetRandomRoomPrefab()
    {
        if (roomPrefabs == null || roomPrefabs.Length == 0)
            return null;

        return roomPrefabs[Random.Range(0, roomPrefabs.Length)];
    }

    private bool TryCreateConnectedRoom(
        Dictionary<Vector2Int, RoomData> occupiedRooms,
        out RoomData previousRoom,
        out RoomData room,
        out Direction direction)
    {
        const int maxAttempts = 100;

        previousRoom = default;
        room = default;
        direction = Direction.Right;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            previousRoom = PickRoomWithFreeDirection(occupiedRooms);
            direction = PickFreeDirection(previousRoom.gridPosition, occupiedRooms);
            Vector2Int gridPosition = previousRoom.gridPosition + DirectionToGridOffset(direction);

            if (occupiedRooms.ContainsKey(gridPosition))
                continue;

            room = CreateConnectedRoom(previousRoom, gridPosition, direction);

            if (!OverlapsExistingRoom(room))
                return true;
        }

        return false;
    }

    private bool OverlapsExistingRoom(RoomData candidate)
    {
        foreach (RoomData existingRoom in generatedRooms)
        {
            if (RoomsOverlap(candidate, existingRoom))
                return true;
        }

        return false;
    }

    private bool RoomsOverlap(RoomData a, RoomData b)
    {
        float maxDistanceX = (a.size.x + b.size.x) * 0.5f + roomPadding;
        float maxDistanceY = (a.size.y + b.size.y) * 0.5f + roomPadding;

        return Mathf.Abs(a.localPosition.x - b.localPosition.x) < maxDistanceX
            && Mathf.Abs(a.localPosition.y - b.localPosition.y) < maxDistanceY;
    }

    private void FitRoomToSize(GameObject roomObject, Vector2 targetSize)
    {
        if (!TryGetObjectBounds(roomObject, out Bounds bounds)
            || bounds.size.x <= Mathf.Epsilon
            || bounds.size.y <= Mathf.Epsilon)
        {
            roomObject.transform.localScale = new Vector3(targetSize.x, targetSize.y, roomObject.transform.localScale.z);
            return;
        }

        Vector3 localScale = roomObject.transform.localScale;
        roomObject.transform.localScale = new Vector3(
            localScale.x * targetSize.x / bounds.size.x,
            localScale.y * targetSize.y / bounds.size.y,
            localScale.z);
    }

    private bool TryGetObjectBounds(GameObject gameObject, out Bounds bounds)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return true;
        }

        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>();

        if (colliders.Length > 0)
        {
            bounds = colliders[0].bounds;

            for (int i = 1; i < colliders.Length; i++)
                bounds.Encapsulate(colliders[i].bounds);

            return true;
        }

        bounds = default;
        return false;
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
                return (fromRoom.localPosition + toRoom.localPosition) * 0.5f;
        }
    }

    private Vector3 GetWallPosition(RoomData room, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return room.localPosition + new Vector3(0f, room.size.y * 0.5f, 0f);
            case Direction.Right:
                return room.localPosition + new Vector3(room.size.x * 0.5f, 0f, 0f);
            case Direction.Down:
                return room.localPosition + new Vector3(0f, -room.size.y * 0.5f, 0f);
            case Direction.Left:
                return room.localPosition + new Vector3(-room.size.x * 0.5f, 0f, 0f);
            default:
                return room.localPosition;
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

    private Vector3 DirectionToLocalOffset(Direction direction, Vector2 fromSize, Vector2 toSize)
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
        roomPadding = Mathf.Max(0f, roomPadding);

        minRoomSize.x = Mathf.Max(1, minRoomSize.x);
        minRoomSize.y = Mathf.Max(1, minRoomSize.y);
        maxRoomSize.x = Mathf.Max(minRoomSize.x, maxRoomSize.x);
        maxRoomSize.y = Mathf.Max(minRoomSize.y, maxRoomSize.y);
    }
}
