using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour, IFadeController
{
    public static FadeController Instance { get; private set; } // ✅ 싱글톤 추가

    [SerializeField] private Image fadeImage;
    [SerializeField] private float duration = 1f;
    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ 씬 간 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Coroutine FadeOut() => StartCoroutine(Fade(0f, 1f));
    public Coroutine FadeIn() => StartCoroutine(Fade(1f, 0f));

    private IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null)
        {
            Debug.LogError("[FadeController] fadeImage가 연결되어 있지 않습니다!");
            yield break;
        }

        float time = 0f;
        Color color = fadeImage.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / duration);
            color.a = Mathf.Lerp(from, to, progress);
            fadeImage.color = color;
            yield return null;
        }

        color.a = to;
        fadeImage.color = color;
    }
}
