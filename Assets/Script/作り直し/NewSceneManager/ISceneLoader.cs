using UnityEngine;

public interface ISceneLoader
{
    Coroutine LoadScene(string sceneName);
}
