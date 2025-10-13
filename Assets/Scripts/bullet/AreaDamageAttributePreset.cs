using UnityEngine;

[CreateAssetMenu(fileName = "AreaDamageAttributePreset", menuName = "Scriptable Objects/AreaDamageAttributePreset")]
public class AreaDamageAttributePreset : ScriptableObject
{
    [Header("Size Attributes")]
    public float scaleX;
    public float scaleY;
}
