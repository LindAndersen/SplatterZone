using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BloodPulse : MonoBehaviour
{
    public Image bloodImage;
    public float fadeInTime = 0.1f;
    public float fadeOutTime = 0.3f;
    public float maxAlpha = 0.75f;

    CanvasGroup canvasGroup;
    RectTransform rectTransform;
    Coroutine pulseRoutine;

    void Awake()
    {
        // Image holen, falls nicht gesetzt
        if (bloodImage == null)
            bloodImage = GetComponent<Image>();

        // CanvasGroup holen oder anlegen
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();

        ApplyFullscreenStretch();

        // Start komplett unsichtbar
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    void ApplyFullscreenStretch()
    {
        if (rectTransform == null) return;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    public void Pulse()
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    IEnumerator PulseRoutine()
    {
        float t = 0f;

        // Fade IN
        while (t < fadeInTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, maxAlpha, t / fadeInTime);
            t += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = maxAlpha;

        t = 0f;

        // Fade OUT
        while (t < fadeOutTime)
        {
            canvasGroup.alpha = Mathf.Lerp(maxAlpha, 0f, t / fadeOutTime);
            t += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;

        pulseRoutine = null;
    }
}
