using UnityEngine;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    public ISceneLoader loader;
    public IFadeController fader;
    public IHUDSceneManager hudManager;
    public IPlayerStateService playerService;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 구성요소 수동 할당 또는 GetComponent로 연결
            loader = GetComponent<ISceneLoader>();
            hudManager = GetComponent<IHUDSceneManager>();
            playerService = GetComponent<IPlayerStateService>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForFader());
    }

    private IEnumerator WaitForFader()
    {
        while (FadeController.Instance == null)
        {
            yield return null;
        }

        fader = FadeController.Instance;
    }

    public void LoadNextScene()
    {
        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            string nextSceneName = System.IO.Path.GetFileNameWithoutExtension(
                UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(nextIndex));
            TransitionTo(nextSceneName);
        }
        else
        {
            Debug.LogWarning("다음 씬이 존재하지 않습니다.");
        }
    }

    public void TransitionTo(string sceneName)
    {
        StartCoroutine(HandleSceneTransition(sceneName));
    }

    private IEnumerator HandleSceneTransition(string sceneName)
    {
        playerService.DisablePlayer();
        yield return fader.FadeOut();

        loader.LoadScene(sceneName);
        if (sceneName != "0 Main" && sceneName != "0-0 King's Room" && sceneName != "0-1 Bedroom")
        {
            yield return hudManager.EnsureHUDLoaded();
        }

        if (sceneName != "0 Main" && sceneName != "0-0 King's Room" && sceneName != "0-1 Bedroom")
        {
            yield return new WaitUntil(() => GameObject.FindWithTag("HUDCanvas") != null);
            var hudCanvas = GameObject.FindWithTag("HUDCanvas");
            var infoUI = hudCanvas?.transform.Find("Info UI")?.gameObject;

            if (sceneName == "4 Fin")
                hudCanvas.SetActive(false);

            if (sceneName == "3 Boss Room")
                infoUI.SetActive(false);

            if (sceneName == "1-2 Hall")
            {
                infoUI.SetActive(true);
                yield return null;

                yield return new WaitUntil(() =>
                    MiniMapRenderer.Instance != null &&
                    MiniMapRenderer.Instance.GetTexture() != null &&
                    MiniMapRenderer.Instance.GetRenderedCellSize().y > 0 &&
                    MiniMapRenderer.Instance.HasValidTilemap()
                );

                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    Debug.Log("[MiniMap] 1-2 Hall AddEntity 수동 호출");
                    MiniMapEntityTracker.Instance?.AddEntity(player.transform, Color.yellow);
                    MiniMapEntityTracker.Instance?.SetCenter(player.transform.position); // ✅ 위치 중심 보정
                }
            }
        }

        if (sceneName == "2 Way")
        {
            MiniMapRenderer.Instance?.ResetMiniMap();
            MiniMapEntityTracker.Instance?.ClearAllEntities();

            yield return new WaitUntil(() =>
    MiniMapRenderer.Instance != null &&
    MiniMapRenderer.Instance.GetTexture() != null &&
    MiniMapRenderer.Instance.GetRenderedCellSize().y > 0 &&
    MiniMapRenderer.Instance.HasValidTilemap()
);
            float ppu = MiniMapRenderer.Instance.GetPixelsPerUnit();

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                MiniMapEntityTracker.Instance?.AddEntity(player.transform, Color.yellow);
            }

            foreach (var portal in GameObject.FindGameObjectsWithTag("Portal"))
            {
                MiniMapEntityTracker.Instance?.AddEntity(portal.transform, Color.green);
            }
        }

        playerService.EnablePlayer();
        yield return fader.FadeIn();
    }

}