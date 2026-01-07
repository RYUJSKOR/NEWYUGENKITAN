using UnityEngine;

public class DeathLine : MonoBehaviour
{
    [SerializeField]
    private float playerDamage = 1f;

    // このオブジェクトに触れたキャラクターは確定で死亡

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // プレイヤーコンポーネントを取得
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // プレイヤーに落下復帰処理の開始を依頼する
                player.StartFallRecovery();
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            // 敵はこれまで通り即死させる
            CharacterHealthManager characterHealth = other.GetComponent<CharacterHealthManager>();
            if (characterHealth != null)
            {
                characterHealth.InstantKill();
            }
        }
    }
}
