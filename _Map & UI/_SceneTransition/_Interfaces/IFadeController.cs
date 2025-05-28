using UnityEngine;

public interface IFadeController
{
    Coroutine FadeIn();
    Coroutine FadeOut();
}
