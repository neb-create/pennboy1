using UnityEngine;

[CreateAssetMenu(fileName = "BulletAttributePreset", menuName = "Scriptable Objects/BulletAttributePreset")]
public class BulletAttributePreset : ScriptableObject
{
    [Header("Homing Attributes")]
    public bool homing;
    public float delayBeforeLockOn;
    public float homingRotateSpeedRads;
    public float lockedOnSpeedMultiplier;

    [Header("General Attributes")]
    public float damage;
}
