using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour, ISceneLoader
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNextScene()
    {
        int index = SceneManager.GetActiveScene().buildIndex + 1;
        if (index < SceneManager.sceneCountInBuildSettings)
        {
            string next = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(index));
            LoadScene(next);
        }
    }
}
