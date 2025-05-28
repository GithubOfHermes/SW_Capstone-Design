using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[System.Serializable]
public class BGMClip
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
}

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;
    public AudioSource bgmSource;

    [Header("BGM Clips & Volumes")]
    public BGMClip mainBGM;
    public BGMClip startBGM;
    public BGMClip normalBGM;
    public BGMClip bossBGM;
    public BGMClip endBGM;

    private bool bossTriggered = false;
    private bool bossDefeated = false;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bossTriggered = false;
        bossDefeated = false;

        string name = scene.name;

        if (name == "0 Main")
            FadeToBGM(mainBGM);
        else if (name.StartsWith("0-0 King's Room") || name.StartsWith("0-1 Bedroom"))
            FadeToBGM(startBGM);
        else if (name.StartsWith("1-1 Bedroom") || name.StartsWith("1-2 Hall") || name.StartsWith("1-3 Elite") || name.StartsWith("2 Way"))
            FadeToBGM(normalBGM);
        else if (name == "3 Boss Room")
            FadeOutAndStop();
        else if (name == "4 Fin")
            FadeToBGM(endBGM);
    }

    public void FadeToBGM(BGMClip newBGM, float fadeTime = 0.3f)
    {
        if (newBGM == null || newBGM.clip == null) return;
        if (bgmSource.clip == newBGM.clip && bgmSource.isPlaying) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeBGM(newBGM, fadeTime));
    }

    private IEnumerator FadeBGM(BGMClip newBGM, float fadeTime)
    {
        float startVolume = bgmSource.volume;

        while (bgmSource.volume > 0f)
        {
            bgmSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newBGM.clip;
        bgmSource.Play();

        while (bgmSource.volume < newBGM.volume)
        {
            bgmSource.volume += newBGM.volume * Time.deltaTime / fadeTime;
            yield return null;
        }

        bgmSource.volume = newBGM.volume;
    }

    public void FadeOutAndStop(float fadeTime = 1f)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutThenStop(fadeTime));
    }

    private IEnumerator FadeOutThenStop(float fadeTime)
    {
        float startVolume = bgmSource.volume;

        while (bgmSource.volume > 0f)
        {
            bgmSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        bgmSource.Stop();
    }

    public void StopBGM()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        bgmSource.Stop();
    }

    public void TriggerBossBGM()
    {
        if (!bossTriggered)
        {
            bossTriggered = true;
            FadeToBGM(bossBGM);
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }
}
