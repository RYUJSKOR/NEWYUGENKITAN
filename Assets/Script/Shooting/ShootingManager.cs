using UnityEngine;
using System.Collections.Generic;

public class ShootingManager : MonoBehaviour
{
    [SerializeField] private List<Shooting> shootingObjects = new();
    [SerializeField] private bool debugLogEnabled = false;

    private static ShootingManager _instance;
    public static ShootingManager Instance => _instance ?? throw new System.Exception("ShootingManager is not initialized");

    private IBulletFactory bulletFactory;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        bulletFactory = new DefaultBulletFactory(); // 将来的に差し替え可能
    }

    private void Update()
    {
        foreach (var shooter in shootingObjects)
        {
            while (shooter != null && shooter.HasShootRequests)
            {
                Vector3 direction = shooter.DequeueShootDirection();
                Fire(shooter, direction);
            }
        }
    }

    public void RegisterShooting(Shooting shooter)
    {
        if (shooter != null && !shootingObjects.Contains(shooter))
        {
            shootingObjects.Add(shooter);
        }
    }

    private void Fire(Shooting shooter, Vector3 direction)
    {
        var bulletPrefab = shooter.GetBulletObject();
        if (bulletPrefab == null)
        {
            Debug.LogWarning($"[{shooter.name}] 弾Prefabが設定されていません。");
            return;
        }

        // spreadDirはZ回転で作ったXY方向の正規化ベクトルと想定
        Vector3 dir = new Vector3(direction.x, direction.y, 0).normalized;

        // spreadDirに直交するベクトルを作る（XY平面）
        Vector3 perp = new Vector3(-dir.y, dir.x, 0);

        // オフセットの左右（x）と上下（y）を使ってオフセット位置を計算
        Vector3 offset = perp * shooter.GetOffset().x + Vector3.up * shooter.GetOffset().y;

        // 発射位置（射手の位置＋オフセット＋弾の進行方向へのちょっとした前進）
        Vector3 shootPos = shooter.transform.position + offset + dir * 1.0f;

        bulletFactory.CreateBullet(bulletPrefab, shootPos, dir, shooter);

        if (debugLogEnabled)
        {
            Debug.Log($"[{shooter.name}] が方向 {direction} に弾を発射");
        }
    }
}
