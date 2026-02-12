using UnityEngine;

[System.Serializable]
public class OrotiBulletEntry
{
    public OrotiBulletType type;
    public GameObject bulletPrefab;

    public OrotiBulletSetting phase1Setting;
    public OrotiBulletSetting phase2Setting;
    public OrotiBulletSetting phase3Setting;
}