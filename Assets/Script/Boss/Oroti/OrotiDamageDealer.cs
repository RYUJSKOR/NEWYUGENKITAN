using UnityEngine;

public class OrotiDamageDealer : MonoBehaviour
{

    [SerializeField] private float damage = 1f;

    private bool canDealDamage = false;

    /// <summary>
    /// 攻撃アニメーションの開始で呼ぶ
    /// </summary>
    public void EnableDamage()
    {
        canDealDamage = true;
    }

    /// <summary>
    /// 攻撃アニメーション終了で呼ぶ
    /// </summary>
    public void DisableDamage()
    {
        canDealDamage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;

        if (other.CompareTag("Player"))
        {
            CharacterHealthManager playerHealth =
                other.GetComponent<CharacterHealthManager>();

            if (playerHealth != null)
            {
                playerHealth.ApplyDamage(damage);
            }
        }
    }
}
