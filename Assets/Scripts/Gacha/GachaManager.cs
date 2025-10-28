using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GachaManager : MonoBehaviour
{
    [Header("UI References")]
    public Button drawButton;
    public GameObject resultPanel;
    public Image characterImage;
    public Text characterName;
    public ParticleSystem flashEffect;

    [Header("Character Pool")]
    public List<CharacterData> characters = new List<CharacterData>();

    [Header("Rarity Rates (0~1 sum = 1)")]
    public float rRate = 0.75f;
    public float srRate = 0.20f;
    public float ssrRate = 0.05f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resultPanel.SetActive(false);
        drawButton.onClick.AddListener(DrawCharacter);
    }

    void DrawCharacter()
    {
        resultPanel.SetActive(false);
        flashEffect.Play();

        Invoke(nameof(ShowResult), 1.2f);
    }

    void ShowResult()
    {
        CharacterData selected = GetRandomCharacter();
        resultPanel.SetActive(true);
        characterImage.sprite = selected.characterImage;
        characterName.text = $"{selected.characterName}({selected.rarity})";
    }

    CharacterData GetRandomCharacter()
    {
        float roll = Random.value;
        Rarity pickedRarity = Rarity.R;

        if (roll < ssrRate) pickedRarity = Rarity.SSR;
        else if (roll < ssrRate + srRate) pickedRarity = Rarity.SR;

        List<CharacterData> pool = characters.FindAll(c => c.rarity == pickedRarity);
        return pool[Random.Range(0, pool.Count)];

    }
    // Update is called once per frame

}
