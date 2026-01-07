using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [Header("弾の設定")]
    [SerializeField] private List<GameObject> bulletPrefabs = new();
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField, TagSelector] private List<string> ignoreTags;
    [SerializeField] private Vector2 Shootingoffset = new Vector2(1.0f,1.0f);

    private Queue<Vector3> shootRequests = new();
    private Vector3? overrideDirection = null;
    private GameObject currentBulletPrefab;

    private void Start()
    {
        if (bulletPrefabs.Count > 0)
        {
            currentBulletPrefab = bulletPrefabs[0]; // 最初の弾をデフォルトに
        }

        ShootingManager.Instance.RegisterShooting(this);
    }

    public void SetDirection(Vector3 dir) => overrideDirection = dir.normalized;
    public void ClearDirection() => overrideDirection = null;

    public void RequestShoot() => shootRequests.Enqueue(GetShootDirection());
    public void RequestShoot(Vector3 direction) => shootRequests.Enqueue(direction.normalized);

    public Vector3 GetShootDirection() => overrideDirection ?? transform.forward;

    public void SetBulletByName(string prefabName)
    {
        currentBulletPrefab = bulletPrefabs.Find(p => p != null && p.name == prefabName);
        if (currentBulletPrefab == null)
        {
            Debug.LogWarning($"Bullet prefab named '{prefabName}' not found.");
        }
    }

    public void SetBulletSpeed(float speed) => bulletSpeed = speed;

    public bool HasShootRequests => shootRequests.Count > 0;
    public Vector3 DequeueShootDirection() => shootRequests.Dequeue();

    public GameObject GetBulletObject() => currentBulletPrefab;
    public float GetBulletSpeed() => bulletSpeed;
    public List<string> GetIgnoreTags() => ignoreTags;
    public void SetOffset(Vector2 vector2){ Shootingoffset = vector2; }
    public Vector2 GetOffset() => Shootingoffset;
}
