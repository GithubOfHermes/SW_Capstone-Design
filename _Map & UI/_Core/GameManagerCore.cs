using UnityEngine;
using System.Collections;

public class GameManagerCore : MonoBehaviour
{
    public static GameManagerCore Instance { get; private set; }

    [SerializeField] private GameObject sceneFaderPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameStartupInitializer.Initialize();
            FaderInitializer.TryCreateFader(sceneFaderPrefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene != "0 Main" &&
        currentScene != "0-0 King's Room" &&
        currentScene != "0-1 Bedroom")
        {
            StartCoroutine(LoadHUDIfNeeded());
        }
    }

    private IEnumerator LoadHUDIfNeeded()
    {
        // HUD 씬이 없다면 로드
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.hudManager != null)
        {
            yield return SceneTransitionManager.Instance.hudManager.EnsureHUDLoaded();
        }

        // HUDCanvas가 로딩되기를 기다림 (선택)
        yield return WaitForHUDCanvas();
    }

    private IEnumerator WaitForHUDCanvas()
    {
        GameObject found = null;
        float t = 0f;
        while (found == null && t < 3f)
        {
            found = GameObject.FindWithTag("HUDCanvas");
            t += Time.deltaTime;
            yield return null;
        }

        if (found == null)
        {
            Debug.LogWarning("HUDCanvas가 3초 내에 발견되지 않았습니다.");
        }
    }

    public static void ForceReset()
    {
        Instance = null;
    }
}