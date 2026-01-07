using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// リスト内のオブジェクトを、向きを固定したまま円形に周回させるスクリプト（最終FIX版）
/// Rigidbodyを持つオブジェクトが滑らないよう、FixedUpdateで処理するよう修正。
/// </summary>
public class ObjectListRotator : MonoBehaviour
{
    [Header("回転の基本設定")]
    [Tooltip("回転の中心となるオブジェクトのTransform")]
    [SerializeField] private Transform rotationCenter;

    [Tooltip("回転の半径")]
    [SerializeField] private float radius = 10f;

    [Tooltip("1秒あたりの回転速度（角度）")]
    [SerializeField] private float rotationSpeed = 30.0f;

    [Tooltip("回転する軸。XY回転(縦回転)の場合は (0, 0, 1) に設定します。")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;

    [Header("向きの固定")]
    [Tooltip("チェックを入れると、オブジェクトの向きが常に真正面（無回転）に固定されます。")]
    [SerializeField] private bool lockRotationToIdentity = true;


    [Header("対象オブジェクト")]
    [Tooltip("回転させたいオブジェクトのリスト")]
    [SerializeField] private List<Transform> rotatingObjects = new List<Transform>();

    // --- プライベート変数 ---
    private Vector3 centerPosition;
    private float currentAngle = 0f;
    private float angleIncrement;
    private Vector3 initialDirection;
    private List<Quaternion> initialRotations;

    void Start()
    {
        if (rotationCenter == null || rotatingObjects.Count == 0)
        {
            this.enabled = false;
            return;
        }

        centerPosition = rotationCenter.position;
        initialRotations = new List<Quaternion>();
        angleIncrement = 360f / rotatingObjects.Count;

        initialDirection = Vector3.right;
        if (Mathf.Abs(Vector3.Dot(rotationAxis.normalized, Vector3.right)) > 0.99f)
        {
            initialDirection = Vector3.forward;
        }

        foreach (var obj in rotatingObjects)
        {
            if (obj != null)
            {
                initialRotations.Add(obj.rotation);
            }
        }

        UpdateObjectPositionsAndRotations();
    }

    // ★★★ 修正点１ ★★★
    // Updateではなく、物理演算のタイミングで呼ばれるFixedUpdateに変更
    void FixedUpdate()
    {
        // ★★★ 修正点２ ★★★
        // Time.deltaTimeではなく、FixedUpdate用のTime.fixedDeltaTimeを使用
        currentAngle += rotationSpeed * Time.fixedDeltaTime;

        UpdateObjectPositionsAndRotations();
    }

    private void UpdateObjectPositionsAndRotations()
    {
        for (int i = 0; i < rotatingObjects.Count; i++)
        {
            if (rotatingObjects[i] == null) continue;

            float objectAngle = currentAngle + (i * angleIncrement);
            Quaternion positionRotation = Quaternion.AngleAxis(objectAngle, rotationAxis);
            Vector3 newPosition = centerPosition + (positionRotation * (initialDirection * radius));

            rotatingObjects[i].position = newPosition;

            if (lockRotationToIdentity)
            {
                rotatingObjects[i].rotation = Quaternion.identity;
            }
            else
            {
                rotatingObjects[i].rotation = initialRotations[i];
            }
        }
    }
}