using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class ScreenTransitionManager : MonoBehaviour
{
    [SerializeField] private RawImage transitionImage; // 画面キャプチャを表示するRawImage
    [SerializeField] private OrbTearEffect orbEffect;  // オーブ演出を制御するスクリプト

    private Action onComplete; // アニメーション終了後のコールバック

    private void Start()
    {
        orbEffect = FindAnyObjectByType<OrbTearEffect>();
    }

    // トランジション演出を開始する関数（Animatorは使用しない）
    public IEnumerator PlayTransition(Action onCompleteCallback)
    {


        // フレームの終わりで画面キャプチャを取得
        yield return new WaitForEndOfFrame();

        // 現在の画面をキャプチャし、Texture2Dとして取得
        Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

        // 画面キャプチャを上下のRawImageに設定
        orbEffect.topImage.texture = tex;
        orbEffect.bottomImage.texture = tex;

        // RawImageに適切なUV Rectを設定（上半分／下半分）
        orbEffect.topImage.uvRect = new Rect(0f, 0.5f, 1f, 0.5f);      // 위쪽
        orbEffect.bottomImage.uvRect = new Rect(0f, 0f, 1f, 0.5f);     // 아래쪽

        // コールバックを保存
        onComplete = onCompleteCallback;

        // 裂け演出を開始
        orbEffect.StartEffect();
    }

    // オーブ演出の完了時に呼び出される関数
    private void OnEffectComplete()
    {
        // キャプチャ画像を非表示にする
        transitionImage.gameObject.SetActive(false);

        // コールバックを実行（例：シーン遷移）
        onComplete?.Invoke();
    }

    public void RequestSceneTransition(string stageSceneName, int stageNumber)
    {
        // ステージ情報を保存
        PlayerPrefs.SetString("SelectedStage", stageSceneName);
        PlayerPrefs.SetInt("SelectedStageNumber", stageNumber);
        PlayerPrefs.Save();

        // トランジション演出を開始し、ローディングシーンに移動
        StartCoroutine(PlayTransition(() =>
        {
            Debug.Log("<color=yellow>--- 2. 画面演出完了！今から `SceneManager.LoadScene` を呼び出します ---</color>");
            SceneManager.LoadScene("Loading");
        }));
    }

}
