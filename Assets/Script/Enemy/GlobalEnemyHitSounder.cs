using UnityEngine;

public class GlobalEnemyHitSounder : MonoBehaviour
{
    private SEController SE;

    [Header("Bullet Prefab Settings")]
    [SerializeField] private GameObject bulletNormal;
    [SerializeField] private GameObject bulletDevil;
    [SerializeField] private GameObject bulletFox;

    void Awake()
    {
        // シーン内のSEControllerを取得
        SE = FindObjectOfType<SEController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }

    /// <summary>
    /// 当たった弾の種類を判定し、対応するSEを再生する
    /// </summary>
    private void HandleHit(GameObject hitObject)
    {
        if (SE == null) return;

        // (Clone)を考慮せず、プレハブの参照で直接比較
        // もし名前で判定したい場合は hitObject.name.Contains(bulletNormal.name) 等を使用

        if (bulletNormal != null && hitObject.name.StartsWith(bulletNormal.name))
        {
            SE.Play("Enemy.HitSoundNormal");
        }
        else if (bulletDevil != null && hitObject.name.StartsWith(bulletDevil.name))
        {
            SE.Play("Enemy.HitSounddevil");
        }
        else if (bulletFox != null && hitObject.name.StartsWith(bulletFox.name))
        {
            SE.Play("Enemy.HitSoundfox");
        }
    }
}