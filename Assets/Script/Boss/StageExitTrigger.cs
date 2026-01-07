using UnityEngine;

public class StageExitTrigger : MonoBehaviour
{
    [Tooltip("次にロードするシーンの名前")]
    [SceneSelector] 
    public string nextSceneName;

    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasBeenTriggered && other.CompareTag("Player"))
        {
            hasBeenTriggered = true;
            Debug.Log(nextSceneName + " への遷移を開始します。");

            BossGameManager.Instance.GoToNextStage(nextSceneName);
        }
    }
}