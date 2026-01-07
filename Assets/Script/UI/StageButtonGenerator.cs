using UnityEngine;
using UnityEngine.UI;

public class StageButtonGenerator : MonoBehaviour
{
    // 
    public GameObject stageButtonPrefab;

    // 
    public Transform buttonParent;

    // 
    public int totalStages = 10;

    private void Start()
    {
        for (int i = 1; i <= totalStages; i++)
        {
            GameObject buttonObj = Instantiate(stageButtonPrefab, buttonParent);
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();

            string sceneName = (i == 1) ? "Scene" : "Scene" + i;
            buttonText.text = "Stage " + i;

            bool isUnlocked = (i == 1 || PlayerPrefs.GetInt("RealScene" + i + "_Unlocked", 0) == 1);
            button.interactable = isUnlocked;

            int stageIndex = i;
            button.onClick.AddListener(() => {
                PlayerPrefs.SetString("SelectedStage", sceneName);
                PlayerPrefs.Save();
                UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
            });
        }
    }
}
