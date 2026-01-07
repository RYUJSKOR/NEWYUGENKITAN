using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    private ButtonManager _buttonManager;

    public GameObject pausePanel;

    public GameObject OptionPanel;

    public bool isPaused = false;

    public SelectManager SelectManager;

    public GameObject[] mainSceneButtons;

    // 
    private InputAction pauseOpenAction;

    // 
    private InputAction pauseCloseAction;

    private InputAction OptionCloseAction;


    private void Awake()
    {
        // ポズ画面開き入力
        pauseOpenAction = new InputAction("PauseOpen", binding: "<Keyboard>/escape");
        pauseOpenAction.AddBinding("<Gamepad>/start");
        pauseOpenAction.Enable();

        // ポズ画面閉じ入力
        pauseCloseAction = new InputAction("PauseClose", binding: "<Keyboard>/escape");
        pauseCloseAction.AddBinding("<Gamepad>/start");
        pauseCloseAction.Enable();

        // オプションパンネル閉じ入力
        OptionCloseAction = new InputAction("OptionClose", binding: "<Keyboard>/escape");
        OptionCloseAction.AddBinding("<Gamepad>/B");
        OptionCloseAction.Enable();
    }

    private void Start()
    {
        // ボタンマネージャを探す
        _buttonManager = FindAnyObjectByType<ButtonManager>();
    }

    void Update()
    {
        // 
        if (!isPaused && pauseOpenAction.triggered)
        {
            ShowPause();
        }

        else if (isPaused && OptionPanel.activeSelf && OptionCloseAction.triggered)
        {
            OptionPanel.SetActive(false);
            pausePanel.SetActive(true);
        }
        // 
        else if (isPaused && pauseCloseAction.triggered)
        {
            _buttonManager.ResumeGame();
        }
    }

    public void ShowPause()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        isPaused = true;

    }

}
