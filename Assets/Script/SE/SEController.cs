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

        foreach (var se in profile.seList)
        {
            dict[se.id] = se;
            lastPlay[se.id] = -Mathf.Infinity;
        }
    }

    public void Play(string id)
    {
        if (!dict.TryGetValue(id, out var data))
        {
            Debug.LogWarning($"SE ID not found : {id}", this);
            return;
        }

        if (Time.time < lastPlay[id] + data.cooldown)
            return;

        lastPlay[id] = Time.time;
        source.PlayOneShot(data.clip, data.volume);
    }
}