using UnityEngine;

public class OrotiNeck : MonoBehaviour
{
    private OrotiController oroti;
    private Animator animator;

    private bool damagedThisFrame;

    private void Awake()
    {
        oroti = GetComponentInParent<OrotiController>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 攻撃アニメを再生（Idleは自動復帰）
    /// </summary>
    public void PlayAttackAnimation(string animName)
    {
        animator.Play(animName);
    }

    /// <summary>
    /// プレイヤー攻撃が当たったときに呼ばれる
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (damagedThisFrame) return;

        damagedThisFrame = true;
        oroti.ApplyDamageToBoss(damage);
        StartCoroutine(ResetDamageFlag());
    }

    private System.Collections.IEnumerator ResetDamageFlag()
    {
        yield return null; // 1フレームで解除
        damagedThisFrame = false;
    }
}
