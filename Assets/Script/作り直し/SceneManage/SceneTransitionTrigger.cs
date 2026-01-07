using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Target Scene Name")]
    [SceneSelector]
    [SerializeField] private string targetScene;

    [Header("Trigger Mode")]
    [SerializeField] private bool triggerOnButtonClick = true;
    [SerializeField] private string targetTag = "Player";

    public void OnClickTransition()
    {
        if (!triggerOnButtonClick) return;
        RequestScene();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnButtonClick) return;

        if (other.CompareTag(targetTag))
        {
            RequestScene();
        }
    }

    private void RequestScene()
    {
        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneFlowController.Instance.RequestScene(targetScene);
        }
        else
        {
            Debug.LogError("TargetScene Ç™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅI");
        }
    }
}
