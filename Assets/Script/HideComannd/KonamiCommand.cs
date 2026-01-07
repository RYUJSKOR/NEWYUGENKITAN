using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class KonamiCommand : MonoBehaviour
{
    private List<KeyCode> konamiCommandSequence = new List<KeyCode> {
        KeyCode.UpArrow,
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.B,
        KeyCode.A
    };

    [SerializeField]
    private bool[] konamiCommandProgress;

    [SerializeField, SceneSelector] private string SceneName; 

    private bool skipThisFrame = false; // ✅ 1フレームスキップ用フラグ

    public bool[] KonamiCommandProgress => konamiCommandProgress;

    void Awake()
    {
        konamiCommandProgress = new bool[konamiCommandSequence.Count];
    }

    void Update()
    {
        // ✅ コマンド発動後の1フレームだけ入力処理をスキップ
        if (skipThisFrame)
        {
            skipThisFrame = false;
            return;
        }

        CheckKonamiCommandInput();
    }

    private void CheckKonamiCommandInput()
    {
        if (Input.anyKeyDown)
        {
            KeyCode currentKey = GetCurrentKeyDown();
            if (currentKey != KeyCode.None)
            {
                int nextInputIndex = konamiCommandProgress.TakeWhile(b => b).Count();

                if (nextInputIndex >= konamiCommandSequence.Count)
                    return;

                KeyCode expectedKey = konamiCommandSequence[nextInputIndex];

                if (currentKey == expectedKey)
                {
                    konamiCommandProgress[nextInputIndex] = true;

                    if (konamiCommandProgress.All(b => b))
                    {
                        ExecuteKonamiCommandAction();
                        ResetKonamiCommandProgress();

                        skipThisFrame = true; // ✅ 次のUpdateはスキップ
                    }
                }
                else
                {
                    ResetKonamiCommandProgress(); // ❌誤入力ならリセット
                }
            }
        }
    }

    KeyCode GetCurrentKeyDown()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
                return key;
        }
        return KeyCode.None;
    }

    void ExecuteKonamiCommandAction()
    {
        Debug.Log("コナミコマンド（キーボード）が入力されました！");

        SceneManager.LoadScene(SceneName);

    }

    void ResetKonamiCommandProgress()
    {
        for (int i = 0; i < konamiCommandProgress.Length; i++)
        {
            konamiCommandProgress[i] = false;
        }
    }
}
