using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BalloonTyper : MonoBehaviour
{
    [SerializeField] private List<GameObject> textLineObjects; // Text를 가진 오브젝트들
    [SerializeField] private float typingSpeed = 0.05f;

    public IEnumerator PlayTyping()
    {
        foreach (var obj in textLineObjects)
        {
            obj.SetActive(true); // ✅ 텍스트 오브젝트 활성화
            Text text = obj.GetComponent<Text>();
            if (text == null || !text.enabled)
                continue;

            string fullText = text.text;
            text.text = "";

            foreach (char c in fullText)
            {
                text.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }
}
