using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    public int sbRoomCount = 5;
    public int sideRoomCount = 2;

    private List<RoomNode> mainPath = new();

    [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);

    [Header("Seed Settings")]
    [SerializeField] private int seed = 0;
    [SerializeField] private bool useCustomSeed = false;

    public static RoomSpawner Instance;
    private RoomGraph graph = new();
    private RoomNode lastRoom = null;

    [SerializeField] private GameObject bossInteriorPrefab;
    [SerializeField] private GameObject startInteriorPrefab;
    [SerializeField] private GameObject shopInteriorPrefab;
    [SerializeField] private List<GameObject> normalRoomPrefabs;
    [SerializeField] private List<GameObject> rewardRoomPrefabs;

    private Vector2Int shopRoomPosition;
    private HashSet<Vector2Int> specialRoomPositions = new();
    public Vector2Int playerRoomPosition;

    [SerializeField] private int roomWidth = 35;
    [SerializeField] private int roomHeight = 23;
    private int roomGapX = 28;
    private int roomGapY = 24;

    private int roomsToSpawn;

    void Awake() => Instance = this;

    void Start()
    {
        if (useCustomSeed)
        {
            Random.InitState(seed);
            Debug.Log($"[Seed 적용] seed = {seed}");
        }
        else
        {
            seed = Random.Range(1, 999999);
            Random.InitState(seed);
            Debug.Log($"[랜덤 Seed 생성] seed = {seed}");
        }

        GenerateMap();
    }

    void GenerateMap()
    {
        RoomNode startNode = graph.CreateRoom(startPosition);
        startNode.IsStartRoom = true;
        playerRoomPosition = startPosition;

        mainPath.Add(startNode);
        roomsToSpawn = sbRoomCount - 1;
        DFSGenerate(startNode); // 루트 구성

        RoomNode bossRoom = mainPath.LastOrDefault(n => !n.IsStartRoom);
        if (bossRoom != null)
        {
            bossRoom.IsBossRoom = true;
            lastRoom = bossRoom;
            Debug.Log($"Boss room at {bossRoom.Position}");
        }

        PlaceShopRoom(startNode, lastRoom);
        GenerateSideRoomsFromStart(startNode); // 시작 방 주변 남은 방향에 갈래 생성
        GenerateSideRooms(); // 일반 갈래 생성
        SpawnAllRooms();

        RoomBehavior startRoom = GetRoomAtPosition(startPosition);
        if (startRoom != null)
        {
            var interior = startRoom.GetComponentInChildren<InteriorRoom>();
            if (interior != null)
            {
                Debug.Log("[MiniMap] 시작방 InteriorRoom에서 수동 RegisterMiniMap() 호출");
                interior.RegisterMiniMap();
            }
            else
            {
                Debug.LogWarning("[MiniMap] 시작방 InteriorRoom 찾지 못함");
            }
        }

    }

    void GenerateSideRoomsFromStart(RoomNode startNode)
    {
        foreach (var dir in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
        {
            Vector2Int pos = startNode.Position + dir.ToOffset();
            if (!graph.RoomExists(pos))
            {
                var sideRoom = graph.CreateRoom(pos);
                startNode.Connect(sideRoom, dir);
                Debug.Log($"[Start 갈래] 생성된 SideRoom at {pos}");
            }
        }
    }

    void DFSGenerate(RoomNode current)
    {
        if (roomsToSpawn <= 0) return;

        var directions = new List<Direction> {
            Direction.Up, Direction.Down, Direction.Left, Direction.Right
        };
        directions.Shuffle();

        int branchCount = Random.Range(2, 5);
        foreach (var dir in directions.Take(branchCount))
        {
            if (roomsToSpawn <= 0) break;

            Vector2Int nextPos = current.Position + dir.ToOffset();
            if (graph.RoomExists(nextPos)) continue;

            var nextRoom = graph.CreateRoom(nextPos);
            current.Connect(nextRoom, dir);

            roomsToSpawn--;
            lastRoom = nextRoom;
            mainPath.Add(nextRoom);

            DFSGenerate(nextRoom);
        }
    }

    void GenerateSideRooms()
    {
        int added = 0;

        while (added < sideRoomCount)
        {
            var candidate = mainPath
                .Where(n => !n.IsStartRoom && !n.IsBossRoom)
                .OrderBy(_ => Random.value)
                .FirstOrDefault();

            if (candidate == null) break;

            var dirs = new List<Direction> {
                Direction.Up, Direction.Down, Direction.Left, Direction.Right
            };
            dirs.Shuffle();

            foreach (var dir in dirs)
            {
                Vector2Int newPos = candidate.Position + dir.ToOffset();
                if (graph.RoomExists(newPos)) continue;

                var sideRoom = graph.CreateRoom(newPos);
                candidate.Connect(sideRoom, dir);
                added++;
                break;
            }
        }
    }

    void SpawnAllRooms()
    {
        foreach (var node in graph.nodes.Values)
        {
            Vector3 worldPos = new Vector3(
                node.Position.x * (roomWidth + roomGapX),
                node.Position.y * (roomHeight + roomGapY),
                0
            );

            GameObject roomGO = Instantiate(roomPrefab, worldPos, Quaternion.identity);
            roomGO.name = $"Room_{node.Position.x}_{node.Position.y}";

            RoomBehavior behavior = roomGO.GetComponent<RoomBehavior>();
            node.roomBehavior = behavior;
            if (behavior != null)
            {
                behavior.Position = node.Position;
                if (!node.IsBossRoom)
                    behavior.ConfigureDoors(node.Neighbors.Keys);
            }

            GameObject interiorPrefab = null;

            if (node.IsBossRoom && bossInteriorPrefab != null)
            {
                interiorPrefab = bossInteriorPrefab;
                specialRoomPositions.Add(node.Position);
            }
            else if (node.IsStartRoom && startInteriorPrefab != null)
            {
                interiorPrefab = startInteriorPrefab;
                specialRoomPositions.Add(node.Position);
            }
            else if (node.Position == shopRoomPosition && shopInteriorPrefab != null)
            {
                interiorPrefab = shopInteriorPrefab;
                specialRoomPositions.Add(node.Position);
            }
            else if (!specialRoomPositions.Contains(node.Position))
            {
                if (node.IsClosedRoom() && Random.value < 0.25f && rewardRoomPrefabs.Count > 0)
                {
                    interiorPrefab = rewardRoomPrefabs[Random.Range(0, rewardRoomPrefabs.Count)];
                }
                else if (normalRoomPrefabs.Count > 0)
                {
                    interiorPrefab = normalRoomPrefabs[Random.Range(0, normalRoomPrefabs.Count)];
                }
            }

            if (interiorPrefab != null)
            {
                GameObject interior = Instantiate(interiorPrefab, worldPos, Quaternion.identity);
                interior.name = $"Interior_{node.Position.x}_{node.Position.y}";
                interior.transform.SetParent(roomGO.transform);
            }
        }
    }

    void PlaceShopRoom(RoomNode start, RoomNode boss)
    {
        var path = graph.GetShortestPath(start, boss);
        if (path == null || path.Count < 3) return;

        int middleIndex = path.Count / 2;
        RoomNode shopNode = path[middleIndex];
        shopRoomPosition = shopNode.Position;

        Debug.Log($"[Shop Room 배치] at {shopRoomPosition}");
    }

    public void MovePlayerToNextRoom(Vector2Int offset, Direction enteredFrom)
    {
        Vector2Int nextRoomPos = playerRoomPosition + offset;

        Vector3 center = new Vector3(
            nextRoomPos.x * (roomWidth + roomGapX),
            nextRoomPos.y * (roomHeight + roomGapY),
            0
        );

        Vector3 adjustment = enteredFrom switch
        {
            Direction.Up => new Vector3(-1f, 0f, -0.9f),
            Direction.Down => new Vector3(-1f, 26f, -0.9f),
            Direction.Left => new Vector3(21f, 12.6f, -0.9f),
            Direction.Right => new Vector3(-23f, 12.6f, -0.9f),
            _ => Vector3.zero
        };

        Vector3 finalPos = center + adjustment;

        var player = GameObject.FindWithTag("Player");
        if (player != null) player.transform.position = finalPos;

        playerRoomPosition = nextRoomPos;

        var room = GetRoomAtPosition(playerRoomPosition);

        if (room == null)
        {
            Debug.LogWarning("[RoomSpawner] 현재 위치에 해당하는 RoomBehavior를 찾을 수 없습니다!");
        }
        else
        {
            // ✅ 미니맵 전용 중심 좌표 보정
            Vector3 correctedCenter = room.transform.position + new Vector3(-0.5f, 12f, 0f);

            Debug.Log($"[RoomSpawner] 현재 방 위치: {playerRoomPosition}, 중심 좌표(보정): {correctedCenter}");
            FindObjectOfType<MiniMapEntityTracker>()?.SetCenter(correctedCenter);

            var interior = room.GetComponentInChildren<InteriorRoom>();
            interior?.RegisterMiniMap();
        }
    }


    public RoomBehavior GetRoomAtPosition(Vector2Int pos)
    {
        return graph.GetRoom(pos)?.roomBehavior;
    }

}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
