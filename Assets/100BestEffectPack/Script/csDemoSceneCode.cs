using UnityEngine;
using UnityEngine.UI; // UI Text を使用するために必要
using System.Collections;

public class csDemoSceneCode : MonoBehaviour
{
    public string[] EffectNames;
    public string[] Effect2Names;
    public Transform[] Effect;
    public Text Text1; // GUIText から UI.Text に変更
    int i = 0;
    int a = 0;

    void Start()
    {
        if (Effect.Length > 0)
        {
            Instantiate(Effect[i], new Vector3(0, 5, 0), Quaternion.identity);
            UpdateText(); // 開始時にテキストを更新
        }
        else
        {
            Debug.LogError("Effect 配列が空です。");
        }
    }

    void Update()
    {
        // Text1 が割り当てられているか確認
        if (Text1 != null && EffectNames.Length > i && i >= 0)
        {
            UpdateText();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (i <= 0)
                i = EffectNames.Length - 1; // 配列の最後にループ

            else
                i--;

            InstantiateEffect();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (i < EffectNames.Length - 1)
                i++;
            else
                i = 0; // 配列の最初にループ

            InstantiateEffect();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            InstantiateEffect();
        }
    }

    void InstantiateEffect()
    {
        if (Effect.Length > i && EffectNames.Length > i)
        {
            bool foundInEffect2 = false;
            for (a = 0; a < Effect2Names.Length; a++)
            {
                if (EffectNames[i] == Effect2Names[a])
                {
                    Instantiate(Effect[i], new Vector3(0, 0.2f, 0), Quaternion.identity);
                    foundInEffect2 = true;
                    break;
                }
            }
            if (!foundInEffect2)
            {
                Instantiate(Effect[i], new Vector3(0, 5, 0), Quaternion.identity);
            }
        }
        else
        {
            Debug.LogError("Effect 配列または EffectNames 配列のインデックスが範囲外です。 i = " + i);
        }
    }

    void UpdateText()
    {
        if (Text1 != null && EffectNames.Length > i && i >= 0)
        {
            Text1.text = (i + 1) + ":" + EffectNames[i];
        }
    }
}