using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SEController : MonoBehaviour
{
	[System.Serializable]
	public struct SEData
	{
		public PlayerSEType type;
		public AudioClip clip;

		[Range(0f, 1f)]
		public float volume;

		[Header("Cooldown (sec)")]
		public float cooldown;
	}

	[Header("SE Settings")]
	[SerializeField] private SEData[] seList;

	private AudioSource seSource;
	private Dictionary<PlayerSEType, SEData> seDict;
	private Dictionary<PlayerSEType, float> lastPlayTimeDict;

	private void Awake()
	{
		seSource = GetComponent<AudioSource>();
		seSource.playOnAwake = false;
		seSource.loop = false;
		seSource.spatialBlend = 0f; // 2D

		seDict = new Dictionary<PlayerSEType, SEData>();
		lastPlayTimeDict = new Dictionary<PlayerSEType, float>();

		foreach (var se in seList)
		{
			seDict[se.type] = se;
			lastPlayTimeDict[se.type] = -Mathf.Infinity;
		}
	}

	/// <summary>
	/// SE再生（クールタイム考慮）
	/// </summary>
	public void Play(PlayerSEType type)
	{
		if (!seDict.TryGetValue(type, out var data))
			return;

		if (data.clip == null)
			return;

		float lastTime = lastPlayTimeDict[type];
		if (Time.time < lastTime + data.cooldown)
			return; // クールタイム中

		lastPlayTimeDict[type] = Time.time;
		seSource.PlayOneShot(data.clip, data.volume);
	}
}