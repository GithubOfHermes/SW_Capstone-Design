using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MiniMapEntityTracker : MonoBehaviour
{
    [SerializeField] private RectTransform iconPrefab;
    [SerializeField] private RectTransform minimapArea;

    private Vector2 minimapCenterWorldPosition = Vector2.zero;
    private Vector2 playerIconOffset = Vector2.zero;

    private class TrackedEntity
    {
        public Transform target;
        public RectTransform icon;
    }

    private readonly List<TrackedEntity> trackedEntities = new();

    public static MiniMapEntityTracker Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // ✅ 현재 씬에 존재하는 모든 Portal 즉시 등록
        GameObject[] portals = GameObject.FindGameObjectsWithTag("Portal");
        foreach (var portal in portals)
        {
            AddEntity(portal.transform, Color.green);
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            AddEntity(player.transform, Color.yellow);
            SetCenter(player.transform.position);
        }
    }

    void Update()
    {
        if (MiniMapRenderer.Instance == null || !MiniMapRenderer.Instance.HasValidTilemap())
            return;

        float pixelsPerUnit = MiniMapRenderer.Instance.GetPixelsPerUnit();
        float manualCorrectionFactor = GetSceneSpecificCorrectionFactor();
        pixelsPerUnit *= manualCorrectionFactor;
        if (pixelsPerUnit <= 0f) return;

        foreach (var tracked in trackedEntities)
        {
            if (tracked.target == null || tracked.icon == null) continue;

            Vector2 offset = (Vector2)tracked.target.position - minimapCenterWorldPosition;

            // ✅ 플레이어일 경우 보정값 적용 (world offset → scaled)
            if (tracked.target.CompareTag("Player"))
                offset += playerIconOffset;

            Vector2 anchored = offset * pixelsPerUnit;
            tracked.icon.anchoredPosition = anchored;
        }
    }
    private float GetSceneSpecificCorrectionFactor()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        return sceneName switch
        {
            "1-2 Hall" => 0.45f,         // 실제 사이즈 반영
            "1-3 Elite" => 0.7f,
            "2 Way" => 0.68f,
            _ => 1f
        };
    }
    public void SetCenter(Vector2 worldCenter)
    {
        minimapCenterWorldPosition = worldCenter + GetSceneSpecificCenterOffset();
        Debug.Log($"[MiniMap] Center Set To: {minimapCenterWorldPosition}");
    }

    public void AddEntity(Transform target, Color color)
    {
        if (iconPrefab == null || minimapArea == null)
        {
            Debug.LogWarning("[MiniMap] AddEntity 실패 - iconPrefab 또는 minimapArea가 null입니다.");
            return;
        }
        RectTransform icon = Instantiate(iconPrefab, minimapArea);
        icon.GetComponent<Image>().color = color;

        trackedEntities.Add(new TrackedEntity
        {
            target = target,
            icon = icon
        });

        Debug.Log($"[MiniMap] AddEntity 성공: {target.name} / 색상: {color}");
    }
    private Vector2 GetSceneSpecificCenterOffset()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        return sceneName switch
        {
            "1-2 Hall" => new Vector2(46f, 13f),
            "1-3 Elite" => new Vector2(35f, 23f),
            _ => Vector2.zero
        };
    }

    public void RemoveEntity(Transform target)
    {
        for (int i = trackedEntities.Count - 1; i >= 0; i--)
        {
            if (trackedEntities[i].target == target)
            {
                if (trackedEntities[i].icon != null)
                    Destroy(trackedEntities[i].icon.gameObject);

                trackedEntities.RemoveAt(i);
                break;
            }
        }
    }

    public void ClearAllEntities()
    {
        foreach (var tracked in trackedEntities)
        {
            if (tracked.icon != null)
                Destroy(tracked.icon.gameObject);
        }
        trackedEntities.Clear();
        Debug.Log("[MiniMapTracker] 모든 Entity 아이콘 제거 완료");
    }

}
