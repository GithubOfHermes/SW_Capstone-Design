using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDebugger : MonoBehaviour
{
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private Color gizmoColor = Color.cyan;

    private void OnDrawGizmos()
    {
        if (targetTilemap == null) return;

        BoundsInt bounds = targetTilemap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (!targetTilemap.HasTile(pos)) continue;

            Vector3 worldPos = targetTilemap.CellToWorld(pos);
            Vector3 cellCenter = worldPos + new Vector3(0.7f, 0.7f); // 셀 중심 보정 (1.4 스케일에 맞춤)

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(cellCenter, Vector3.one * 1.4f); // 셀 크기 확대

#if UNITY_EDITOR
            UnityEditor.Handles.Label(cellCenter + Vector3.up * 0.3f, $"({pos.x}, {pos.y})");
#endif
        }
    }
}
