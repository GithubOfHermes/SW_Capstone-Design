using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneIntroManager : MonoBehaviour
{
    [Header("연출 구성 요소")]
    [SerializeField] private CameraZoomController cameraZoom;
    [SerializeField] private EnemyIntroManager enemyManager;
    [SerializeField] private IntroTextController textController;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject cinemachineCamera;
    [SerializeField] private List<GameObject> balloonList;
    private HUDIntroController hudController;
    private bool introPlayed = false;

    public void RegisterHUD(HUDIntroController hud)
    {
        hudController = hud;
    }

    private void Awake()
    {
        if (cinemachineCamera != null)
            cinemachineCamera.SetActive(false); // 인트로 중 비활성화

        if (playerObject != null)
            playerObject.GetComponent<PlayerController>().enabled = false;
    }

    private void Start()
    {
        if (!introPlayed)
        {
            StartCoroutine(PlayIntroSequence());
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        yield return new WaitForSeconds(1f);

        // 1. 첫 말풍선
        yield return ShowBalloonWithTyping(0);

        // 2. 플레이어 회전
        playerObject.transform.position = new Vector3(1f, -1.88f, -1f);
        playerObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        yield return new WaitForSeconds(0.5f);

        // 3. 두 번째 말풍선
        yield return ShowBalloonWithTyping(1);

        // 4. 공격 애니메이션
        var pc = playerObject.GetComponent<PlayerController>();
        playerObject.GetComponent<PlayerController>().enabled = true;
        yield return new WaitForEndOfFrame(); // Animator가 세팅되도록 프레임 하나 넘김
        if (pc != null)
        {
            yield return pc.StartCoroutine("PlayAttackAnimation", "Attack");
            pc.ForceIdleAndStopDash();
        }
        playerObject.GetComponent<PlayerController>().enabled = false;

        // 5. 적 애니메이션 Die
        yield return enemyManager.DisableEnemiesSequentially();

        yield return new WaitForSeconds(1f);

        // 6. 세 번째 말풍선
        yield return ShowBalloonWithTyping(2);

        // 7. 카메라 줌 아웃
        yield return cameraZoom.ZoomOut();

        // 8. HUD 및 조작 활성화
        hudController.ShowHUD();

        SceneTransitionManager.Instance.playerService.EnablePlayer();
        playerObject.GetComponent<PlayerController>().enabled = true;

        if (cinemachineCamera != null)
            cinemachineCamera.SetActive(true);
        Time.timeScale = 1f;
        introPlayed = true;
    }

    private IEnumerator ShowBalloonWithTyping(int index)
    {
        if (index < 0 || index >= balloonList.Count) yield break;

        GameObject balloon = balloonList[index];
        balloon.SetActive(true);

        var typer = balloon.GetComponent<BalloonTyper>();
        if (typer != null)
            yield return typer.PlayTyping();

        yield return new WaitForSeconds(1.5f);
        balloon.SetActive(false);
    }

}
