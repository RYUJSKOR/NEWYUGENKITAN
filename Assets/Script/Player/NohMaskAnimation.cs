using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NohMaskAnimation : MonoBehaviour
{
    [Header("Aura ビームプレハブ")]
    [SerializeField] private GameObject auraPrefab;

    [Header("仮面エフェクト")]
    [SerializeField] private GameObject maskEffectPrefab;
    [SerializeField] private Vector3 maskOffset = new Vector3(0, 2f, 0);

    private Transform player;
    private GameObject currentMask;  // ← 追加：後で縮小する用

    private void Awake()
    {
        player = transform;
    }

    public void ShowMultiple(List<GameObject> targets, Action onComplete)
    {
        StartCoroutine(PlayRoutine(targets, onComplete));
    }

    private IEnumerator PlayRoutine(List<GameObject> targets, Action onComplete)
    {
        // プレイヤー仮面演出
        if (maskEffectPrefab != null)
        {
            currentMask = Instantiate(maskEffectPrefab, player.position + maskOffset, Quaternion.identity);
            currentMask.transform.SetParent(player);
        }

        yield return new WaitForSeconds(0.15f);

        // ビーム生成（到達したら即ダメージ）
        int finishedCount = 0;
        int total = targets.Count;

        foreach (var enemyObj in targets)
        {
            if (enemyObj == null)
            {
                finishedCount++;
                continue;
            }

            GameObject aura = Instantiate(auraPrefab, player.position, Quaternion.identity);
            var control = aura.GetComponent<cs_AuraControl>();

            control.player = player;
            control.target = enemyObj.transform;

            control.onHit += () =>
            {
                finishedCount++;
            };
        }

        // 全ビームが「到達」したら完了
        while (finishedCount < total)
            yield return null;

        // 完了 → スキルダメージ発動
        onComplete?.Invoke();

        // ★ 縮小開始までの待ち時間（ここを好きな時間に変える）
        yield return new WaitForSeconds(0.6f);

        // マスクを徐々に縮小して消す
        if (currentMask != null)
            StartCoroutine(ScaleDownAndDestroy(currentMask, 1f));
    }

    /// <summary>
    /// マスクを徐々にスケールダウン → 消滅
    /// </summary>
    private IEnumerator ScaleDownAndDestroy(GameObject obj, float duration)
    {
        float timer = 0f;
        List<Transform> allChildren = new List<Transform>();
        obj.GetComponentsInChildren<Transform>(allChildren);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float scale = Mathf.Lerp(1f, 0f, t);

            foreach (var tr in allChildren)
                tr.localScale = Vector3.one * scale;

            yield return null;
        }

        Destroy(obj);
    }
}
