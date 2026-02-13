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

        // ˆÚ“®’â~
        speed = 0f;

        // UŒ‚”»’è‚ğ–³Œø‰»
        DisableDamage();

        // Rigidbody‚ğ•¨—‰»
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // ˆê’èŠÔŒã‚ÉÁ‚¦‚é
        Destroy(gameObject, setting.remainTime);
    }

    private void DisableDamage()
    {
        // Bullet‘¤‚ÉUŒ‚—pCollider‚ª‚ ‚é‚È‚ç–³Œø‰»
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false; // ’ÊíCollider‚É
        }

        SetPower(0); // ƒ_ƒ[ƒW0
    }
}
