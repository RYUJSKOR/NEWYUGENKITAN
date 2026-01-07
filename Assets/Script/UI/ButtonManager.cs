using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

public class ButtonManager : MonoBehaviour
{
    private PlayerStateMachine playerstate;
    private BulletSkill bulletskill;
    private InputSystem_PlayerActions inputAction;
    private PauseManager _pauseManager;
    BossTimerManager bossTimerManager;
    private GameData gameData;
    private TitleOptionManager titleOptionManager;

    // ローディングシーンの名前
    private string loadingSceneName = "Loading";

    void Start()
    {
        inputAction = new InputSystem_PlayerActions();
        _pauseManager = FindAnyObjectByType<PauseManager>();

        bossTimerManager = FindAnyObjectByType<BossTimerManager>();

        playerstate = FindAnyObjectByType<PlayerStateMachine>();

        gameData = FindAnyObjectByType<GameData>();

        titleOptionManager = FindAnyObjectByType<TitleOptionManager>();

        if (playerstate != null)
        {
            bulletskill = playerstate.GetStateByBaseClass<BulletSkill>();
        }
        else
        {
            Debug.Log("ステートマシンNULL"+playerstate);
        }
    }

    void Update()
    {
    }

    public void OnOptionWindow()
    {
        _pauseManager.ShowPause();
    }

    public void OnStage1Button()
    {
        OnStageButtonClicked("RealScene1", 1);
    }

    public void OnStage2Button()
    {
        OnStageButtonClicked("RealScene2", 2);
    }

    //public void OnStage3Button()
    //{
        //OnStageButtonClicked("RealScene3", 3);
    //}

    //public void OnStage4Button()
    //{
        //OnStageButtonClicked("RealScene4", 4);
    //}

    // ステージボタンが押されたときに呼び出される関数
    public void OnStageButtonClicked(string stageSceneName, int stageNumber)
    {
        Debug.Log((stageSceneName,stageNumber));
        // ScreenTransitionManagerの演出を使う場合
        var transitionManager = FindObjectOfType<ScreenTransitionManager>();
        if (transitionManager != null)
        {
            // 演出を開始し、演出が終わったらSceneLoadManagerを呼び出す
            StartCoroutine(transitionManager.PlayTransition(() =>
            {
                // 司令塔に次のシーンを伝え、ローディングを開始させる
                SceneLoadManager.Instance.LoadScene(stageSceneName);
            }));
        }
        else
        {
            // 演出がない場合は直接司令塔を呼び出す
            Debug.LogWarning("ScreenTransitionManager が見つかりません。直接ロードします。");
            SceneLoadManager.Instance.LoadScene(stageSceneName);
        }

        // PlayerPrefsへの保存は、ステージ番号など、
        // ステージシーン側で必要になる情報だけ残しても良いでしょう。
        PlayerPrefs.SetInt("SelectedStageNumber", stageNumber);
        PlayerPrefs.Save();
    }

    // ゲームを終了する
    public void ExitGame()
    {
        Application.Quit();
    }

    // ローディングシーンに移動
    public void GoToLoadingSecene()
    {
        SceneManager.LoadScene("Loading");
    }

    // タイトル画面に戻る
    public void BackToTitle()
    {
        if (GameData.Instance != null)
            GameData.Instance.ResetAll();

        if (bulletskill != null)
        {
            bulletskill.SetGauge(0f);
        }

        if (BossGameManager.Instance != null)
        {
            BossGameManager.Instance.ResetSavedData();
        }

        if (gameData != null && bossTimerManager != null)
        {
            gameData.saveBossTime(0.0f);
            bossTimerManager.ResetTimer(0.0f);
        }

        if (bossTimerManager != null)
        {
            bossTimerManager.StopTimer(); // 最終時間を保存
            Destroy(bossTimerManager.gameObject); // タイマーオブジェクトを削除
        }


        SceneManager.LoadScene("Title");
    }

    // ステージ選択画面に移動
    public void GoToSceneSelecter()
    {
        SceneManager.LoadScene("SceneSelecter");
    }

    // ゲームを再開する
    public void ResumeGame()
    {
        StartCoroutine(ResumeWithDelay());
    }

    private IEnumerator ResumeWithDelay()
    {
        yield return null; // 1フレーム待機
        Time.timeScale = 1f;
        _pauseManager.pausePanel.SetActive(false);
        _pauseManager.isPaused = false;
    }

    // オプション画面を開く
    public void OpenOptionPanel()
    {
        _pauseManager.pausePanel.SetActive(false);
        _pauseManager.OptionPanel.SetActive(true);
    }

    public void OpenTitleOptionPanel()
    {
        titleOptionManager.ToggleOption();
    }

    // ゲーム画面に戻る
    public void BackToStage()
    {
        // 選択されたステージ名を取得
        string stageName = PlayerPrefs.GetString("SelectedStage", "Scene");

        if (BossGameManager.Instance != null)
        {
            BossGameManager.Instance.ResetSavedData();
        }
        else Debug.Log("ボスゲームマネージャーなんかねーよ");

        if (gameData != null && bossTimerManager != null)
        {
            gameData.saveBossTime(0.0f);
            bossTimerManager.ResetTimer(0.0f);
        }

        if (bossTimerManager != null)
        {
            bossTimerManager.StopTimer(); // 最終時間を保存
            Destroy(bossTimerManager.gameObject); // タイマーオブジェクトを削除
        }


        if (bulletskill != null)
        { 
            bulletskill.SetGauge(0f);
        }
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(stageName);
    }
}
