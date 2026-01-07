using UnityEngine;

public abstract class BossAttackPattern : ScriptableObject
{
    [Header("アニメーション設定")]
    // ▼▼▼ 以下に [Tooltip("説明文")] を追加しました ▼▼▼
    [Tooltip("攻撃の準備段階（狙う、振りかぶるなど）での手の形")]
    public ArmAnimationState prepareHandState = ArmAnimationState.OpenHand;

    [Tooltip("攻撃がヒットする瞬間（叩きつけ、ビンタなど）での手の形")]
    public ArmAnimationState actionHandState = ArmAnimationState.Fist;

    [Tooltip("攻撃後に待機位置へ戻る際の手の形")]
    public ArmAnimationState returnHandState = ArmAnimationState.Default;
    // ▲▲▲ ここまで ▲▲▲

    public abstract void Execute(BossController boss);

    public virtual void Cleanup(BossController boss)
    {
        // 基底クラスでは何もしない
    }
}