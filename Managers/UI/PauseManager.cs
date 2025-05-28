using UnityEngine;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    private bool isPaused = false;

    private void Update()
    {
        if (SceneTransitionManager.Instance == null) return;

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "0 Main") return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (FadeController.Instance != null)
        {
            StartCoroutine(FadeOutAndRestart());
        }
        else
        {
            RestartGameManager.Restart("0 Main");
        }
    }

    private IEnumerator FadeOutAndRestart()
    {
        yield return FadeController.Instance.FadeOut();
        RestartGameManager.Restart("0 Main");
    }

    public void ToggleSound()
    {
        AudioListener.pause = !AudioListener.pause;
    }
}
