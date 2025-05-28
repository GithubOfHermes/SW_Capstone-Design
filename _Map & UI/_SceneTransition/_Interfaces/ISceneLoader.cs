using UnityEngine;

public interface ISceneLoader
{
    void LoadScene(string sceneName);
    void LoadNextScene();
}
