using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MiniMapRenderer : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap platformTilemap;

    [Header("MiniMap Display")]
    [SerializeField] private RawImage miniMapImage;

    [Header("Tile Colors")]
    [SerializeField] private Color backgroundColor = new Color(1f, 1f, 1f, 100f / 255f);
    [SerializeField] private Color wallColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color groundColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color platformColor = new Color(0f, 0f, 0f, 1f);

    [Header("Debug Settings")]
    [SerializeField] private Vector2Int testOverrideCellSize = Vector2Int.zero;
    [SerializeField] private int pixelsPerCell = 2;

    private Texture2D texture;
    public static MiniMapRenderer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        // ✅ 씬 이름 기반 자동 설정
        string sceneName = SceneManager.GetActiveScene().name;
        testOverrideCellSize = GetCellSizeForScene(sceneName);
    }

    private Vector2Int GetCellSizeForScene(string sceneName)
    {
        // ✅ 씬 이름에 따라 방 크기 수동 지정
        Dictionary<string, Vector2Int> mapSizes = new()
        {
            { "1-2 Hall", new Vector2Int(105, 30) },
            { "1-3 Elite", new Vector2Int(57, 35) },
        };

        if (sceneName == "2 Way")
            return Vector2Int.zero; // 자동 계산 (InteriorRoom 사용)

        if (mapSizes.TryGetValue(sceneName, out Vector2Int size))
            return size;

        Debug.LogWarning($"[MiniMap] 씬 '{sceneName}'에 대한 CellSize가 지정되지 않았습니다. 기본값 사용.");
        return new Vector2Int(35, 23); // 기본값
    }

    public Texture2D GetTexture() => texture;

    public Vector2Int GetRenderedCellSize()
    {
        if (testOverrideCellSize != Vector2Int.zero)
            return testOverrideCellSize;

        return wallTilemap != null
            ? new Vector2Int(wallTilemap.cellBounds.size.x, wallTilemap.cellBounds.size.y)
            : new Vector2Int(35, 23);
    }

    public void RegisterRoomTilemaps(Tilemap wall, Tilemap ground, Tilemap platform)
    {
        wallTilemap = wall;
        groundTilemap = ground;
        platformTilemap = platform;

        StartCoroutine(RenderWhenReady());
    }

    private IEnumerator RenderWhenReady()
    {
        yield return new WaitUntil(() => wallTilemap != null && wallTilemap.cellBounds.size.x > 0);
        wallTilemap.CompressBounds();
        RenderMiniMap();
    }


    public void RenderMiniMap()
    {
        // ✅ 압축된 타일맵 경계 기준으로 계산
        wallTilemap.CompressBounds();
        BoundsInt bounds = wallTilemap.cellBounds;

        Vector2Int cellSize = new Vector2Int(bounds.size.x, bounds.size.y);
        int mapWidth = pixelsPerCell * cellSize.x;
        int mapHeight = pixelsPerCell * cellSize.y;


        // ✅ 텍스처 생성
        texture = new Texture2D(mapWidth, mapHeight);
        texture.filterMode = FilterMode.Point;

        for (int y = 0; y < cellSize.y; y++)
        {
            for (int x = 0; x < cellSize.x; x++)
            {
                Vector3Int cellPos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                Color pixelColor = backgroundColor;

                if (wallTilemap.HasTile(cellPos))
                    pixelColor = wallColor;
                else if (groundTilemap.HasTile(cellPos))
                    pixelColor = groundColor;
                else if (platformTilemap != null && platformTilemap.HasTile(cellPos)) // ✅ null 체크
                    pixelColor = platformColor;


                for (int dx = 0; dx < pixelsPerCell; dx++)
                {
                    for (int dy = 0; dy < pixelsPerCell; dy++)
                    {
                        int px = x * pixelsPerCell + dx;
                        int py = y * pixelsPerCell + dy;
                        texture.SetPixel(px, py, pixelColor);
                    }
                }
            }
        }

        texture.Apply();
        miniMapImage.texture = texture;

        // ✅ UI 설정
        miniMapImage.uvRect = new Rect(0f, 0f, 1f, 1f);
        miniMapImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        miniMapImage.rectTransform.sizeDelta = new Vector2(mapWidth, mapHeight);

        string sceneName = SceneManager.GetActiveScene().name;
        float scale = sceneName switch
        {
            "1-2 Hall" => 1f,
            _ => 65f / mapHeight
        };
        miniMapImage.rectTransform.localScale = new Vector3(scale, scale, 1f);
    }

    public float GetPixelsPerUnit()
    {
        Vector2Int cellSize = new Vector2Int(wallTilemap.cellBounds.size.x, wallTilemap.cellBounds.size.y);
        int mapHeight = pixelsPerCell * cellSize.y;

        float targetPanelHeight = 65f;
        float scale = targetPanelHeight / mapHeight;

        return pixelsPerCell * scale; // 화면 기준 픽셀/유닛
    }
    public bool HasValidTilemap()
    {
        return wallTilemap != null && wallTilemap && wallTilemap.cellBounds.size.y > 0;
    }
    public void ResetMiniMap()
    {
        texture = null;
        miniMapImage.texture = null;
        miniMapImage.rectTransform.anchoredPosition = Vector2.zero;
        miniMapImage.rectTransform.sizeDelta = Vector2.zero;
    }

}
