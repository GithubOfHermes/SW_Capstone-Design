using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Collections;

public class InteriorRoom : MonoBehaviour
{
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap platformTilemap;

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != "2 Way")
        {
            Debug.Log($"[InteriorRoom] '{sceneName}' 씬이므로 Start에서 RegisterMiniMap() 실행");
            StartCoroutine(WaitThenRegister());
        }
    }

    public void RegisterMiniMap()
    {
        StartCoroutine(WaitThenRegister());
    }

    private IEnumerator WaitThenRegister()
    {
        yield return new WaitUntil(() => MiniMapRenderer.Instance != null);
        MiniMapRenderer.Instance.RegisterRoomTilemaps(wallTilemap, groundTilemap, platformTilemap);
    }
}
