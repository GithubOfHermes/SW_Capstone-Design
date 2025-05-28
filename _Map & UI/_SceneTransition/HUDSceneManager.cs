using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class HUDSceneManager : MonoBehaviour, IHUDSceneManager
{
    private static bool hudLoaded = false;

    public Coroutine EnsureHUDLoaded()
    {
        return StartCoroutine(LoadHUD());
    }

    private IEnumerator LoadHUD()
    {
        if (!hudLoaded)
        {
            var op = SceneManager.LoadSceneAsync("HUD", LoadSceneMode.Additive);
            yield return op;

            // HUD 씬 루트 오브젝트에 DontDestroyOnLoad 적용
            var hudRoot = GameObject.FindWithTag("HUDCanvas");
            if (hudRoot != null)
            {
                DontDestroyOnLoad(hudRoot.transform.root.gameObject);
            }

            hudLoaded = true;
        }
    }
    public static void ResetHUDStatus()
    {
        hudLoaded = false;
    }
}

