using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    protected CharacterHealthManager healthManager;
    [SerializeField] protected GameObject piecesPrefab;
    [SerializeField] protected int GomiNum;
    [SerializeField] protected float GomiLifeTime;

    [Header("Speed Settings")]
    // 現在の速度倍率を保持する変数
    [SerializeField] protected float currentSpeedModifier = 1.0f;

    [Header("ドロップ設定")]
    [Range(0f, 100f)]
    [SerializeField] private float dropChance = 100f; // デフォルトは10%（追加：橋本）

    [SerializeField] private GameObject dropItemPrefab; // 回復アイテムを入れる箱（追加：橋本）


    protected virtual void Start()
    {
        healthManager = GetComponent<CharacterHealthManager>();

        if (healthManager != null)
        {
            healthManager.OnDamageTaken += OnDamageTakenHandler;
        }
    }

    virtual protected void Explode()
    {
        for (int i = 0; i < GomiNum; i++)
        {
            GameObject gomi = Instantiate(piecesPrefab, transform.position, Quaternion.identity);
            gomi.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-3f, 3f), Random.Range(2f, 5f), Random.Range(-3f, 3f)));
            Destroy(gomi, GomiLifeTime);
        }

        // 倒されたときに確率でアイテムをドロップ（追加：橋本）
        DropItem();
    }

    public virtual void ApplySpeedModifier(float modifier)
    {
        currentSpeedModifier = modifier;
    }

    protected virtual void OnDestroy()
    {
        if (healthManager != null)
        {
            healthManager.OnDamageTaken -= OnDamageTakenHandler;
        }

        if (EnemyCounter.Instance != null)
        {
            EnemyCounter.Instance.RemoveEnemy(gameObject);
            Debug.Log($"{gameObject.name} が破棄されたため、敵カウンターから削除しました。現在の敵数: {EnemyCounter.Instance.EnemyCountInView}");
        }
    }

    private void OnDamageTakenHandler()
    {
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        var rend = GetComponentInChildren<MeshRenderer>();
        if (rend == null) yield break;

        // マテリアルのインスタンス化（共有を避ける）
        rend.material = new Material(rend.material);

        // 元の色を取得（_TintColorが無ければ白にフォールバック）
        Color originalColor = Color.white;
        if (rend.material.HasProperty("_TintColor"))
        {
            originalColor = rend.material.GetColor("_TintColor");
        }
        else if (rend.material.HasProperty("_Color"))
        {
            originalColor = rend.material.GetColor("_Color");
        }

        rend.material.SetColor("_TintColor", Color.red);

        yield return new WaitForSeconds(0.2f);

        rend.material.SetColor("_TintColor", originalColor);
    }

    //確率でアイテムをドロップさせる関数（追加：橋本）
    protected void DropItem()
    {
        // Prefabが未設定なら何も落とさない
        if (dropItemPrefab == null) return;

        // ランダムな値が dropChance 以下ならドロップ
        if (Random.Range(0f, 100f) < dropChance)
        {
            Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
        }
    }
}
