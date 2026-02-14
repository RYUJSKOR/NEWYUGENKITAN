using GLTFast.Schema;
using UnityEngine;

public class OrotiRockBullet : OrotiBulletBase
{
    private bool isRock = false;

    protected override void OnGroundHit(Collider other)
    {
        BecomeRock();
        Debug.Log("Ground Hit");
    }

    private void BecomeRock()
    {
        if (isRock) return;
        isRock = true;

        speed = 0f;
        DisableDamage();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;     // ← 固定
            rb.useGravity = false;     // ← 重力も不要
        }

        gameObject.tag = "Ground";
        gameObject.layer = LayerMask.NameToLayer("Ground");

        Destroy(gameObject, setting.remainTime);

        Destroy(gameObject, setting.remainTime);
    }

    private void DisableDamage()
    {
        // Bullet側に攻撃用Colliderがあるなら無効化
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false; // 通常Colliderに
        }

        SetPower(0); // ダメージ0
    }
}
