using UnityEngine;

/// <summary>
/// BaseItemを継承した、HPを回復させるアイテム。
/// </summary>
public class HealthPotion : BaseItem
{
    [Header("回復アイテムの設定")]
    [SerializeField]
    private float recoveryValue = 20f; // HPの回復量

    /// <summary>
    /// アイテム使用時の処理を具体的に実装する。
    /// </summary>
    /// <param name="user">使用者</param>
    public override void Use(GameObject user)
    {
        // 使用者からCharacterHealthManagerコンポーネントを取得
        CharacterHealthManager healthManager = user.GetComponent<CharacterHealthManager>();

        if (healthManager != null)
        {
            // 体力を回復させる
            healthManager.Recovery(recoveryValue);
            Debug.Log(user.name + " のHPが " + recoveryValue + " 回復した！");

            // もしピックアップエフェクトが設定されていれば、アイテムの位置に生成する
            if (pickupEffectPrefab != null)
            {
                GameObject effectInstance = Instantiate(pickupEffectPrefab, user.transform.position, Quaternion.identity);
                effectInstance.transform.SetParent(user.transform); // 使用者に追従
            }

            // アイテムは消費されたので、シーンからオブジェクトを破棄する
            PickupAndDestroyAfterSE("Item.Heel");
        }
        else
        {
            Debug.LogWarning(user.name + " は回復アイテムを使用できません。");
        }
    }
}