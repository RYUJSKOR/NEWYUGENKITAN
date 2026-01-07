using UnityEngine;

public class VolumeSettings : MonoBehaviour
{
    public static VolumeSettings Instance { get; private set; }

    public float Volume { get; private set; } = 0.5f;

    void Awake()
    {
        // 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 
        Volume = PlayerPrefs.GetFloat("GlobalVolume", 0.5f);
    }

    public void SetVolume(float value)
    {
        Volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("GlobalVolume", Volume);
    }
}
