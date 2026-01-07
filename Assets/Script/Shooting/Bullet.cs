using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("弾設定")]
    [SerializeField] protected float power = 1.0f;
    [SerializeField] protected float lifeTime = 5f;
    [SerializeField] protected GameObject ExplosionObject;
    protected GameObject Owner;
    [SerializeField, TagSelector] protected List<string> ignoreTags = null;

    private Camera mainCamera; // メインカメラをキャッシュする変数
    private float checkInterval = 0.2f; // 画面外チェックを行う間隔（秒）
    private float nextCheckTime; // 次にチェックを行う時間

    protected void Start()
    {
        mainCamera = Camera.main; // 最初にメインカメラを取得しておく
        nextCheckTime = Time.time + checkInterval; // 最初のチェック時間を設定
        Destroy(gameObject, lifeTime); // 一定時間後に削除
    }

    protected virtual void Update()
    {
        if (Time.time > nextCheckTime)
        {
            CheckIfOffScreen();
            nextCheckTime = Time.time + checkInterval;
        }
    }

    private void CheckIfOffScreen()
    {
        // メインカメラがなければ処理しない
        if (mainCamera == null)
        {
            Debug.Log("カメラが設定されていません！");
            return;
        }
        // 自身のワールド座標をカメラのビューポート座標に変換
        // ビューポート座標: 画面の左下が(0,0)、右上が(1,1)になる座標系
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        // z座標がマイナスの場合、カメラの後ろにあるので画面外とみなす
        // xかyが0未満、または1より大きい場合も画面外
        if (viewportPos.z < 0 || viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            // 画面外に出たので、このゲームオブジェクトを破棄する
            Destroy(gameObject);
            Debug.Log("弾が画面外に出たため削除しました。"); // テスト用
        }

        Debug.Log("カメラ内だよ！");
    }

    virtual protected void OnTriggerEnter(Collider collision)
    {
        // 無視リストにあるタグならスルー
        if (ignoreTags.Contains(collision.tag) || collision.gameObject == Owner) return;

        // 攻撃処理
        var target = collision.GetComponent<CharacterHealthManager>();
        if (target != null && collision.gameObject != Owner)
        {
            target.ApplyDamage(power);
        }

        // 爆発オブジェクト生成
        if (ExplosionObject != null)
        {
            GameObject obj = Instantiate(ExplosionObject, transform.position, Quaternion.identity);
            Destroy(obj, 2);
        }

        // 弾を削除
        Destroy(gameObject);
    }

    public void SetOwner(GameObject owner) => Owner = owner;

    public void SetIgnoreTags(List<string> tags) => ignoreTags = tags;

    public float GetPower() => power;

    public void SetPower(float Power) { power = Power; }

    public float GetLifeTime() => lifeTime;

    public void SetLifeTime(float lifetime) { lifeTime = lifetime; }
}