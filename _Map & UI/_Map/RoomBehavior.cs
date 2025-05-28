using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomBehavior : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [SerializeField] private Tilemap wallTilemap;

    [Header("Door Effects")]
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private Transform doorParent;
    public List<GameObject> doorObjects = new();
    public Vector2Int Position { get; set; }
    private List<MonoBehaviour> monsters = new List<MonoBehaviour>();

    private readonly Dictionary<Direction, List<Vector3Int>> tileSets = new()
    {
        { Direction.Up, new List<Vector3Int> {
            new Vector3Int(-2, 20, 0),
            new Vector3Int(-1, 20, 0),
            new Vector3Int(0, 20, 0)
        }},
        { Direction.Down, new List<Vector3Int> {
            new Vector3Int(-2, -2, 0),
            new Vector3Int(-1, -2, 0),
            new Vector3Int(0, -2, 0)
        }},
        { Direction.Left, new List<Vector3Int> {
            new Vector3Int(-18, 10, 0),
            new Vector3Int(-18, 9, 0),
            new Vector3Int(-18, 8, 0)
        }},
        { Direction.Right, new List<Vector3Int> {
            new Vector3Int(16, 10, 0),
            new Vector3Int(16, 9, 0),
            new Vector3Int(16, 8, 0)
        }}
    };

    public void ConfigureDoors(IEnumerable<Direction> openDirections)
    {
        if (wallTilemap == null)
        {
            Debug.LogWarning($"[RoomBehavior] wallTilemap is not assigned in {name}");
            return;
        }

        HashSet<Direction> openSet = openDirections != null
            ? new HashSet<Direction>(openDirections)
            : new HashSet<Direction>();

        foreach (var pair in tileSets)
        {
            Direction dir = pair.Key;
            List<Vector3Int> doorTiles = pair.Value;

            if (openSet.Contains(dir))
            {
                foreach (var innerPos in doorTiles)
                {
                    wallTilemap.SetTile(innerPos, null);
                }

                if (doorPrefab != null)
                {
                    Vector3 basePos = wallTilemap.CellToWorld(doorTiles[1]) + wallTilemap.tileAnchor;
                    Vector3 offset = Vector3.zero;
                    float zRot = 0f;

                    switch (dir)
                    {
                        case Direction.Right:
                            offset = new Vector3(1.25f, 0.19f, -0.8f);
                            zRot = 0f;
                            break;
                        case Direction.Left:
                            offset = new Vector3(-1f, 0.19f, -0.8f);
                            zRot = 180f;
                            break;
                        case Direction.Up:
                            offset = new Vector3(0.15f, 1.15f, -0.8f);
                            zRot = -90f;
                            break;
                        case Direction.Down:
                            offset = new Vector3(0.15f, -1f, -0.8f);
                            zRot = 90f;
                            break;
                    }
                    Vector3 finalWorldPos = basePos + offset;
                    var fx = Instantiate(doorPrefab, finalWorldPos, Quaternion.Euler(0, 0, zRot), transform);
                    var trigger = fx.AddComponent<DoorParticleTrigger>();
                    RegisterDoor(fx);
                }
            }
        }
    }
    public void RegisterMonster(MonoBehaviour monster)
    {
        monsters.Add(monster);
    }
    public void RegisterDoor(GameObject door)
    {
        if (!doorObjects.Contains(door))
        {
            doorObjects.Add(door);
        }

        var col = door.GetComponent<Collider2D>();
        var sprite = door.GetComponent<SpriteRenderer>();
        var anim = door.GetComponent<Animator>();

        if (col != null) col.enabled = false;
        if (sprite != null) sprite.color = Color.red;
        if (anim != null) anim.enabled = false;
    }

    public void LockAllDoors()
    {
        foreach (var door in doorObjects)
        {
            var col = door.GetComponent<Collider2D>();
            var sprite = door.GetComponent<SpriteRenderer>();
            var anim = door.GetComponent<Animator>();

            if (col != null) col.enabled = false;
            if (sprite != null) sprite.color = Color.red;
            if (anim != null) anim.enabled = false;
        }
    }

    public void OpenDoors()
    {

        foreach (var door in doorObjects)
        {
            var col = door.GetComponent<Collider2D>();
            var sprite = door.GetComponent<SpriteRenderer>();
            var anim = door.GetComponent<Animator>();


            if (col != null) col.enabled = true;
            if (sprite != null) sprite.color = Color.white;
            if (anim != null) anim.enabled = true;
        }
    }
}
