using UnityEngine;

public class GameData : MonoBehaviour
{

    public bool IsBossStage = false;  // 現在のステージがボスかどうか
    public bool IsBossClear = false;  // クリア種別（結果画面の分岐用）


    public float PlayTime;

    public float PlayerHP;

    public float PlayerMaxHP = 10.0f;

    // 
    private static GameData instance;
    public static GameData Instance => instance;

    // 
    public float Boss1PlayTime = 0f;
    public float Boss2PlayTime = 0f;

    public float PlayTimeFinal = 0f;
    public float Boss1PlayTimeFinal = 0f;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetAll()
    {
        PlayTime = 0f;
        PlayerHP = 0f;
    }

    // 
    // 
    // 
    public void saveTime(float time)
    {
        PlayTime = time;
    }

    // 
    public void saveBossTime(float time)
    {
        Boss1PlayTime = time;
    }

    // 
    public float GetTotalBossTime()
    {
        return Boss1PlayTime + Boss2PlayTime;
    }

    // 
    // 
    // 
    public void savePlayerHP(float HP)
    {
        PlayerHP = HP;
    }

}
