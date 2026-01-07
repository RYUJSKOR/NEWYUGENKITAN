using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AsyncSceneLoader : MonoBehaviour, ISceneLoader
{
    public LoadingUIController UI;

    public Coroutine LoadScene(string sceneName)
    {
        return StartCoroutine(LoadProcess(sceneName));
    }

    public static IEnumerator Load(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;
    }

    private IEnumerator LoadProcess(string sceneName)
    {
        UI?.Show();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            UI?.SetProgress(op.progress / 0.9f);
            yield return null;
        }

        UI?.SetProgress(1f);

        yield return UI?.FadeOut();
        op.allowSceneActivation = true;
    }
}
