using UnityEngine;

public class BoneFragment : MonoBehaviour
{
    public float pileSearchRadius = 2.0f;
    public GameObject bonePilePrefab;

    private void OnTriggerEnter(Collider other)
    {
        // ★ログ1：まず、トリガーが検知されているかを確認
        Debug.Log(gameObject.name + " が " + other.gameObject.name + " のトリガーに入りました。");

        // ★ログ2：接触した相手のタグとレイヤー名を出力
        Debug.Log("接触相手のタグ: " + other.gameObject.tag + ", レイヤー: " + LayerMask.LayerToName(other.gameObject.layer));

        // 透明な地面("BonePileGround")に接触した場合
        if (other.gameObject.layer == LayerMask.NameToLayer("BonePileGround"))
        {
            // ★ログ3：正しい地面に接触したことを確認
            Debug.Log("正しい地面/足場に接触しました。骨の山を探します。");

            Collider[] nearbyPiles = Physics.OverlapSphere(transform.position, pileSearchRadius);

            BonePile existingPile = null;
            foreach (var coll in nearbyPiles)
            {
                if (coll.GetComponent<BonePile>() != null)
                {
                    existingPile = coll.GetComponent<BonePile>();
                    break;
                }
            }

            if (existingPile != null)
            {
                existingPile.AddBone();
            }
            else
            {
                if (bonePilePrefab != null)
                {
                    Instantiate(bonePilePrefab, transform.position, Quaternion.identity);
                }
            }

            Destroy(gameObject);
        }
    }
}