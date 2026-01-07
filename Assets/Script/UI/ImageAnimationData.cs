using UnityEngine;
using UnityEngine.UI; // Imageを使うために必要

[System.Serializable]
public class ImageAnimationData
{
    // ▼ ここをTextからImageに変更しました
    public Image imageObject;

    [Tooltip("次の画像を表示するまでの待機時間")]
    public float displayInterval = 0.5f;

    [Header("Scaling Animation")]
    public bool shouldAnimateScale = true;
    public float animationDuration = 0.5f;
    public float startScaleMultiplier = 1.5f; // 元のサイズに対する倍率
    [HideInInspector] public Vector3 endScale;
}

