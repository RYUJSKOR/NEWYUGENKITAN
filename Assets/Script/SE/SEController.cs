using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SEController : MonoBehaviour
{
    [SerializeField] private SEProfile profile;

    private AudioSource source;
    private Dictionary<string, SEProfile.SEData> dict = new();
    private Dictionary<string, float> lastPlay = new();

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;

        foreach (var se in profile.seList)
        {
            dict[se.id] = se;
            lastPlay[se.id] = -Mathf.Infinity;
        }
    }

    /// <summary>
    /// SEÇçƒê∂ÇµÅAÇªÇÃSEÇÃçƒê∂éûä‘Çï‘Ç∑
    /// </summary>
    public float Play(string id)
    {
        if (!dict.TryGetValue(id, out var data) || data.clip == null)
        {
            Debug.LogWarning($"SE ID not found : {id}", this);
            return 0.1f;
        }

        if (Time.time < lastPlay[id] + data.cooldown)
            return 0f;

        lastPlay[id] = Time.time;
        source.PlayOneShot(data.clip, data.volume);

        return data.clip.length;
    }
}