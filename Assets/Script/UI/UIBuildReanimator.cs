using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// ---------------- 目的 ----------------
// ビルド時に「このUIだけ見えない」を強制的に可視化する応急パッチ
// ・親チェーンの CanvasGroup/Mask を可視状態に補正
// ・子Canvasを作り Override Sorting=ON, Order=10000 で最前面描画
// ・Graphic(Image/RawImage/Text等) の有効化/カラーα=1 を強制
// ・Image/RawImage で Sprite/Texture が null の場合は白ダミーを差し込む
// ・エラーマテリアル/未知シェーダはデフォルトUI相当にフォールバック
// ※ 問題のUIルートにだけ一時的にアタッチしてください

[DefaultExecutionOrder(9999)] // できるだけ遅く実行して他の初期化より後で上書き
public class UIBuildReanimator : MonoBehaviour
{
    // ← 何フレーム強制上書きするか（初期化で消されても上書き勝ちする）
    [SerializeField] int frames = 45; // およそ0.75秒@60fps

    Canvas _forceCanvas;
    Sprite _whiteSpriteCache;

    void Awake()
    {
        // 1) 親チェーンを可視状態に補正（Active/CanvasGroup/Mask）
        Transform t = transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);

            var cg = t.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;            // 完全不透明
                cg.interactable = true;   // 入力可
                cg.blocksRaycasts = true;
            }

            var mask = t.GetComponent<Mask>();
            if (mask) mask.enabled = false;

            var rmask = t.GetComponent<RectMask2D>();
            if (rmask) rmask.enabled = false;

            t = t.parent;
        }

        // 2) 子Canvasを作って最前面描画（他のUIに隠されないように）
        var go = new GameObject("UIBuildReanimator_ForceCanvas");
        go.transform.SetParent(transform, false);
        _forceCanvas = go.AddComponent<Canvas>();
        _forceCanvas.overrideSorting = true;
        _forceCanvas.sortingOrder = 10000;
        go.AddComponent<GraphicRaycaster>();

        // 3) 何フレームか連続で「見える状態」を上書き
        StartCoroutine(ForceVisibleForFrames(frames));
    }

    IEnumerator ForceVisibleForFrames(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ForceOnce();
            yield return null; // 次のフレームまで待つ
        }
    }

    void ForceOnce()
    {
        // 3-1) この枝の全Graphicを可視化（enabled/α=1）
        var graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
        {
            if (!g.enabled) g.enabled = true;
            var c = g.color; c.a = 1f; g.color = c;

            // マテリアルが内部エラー系ならデフォルトに戻す
            if (g.material != null && g.material.shader != null &&
                g.material.shader.name == "Hidden/InternalErrorShader")
            {
                g.material = null; // UGUIはnullでデフォルトUIマテリアル相当
            }
        }

        // 3-2) Image/RawImage のスプライト/テクスチャ欠落を白ダミーで補完
        var images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.sprite == null)
            {
                img.sprite = GetWhiteSprite();
                img.type = Image.Type.Simple;
                img.preserveAspect = false;
            }
            // 透明化を防ぐため、マテリアルはデフォルトへ
            if (img.material != null && img.material.shader != null &&
                img.material.shader.name == "Hidden/InternalErrorShader")
            {
                img.material = null;
            }
        }

        var raws = GetComponentsInChildren<RawImage>(true);
        foreach (var ri in raws)
        {
            if (ri.texture == null)
            {
                ri.texture = Texture2D.whiteTexture;
            }
            if (ri.material != null && ri.material.shader != null &&
                ri.material.shader.name == "Hidden/InternalErrorShader")
            {
                ri.material = null;
            }
            var c = ri.color; c.a = 1f; ri.color = c;
        }

        // 3-3) 極端なスケール/位置ずれの最小補正
        var rts = GetComponentsInChildren<RectTransform>(true);
        foreach (var rt in rts)
        {
            // スケールがゼロなら1に補正
            if (Mathf.Approximately(rt.localScale.x, 0f) || Mathf.Approximately(rt.localScale.y, 0f))
                rt.localScale = Vector3.one;
        }
    }

    // 真っ白のダミースプライトを一度だけ生成
    Sprite GetWhiteSprite()
    {
        if (_whiteSpriteCache != null) return _whiteSpriteCache;
        var tex = Texture2D.whiteTexture;
        _whiteSpriteCache = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        return _whiteSpriteCache;
    }
}
