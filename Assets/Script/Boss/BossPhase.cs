using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BossPhase
{
    public string phaseName;

    [Header("このフェーズのマテリアルカラー")]
    public Color phaseColor = Color.white; // デフォルト値を白に設定

    [Header("このフェーズで使う攻撃パターン")]
    public List<BossAttackPattern> attackPatterns;

    public float attackInterval = 5.0f;

    [Header("次のフェーズへの移行条件")]
    [Tooltip("この体力割合（0~1）以下になったら次のフェーズに移行する")]
    [Range(0f, 1f)]
    public float transitionHealthThreshold;

    [Header("移行先のシーン")]
    [SceneSelector]
    public string nextSceneName;
}