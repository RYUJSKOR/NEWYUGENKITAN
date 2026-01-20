using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 全てのアイテムの基底となる抽象クラス。
/// </summary>
public abstract class BaseItem : MonoBehaviour
{
    [Header("アイテムの基本情報")]
    [SerializeField]
    private string itemName = "New Item";

    [SerializeField, TextArea]
    private string itemDescription = "Item Description";

    [Header("アイテムの効果対象")]
    [Tooltip("このリストが空の場合、誰でもアイテムを使用できます。タグを指定した場合、そのタグを持つオブジェクトのみが使用できます。")]
    [SerializeField, TagSelector]
    protected List<string> targetTags = new List<string>();

    [Header("アニメーション設定")]
    [SerializeField]
    private float rotationSpeed = 50f; // 1秒間の回転角度
    [SerializeField]
    private float hoverSpeed = 1.5f;   // 上下運動の速さ
    [SerializeField]
    private float hoverHeight = 0.2f;  // 上下運動の幅

    [Header("エフェクト設定")]
    [Tooltip("アイテム取得時に再生するエフェクトのプレハブ")]
    [SerializeField] protected GameObject pickupEffectPrefab;

    private Vector3 startPosition; // アイテムの初期位置を保存する変数

    protected SEController SE;

    /// <summary>
    /// アイテムを使用した時の効果を定義する抽象メソッド。
    /// 派生クラスで具体的な処理を実装する。
    /// </summary>
    /// <param name="user">アイテムを使用したキャラクターのGameObject</param>
    public abstract void Use(GameObject user);

    void Start()
    {
        // 起動した時のオブジェクトのY軸を含めた位置を記憶しておく
        startPosition = transform.position;

        if (SE == null)
        {
            SE = GetComponentInChildren<SEController>();
        }

        if (SE == null)
        {
            Debug.LogError($"{name} に SEController が設定されていません。");
        }
    }

    void Update()
    {
        // 1. 回転処理
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // 2. 上下運動処理
        float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 対象タグリストが空、または接触したオブジェクトのタグがリストに含まれているかチェック
        bool canUse = targetTags.Count == 0 || targetTags.Contains(other.gameObject.tag);

        if (canUse)
        {
            // アイテム使用を試みるキャラクターがCharacterHealthManagerを持っているか確認
            if (other.GetComponent<CharacterHealthManager>() != null)
            {
                Use(other.gameObject);
            }
        }
    }

    protected void PickupAndDestroyAfterSE(string seKey)
    {
        // 1. 見た目と当たり判定を消す
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        foreach (var c in GetComponents<Collider>())
            c.enabled = false;

        // 2. SE 再生
        float seLength = 0f;

        if (SE != null)
        {
            seLength = SE.Play(seKey);
        }

        // 3. 再生完了後に Destroy
        Destroy(gameObject, seLength > 0 ? seLength : 0.1f);
    }
}