using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Gacha/Character")]
public class CharacterDataSO : ScriptableObject
{
    public string characterName;
    public Sprite characterImage;
    public Rarity rarity;
}
