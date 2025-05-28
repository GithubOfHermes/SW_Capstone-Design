using UnityEngine;

public class IntroTextController : MonoBehaviour
{
    [SerializeField] private GameObject introTextUI;

    private void Awake()
    {
        if (introTextUI != null)
            introTextUI.SetActive(false);
    }

    public void ShowIntroText()
    {
        if (introTextUI != null)
            introTextUI.SetActive(true);
    }
}