using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingUIController : MonoBehaviour
{
    [SerializeField] private Slider progress;
    [SerializeField] private CanvasGroup fadeCanvas;

    public void Show()
    {
        gameObject.SetActive(true);
        fadeCanvas.alpha = 0;
    }

    public void SetProgress(float value)
    {
        if (progress) progress.value = value;
    }

    public IEnumerator FadeOut(float time = 0.5f)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0, 1, t / time);
            yield return null;
        }
    }
}
