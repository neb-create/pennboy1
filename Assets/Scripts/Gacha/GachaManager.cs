using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GachaManager : MonoBehaviour
{
    [Header("UI References")]
    public Image characterImage;
    public TextMeshProUGUI characterName;
    public Button draw1Button;
    public Button draw10Button;
    public Button confirmButton;

    public GameObject resultPanel;
    public ParticleSystem flashEffect;

    [Header("Character Pool")]
    public List<CharacterDataSO> characters;

    [Header("Rarity Rates (0~1 sum = 1)")]
    public float rRate = 0.75f;
    public float srRate = 0.20f;
    public float ssrRate = 0.05f;


    // internal states
    private List<CharacterDataSO> pulledCharacters = new List<CharacterDataSO>();
    private int currentIndex = -1;
    private bool showingResults = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resultPanel.SetActive(false);
        draw1Button.onClick.AddListener(() => DrawCharacter(1));
        draw10Button.onClick.AddListener(() => DrawCharacter(10));
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    IEnumerator DelayShow(CharacterDataSO selected, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowResult(selected);
    }

    void DrawCharacter(int count)
    {
        Debug.Log($"Starting gacha: {count} pull(s)!");
        resultPanel.SetActive(false);
        flashEffect.Play();

        if (count == 1)
        {
            CharacterDataSO selected = GetRandomCharacter();
            selected.owned = true;
            StartCoroutine(DelayShow(selected, 1.2f));
            Debug.Log($"Character: {selected.characterName}({selected.rarity})");
        }
        else
        {
            pulledCharacters.Clear();
            for (int i = 0; i < 10; i++)
            {
                CharacterDataSO selected = GetRandomCharacter();
                selected.owned = true;
                pulledCharacters.Add(selected);
                Debug.Log($"[{i}] {selected.characterName} ({selected.rarity})");
            }

            // start from last card
            currentIndex = pulledCharacters.Count - 1;
            showingResults = true;

            // show first card in sequence
            StartCoroutine(DelayShow(pulledCharacters[currentIndex], 1.2f));
            confirmButton.gameObject.SetActive(true);
        };

    }

    void ShowResult(CharacterDataSO selected)
    {
        resultPanel.SetActive(true);
        characterImage.sprite = selected.characterImage;
        characterName.text = $"{selected.characterName}({selected.rarity})";
        flashEffect.Play();
    }

    void OnConfirmClicked()
    {
        if (!showingResults) return;

        flashEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        currentIndex--;

        if (currentIndex < 0)
        {
            // finished showing all cards
            showingResults = false;
            confirmButton.gameObject.SetActive(false);
            resultPanel.SetActive(false);
            Debug.Log("All 10 cards revealed!");
            return;
        }

        ShowResult(pulledCharacters[currentIndex]);
    }

    CharacterDataSO GetRandomCharacter()
    {
        float roll = Random.value;
        Rarity pickedRarity = Rarity.R;

        if (roll < ssrRate) pickedRarity = Rarity.SSR;
        else if (roll < ssrRate + srRate) pickedRarity = Rarity.SR;

        List<CharacterDataSO> pool = characters.FindAll(c => c.rarity == pickedRarity);
        return pool[Random.Range(0, pool.Count)];

    }
    // Update is called once per frame

}
