using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RestartGameManager : MonoBehaviour
{
    public static void Restart(string sceneName = "0 Main")
    {
        var go = new GameObject("RestartGameManager");
        DontDestroyOnLoad(go);
        var mgr = go.AddComponent<RestartGameManager>();
        mgr.StartCoroutine(mgr.RestartGameCoroutine(sceneName));
    }

    private IEnumerator RestartGameCoroutine(string sceneName)
    {
        // 1. 씬 로드
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        // 2. 씬 로딩 완료까지 대기
        yield return null;

        // 3. HUD 관련 static 초기화
        HUDManager.Instance = null;
        HUDSceneManager.ResetHUDStatus();

        // 4. HUD 오브젝트만 제거
        DestroyOnlyHUD();

        if (FadeController.Instance != null)
        {
            yield return FadeController.Instance.FadeIn();
        }

        // 5. 이 매니저도 제거
        Destroy(gameObject);
    }

    private void DestroyOnlyHUD()
    {
        var allRoots = GameObject.FindObjectsOfType<GameObject>(true);
        foreach (var go in allRoots)
        {
            if (go.scene.name == "DontDestroyOnLoad")
            {
                string name = go.name;

                if (name.Contains("Canvas"))
                {
                    Destroy(go);
                }
            }
        }
    }
}
