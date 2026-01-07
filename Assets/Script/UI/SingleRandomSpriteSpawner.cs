using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
public class RandomSpriteAssigner : MonoBehaviour
{
    [Header("スプライト設定")]
    [SerializeField]
    private List<Sprite> spriteOptions;

    [Header("回転設定")]
    [SerializeField]
    private float minRotationSpeed = 30f;
    [SerializeField]
    private float maxRotationSpeed = 180f;

    private Image targetImage;
    private SpriteRotator rotator;

    // 変更点: Start -> OnEnable
    // オブジェクトがアクティブになるたびに実行
    void OnEnable()
    {
        // 1. コンポーネント取得
        targetImage = GetComponent<Image>();
        rotator = GetComponent<SpriteRotator>();
        if (rotator == null)
        {
            rotator = gameObject.AddComponent<SpriteRotator>();
        }

        // 2. スプライトのリストチェック
        if (spriteOptions == null || spriteOptions.Count == 0)
        {
            Debug.LogWarning("表示するスプライトの候補が設定されていません。", this);
            return;
        }

        // 3. ランダムなスプライトを割り当て (毎回変わる)
        int randomIndex = Random.Range(0, spriteOptions.Count);
        targetImage.sprite = spriteOptions[randomIndex];
        targetImage.color = Color.white;

        // 4. ランダムな回転を設定 (毎回変わる)
        float randomSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        Vector3 randomDirection = (Random.value < 0.5f) ? Vector3.forward : Vector3.back;

        // 5. SpriteRotatorに設定を渡して回転開始
        rotator.SetRotation(randomSpeed, randomDirection);
    }
}