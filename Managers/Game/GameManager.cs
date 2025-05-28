using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void TakeDamage(int amount)
    {
        if (HUDManager.Instance != null)
        {
            HUDUtil.ReduceHP(amount);
        }
    }

    public void GoToNextScene()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadNextScene();
        }
    }
}
