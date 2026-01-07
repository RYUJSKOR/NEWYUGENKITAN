using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrbTearEffect : MonoBehaviour
{
    // オーブ（黄色い球体）
    public RectTransform orb;

    // 上下に裂けるパネル
    public RawImage topImage;
    public RawImage bottomImage;

    private RectTransform topPanel;
    private RectTransform bottomPanel;

    [Header("演出ディレイ設定")]
    [Tooltip("画像分離までの遅延時間（秒）")]
    public float delayBeforeSplit = 0.5f;

    // オーブの移動時間（長くすることで遅く）
    public float moveDuration = 4f;

    // パネルの移動距離（上下にどれだけ開くか）
    public float tearAmount = 300f;

    // パネルの回転角度（Z軸）
    public float maxRollAngle = 45f;

    // パネルの縮小率（縦方向）
    public float scaleShrinkFactor = 0.5f;

    // オーブの回転速度
    public float rotateSpeed = 90f;

    // オーブの開始・終了位置（UI基準）
    private Vector2 startPos = new Vector2(-960f, 0f);
    private Vector2 endPos = new Vector2(960f, 0f);

    private Coroutine splitCoroutine = null;

    private float elapsed = 0f;
    private bool isPlaying = false;
    private bool isSplitting = false;

    private void Start()
    {
        if (topImage != null) topPanel = topImage.rectTransform;
        if (bottomImage != null) bottomPanel = bottomImage.rectTransform;

        if (orb != null)
        {
            ResetEffectState();
        }
        else
        {
            Debug.Log("自殺");
        }
    }

    // アニメーション開始を呼び出す関数
    public void StartEffect()
    {

        if (splitCoroutine != null)
        {
            StopCoroutine(splitCoroutine);
            splitCoroutine = null;
        }

        startPos = new Vector2(-960f, 0f);
        endPos = new Vector2(960f, 0f);

        Debug.Log("自爆");
        elapsed = 0f;
        isPlaying = true;
        isSplitting = false;

        // 初期状態に戻す
        orb.anchoredPosition = startPos;
        orb.localRotation = Quaternion.identity;
        orb.gameObject.SetActive(true);
        Debug.Log("自己紹介1");

        topPanel.anchoredPosition = Vector2.zero;
        bottomPanel.anchoredPosition = Vector2.zero;
        Debug.Log("自己紹介2");

        topPanel.localRotation = Quaternion.identity;
        bottomPanel.localRotation = Quaternion.identity;
		Debug.Log("自己紹介3");

		topPanel.localScale = Vector3.one;
        bottomPanel.localScale = Vector3.one;

        // 表示をON
        
        topPanel.gameObject.SetActive(true);
        bottomPanel.gameObject.SetActive(true);

        splitCoroutine = StartCoroutine(DelaySplitStart());
    }

    private IEnumerator DelaySplitStart()
    {
        Debug.Log("自己自慢");
        yield return new WaitForSeconds(delayBeforeSplit);
        isSplitting = true;
    }

    void Update()
    {
        // オーブの移動が無効なら処理しない
        if (!isPlaying)
        {
            return;
        }
        else
        {
            Debug.Log("自身");
        }

        // 経過時間を加算（常にオーブは動かす）
        elapsed += Time.deltaTime;

        // 全体の進行度（0〜1）を滑らかに計算
        float t = Mathf.Clamp01(elapsed / moveDuration);
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // オーブを左から右へ移動させる
        orb.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothT);

        Debug.Log($"orb pos: {orb.anchoredPosition}, smoothT: {smoothT}");

        // オーブを回転させる
        orb.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        // ここから下は「遅れて」実行される画像の分離処理
        if (isSplitting)
        {
            Debug.Log("自身２");

            // オーブの位置に基づいて進行度を再計算（位置ベース）
            float orbT = Mathf.InverseLerp(startPos.x, endPos.x, orb.anchoredPosition.x);
            float splitT = Mathf.SmoothStep(0f, 1f, orbT);

            // 画像の上下移動（裂ける動き）
            float tearY = tearAmount * splitT;
            topPanel.anchoredPosition = new Vector2(0, tearY);
            bottomPanel.anchoredPosition = new Vector2(0, -tearY);

            // 画像の回転（ひねるような効果）
            float rollZ = maxRollAngle * splitT;
            topPanel.localRotation = Quaternion.Euler(0, 0, rollZ);
            bottomPanel.localRotation = Quaternion.Euler(0, 0, -rollZ);

            // 画像の縦方向スケーリング（圧縮効果）
            float shrinkY = 1f - scaleShrinkFactor * splitT;
            topPanel.localScale = new Vector3(1, shrinkY, 1);
            bottomPanel.localScale = new Vector3(1, shrinkY, 1);
        }

        // アニメーションが完了したら、シーン遷移を実行
        if (elapsed >= moveDuration)
        {
            isPlaying = false;
            isSplitting = false;
            SceneManager.LoadScene("Loading");
        }
    }

    // 演出を初期状態にリセットする関数
    public void ResetEffectState()
    {
        startPos = new Vector2(-960f, 0f);
        endPos = new Vector2(960f, 0f);

        Debug.Log("自Tiqkf");
        isPlaying = false;
        isSplitting = false;
        elapsed = 0f;

        if (orb != null)
        {
            orb.anchoredPosition = startPos;
            orb.localRotation = Quaternion.identity;
            orb.gameObject.SetActive(false); // 非表示にしておく（必要に応じて）
        }

        if (topPanel != null)
        {
            topPanel.anchoredPosition = Vector2.zero;
            topPanel.localRotation = Quaternion.identity;
            topPanel.localScale = Vector3.one;
            topPanel.gameObject.SetActive(false); // 非表示にしておく

            topImage.texture = null;
        }

        if (bottomPanel != null)
        {
            bottomPanel.anchoredPosition = Vector2.zero;
            bottomPanel.localRotation = Quaternion.identity;
            bottomPanel.localScale = Vector3.one;
            bottomPanel.gameObject.SetActive(false); // 非表示にしておく

            bottomImage.texture = null;
        }
    }
}
