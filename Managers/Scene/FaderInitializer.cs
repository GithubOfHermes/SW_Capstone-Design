using UnityEngine;

public static class FaderInitializer
{
    public static void TryCreateFader(GameObject faderPrefab)
    {
        if (FadeController.Instance == null && faderPrefab != null)
        {
            var fader = Object.Instantiate(faderPrefab);
            Object.DontDestroyOnLoad(fader);
        }
    }
}
