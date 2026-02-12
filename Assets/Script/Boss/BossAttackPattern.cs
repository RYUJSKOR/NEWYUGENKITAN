using UnityEngine;

public abstract class BossAttackPattern : ScriptableObject
{
    [Header("アニメ?ション設定")]
    // ▼▼▼ 以下に [Tooltip("説明文")] を追加しました ▼▼▼
    [Tooltip("攻撃の?備段階（?う、振りかぶるなど）での手の?")]
    public ArmAnimationState prepareHandState = ArmAnimationState.OpenHand;

    [Tooltip("攻撃がヒットする瞬間（?きつけ、ビン?など）での手の?")]
    public ArmAnimationState actionHandState = ArmAnimationState.Fist;

    [Tooltip("攻撃後に待?位置へ戻る際の手の?")]
    public ArmAnimationState returnHandState = ArmAnimationState.Default;
    // ▲▲▲ ここまで ▲▲▲

    public abstract void Execute(BossController boss);

    public virtual void Cleanup(BossController boss)
    {
        // 基底クラスでは何もしない
    }
}