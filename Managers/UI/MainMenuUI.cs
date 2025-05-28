using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Image[] buttonImages;
    private Color originalColor = Color.white;
    private Color pressedColor = new Color(100f / 255f, 100f / 255f, 100f / 255f);

    public void OnButtonDown(Image target)
    {
        target.color = pressedColor;
    }

    public void OnButtonUp(Image target)
    {
        target.color = originalColor;
    }

    public void OnClickStart()
    {
        SceneTransitionManager.Instance.LoadNextScene();
    }

    public void OnClickQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}