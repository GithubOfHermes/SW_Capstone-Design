using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingsRoomIntroManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> balloonList;
    [SerializeField] private GameObject playerObject;

    private void Start()
    {
        StartCoroutine(PlayMainIntro());
    }

    private IEnumerator PlayMainIntro()
    {

        var playerController = playerObject.GetComponent<PlayerController>();
        if (playerController != null) playerController.enabled = false;

        yield return new WaitForSeconds(2f);
        // 말풍선 순차 출력
        foreach (var balloon in balloonList)
        {
            balloon.SetActive(true);
            var typer = balloon.GetComponent<BalloonTyper>();
            if (typer != null)
            {
                yield return StartCoroutine(typer.PlayTyping());
            }
            yield return new WaitForSeconds(2f);
            balloon.SetActive(false);
        }

        SceneTransitionManager.Instance.LoadNextScene();
    }
}
