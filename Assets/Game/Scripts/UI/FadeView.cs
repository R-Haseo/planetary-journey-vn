using System;
using System.Collections;
using UnityEngine;

public class FadeView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine fadeCoroutine;

    public void FadeOut(float duration, Action onCompleted = null)
    {
        StartFade(1f, duration, onCompleted);
    }

    public void FadeIn(float duration, Action onCompleted = null)
    {
        StartFade(0f, duration, onCompleted);
    }

    private void StartFade(float targetAlpha, float duration, Action onCompleted)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha, duration, onCompleted));
    }

    private IEnumerator FadeCoroutine(
        float targetAlpha,
        float duration,
        Action onCompleted)
    {
        var startAlpha = canvasGroup.alpha;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            var progress = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                progress);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        fadeCoroutine = null;

        onCompleted?.Invoke();
    }
}
