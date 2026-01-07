using UnityEngine;

public class BossDamageDealer : MonoBehaviour
{
    // ダメージ量などをインスペクターから設定できるようにしても良い
    // public float damageAmount = 10f;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーに当たったかタグで確認
        if (other.CompareTag("Player"))
        {
            Debug.Log("攻撃がプレイヤーにヒット！");

            // プレイヤーのCharacterHealthManagerを取得してダメージを与える
            CharacterHealthManager playerHealth = other.GetComponent<CharacterHealthManager>();
            if (playerHealth != null)
            {
                playerHealth.ApplyDamage(1); // ダメージ量を1に設定
            }
        }
    }
}