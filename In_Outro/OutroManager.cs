using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutroManager : MonoBehaviour
{
    [Header("카메라/연출")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeDuration = 1f;

    [Header("조명")]
    [SerializeField] private GameObject light1;
    [SerializeField] private GameObject light2;

    [Header("등장 객체")]
    [SerializeField] private GameObject monster;
    [SerializeField] private GameObject npc;

    [Header("말풍선")]
    [SerializeField] private List<GameObject> balloonList;

    [Header("카메라 이동")]
    [SerializeField] private List<Vector3> cameraPositions;
    [SerializeField] private float moveDuration = 2f;

    [Header("엔딩 텍스트")]
    [SerializeField] private BalloonTyper typer;
    [SerializeField] private BalloonTyper finalTyper;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        StartCoroutine(PlayOutroSequence());
    }

    private IEnumerator PlayOutroSequence()
    {
        // 1. 첫 말풍선
        yield return ShowBalloon(0);

        yield return ShowBalloon(1);

        // 2. 카메라 흔들림 + 페이드아웃
        StartCoroutine(CameraShake(shakeDuration, shakeIntensity));
        yield return FadeController.Instance.FadeOut();

        // 3. 조명 전환 + 몬스터/NPC 전환
        light1.SetActive(false);
        light2.SetActive(true);
        monster.SetActive(false);
        npc.SetActive(true);

        // 4. 페이드인 후 말풍선
        yield return FadeController.Instance.FadeIn();
        yield return ShowBalloon(2);

        yield return new WaitForSeconds(1f);

        // 5. 카메라 이동 순차 수행
        foreach (var targetPos in cameraPositions)
        {
            mainCamera.transform.position = targetPos;
            yield return new WaitForSeconds(2f);
        }

        // 6. 말풍선 2개 연속 출력
        yield return ShowBalloon(3);
        yield return ShowBalloon(4);

        // 7. 카메라 서서히 위로 이동
        yield return StartCoroutine(PanCameraUp(30f));

        // 8. 마지막 텍스트 타이핑
        if (typer != null)
            yield return typer.PlayTyping();
        yield return new WaitForSeconds(2f);
        typer.gameObject.SetActive(false);
        if (finalTyper != null)
            yield return finalTyper.PlayTyping();

        yield return new WaitForSeconds(3f);
        RestartGameManager.Restart("0 Main");
    }

    private IEnumerator ShowBalloon(int index)
    {
        if (index < 0 || index >= balloonList.Count) yield break;

        var balloon = balloonList[index];
        balloon.SetActive(true);

        var typer = balloon.GetComponent<BalloonTyper>();
        if (typer != null)
            yield return typer.PlayTyping();

        yield return new WaitForSeconds(2f);
        balloon.SetActive(false);
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCamera.transform.position = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalPos;
    }

    private IEnumerator PanCameraUp(float yOffset)
    {
        Vector3 start = mainCamera.transform.position;
        Vector3 target = start + new Vector3(0f, yOffset, 0f);
        float elapsed = 0f;
        float duration = 5f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = t * t; // ⬅ Ease-In: 느리게 시작해서 빨라짐
            mainCamera.transform.position = Vector3.Lerp(start, target, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }


        mainCamera.transform.position = target;
    }
}
