using UnityEngine;

public class HUDIntroController : MonoBehaviour
{
    private GameObject hudCanvas;
    [SerializeField] private GameObject infoUI; // ✅ 인스펙터에 할당
    private void Awake()
    {
        hudCanvas = GameObject.FindWithTag("HUDCanvas");
    }
    void Start()
    {
        var intro = FindAnyObjectByType<SceneIntroManager>();

        if (intro != null)
        {
            intro.RegisterHUD(this);
            HideHUD();
        }
    }

    public void HideHUD()
    {
        if (hudCanvas != null)
            hudCanvas.SetActive(false);
    }

    public void ShowHUD()
    {
        if (hudCanvas != null)
            hudCanvas.SetActive(true);
    }

    public void ShowInfoUI() // ✅ 이 함수 새로 추가
    {
        if (infoUI != null)
            infoUI.SetActive(true);
    }
}