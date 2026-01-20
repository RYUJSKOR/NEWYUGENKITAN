using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SE Profile")]
public class SEProfile : ScriptableObject
{
    [System.Serializable]
    public class SEData
    {
        public string id;              // —á: "Player.Jump"
        public AudioClip clip;
        [Range(0f, 5f)] public float volume = 1f;
        public float cooldown = 0f;
    }

    public SEData[] seList;
}