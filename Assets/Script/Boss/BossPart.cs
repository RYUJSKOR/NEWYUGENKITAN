using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BossPartFlash : MonoBehaviour
{
    private Coroutine flashCoroutine;
    private Renderer[] renderers;
    private MaterialPropertyBlock propBlock; // BossControllerと統一

    [Header("点滅設定")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;

    [Header("シェーダーの色のプロパティ名")]
    [Tooltip("CircleFadeシェーダーの場合は _MainColor のままにしてください")]
    [SerializeField] private string colorPropertyName = "_MainColor"; // BossControllerと統一

    [Header("除外設定")]
    [Tooltip("ダメージ点滅から除外するRenderer（仮面など）")]
    [SerializeField] private List<Renderer> excludeRenderers = new List<Renderer>();

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        // 1. まず全ての子Rendererを取得
        List<Renderer> allRenderers = GetComponentsInChildren<Renderer>().ToList();

        // 2. 除外リストが設定されていれば、それらの要素をリストから削除
        if (excludeRenderers != null && excludeRenderers.Count > 0)
        {
            // LINQのExceptを使用して、除外リストに含まれるものを除外する
            renderers = allRenderers.Except(excludeRenderers).ToArray();
        }
        else
        {
            // 除外リストが空なら、そのまま全てを配列に
            renderers = allRenderers.ToArray();
        }

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning("BossPartFlash: 対象のRendererが見つかりません。", this);
        }
    }

    /// <summary>
    /// 指定された色（revertColor）に復帰することを前提として、赤く点滅します。
    /// </summary>
    /// <param name="revertColor">点滅後に戻す色（現在のフェーズの色）</param>
    public void FlashRed(Color revertColor)
    {
        if (renderers == null || renderers.Length == 0) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashCoroutine(revertColor));
    }

    private IEnumerator FlashCoroutine(Color revertColor)
    {
        // 1. フラッシュ色（赤）に変更
        propBlock.SetColor(colorPropertyName, flashColor);
        foreach (var rend in renderers)
        {
            rend.SetPropertyBlock(propBlock);
        }

        yield return new WaitForSeconds(flashDuration);

        // 2. 元の色（引数で受け取ったフェーズの色）に戻す
        propBlock.SetColor(colorPropertyName, revertColor);
        foreach (var rend in renderers)
        {
            rend.SetPropertyBlock(propBlock);
        }

        flashCoroutine = null;
    }

    /// <summary>
    /// 点滅を強制停止し、指定された色（通常フェーズの色）に戻します。
    /// </summary>
    public void ResetFlash(Color revertColor)
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (renderers == null || renderers.Length == 0) return;

        propBlock.SetColor(colorPropertyName, revertColor);
        foreach (var rend in renderers)
        {
            rend.SetPropertyBlock(propBlock);
        }
    }

    // ▼▼▼ BossControllerから呼ばれるResetAllFlashと名前が競合していたので、
    // ▼▼▼ こちらの古いResetFlash()は削除しました。
    // public void ResetFlash() { ... }
}