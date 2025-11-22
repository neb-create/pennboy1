using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishSelector : MonoBehaviour
{
    string selectedColorString = "#FF4DFE";
    Color selectedColor;
    [SerializeField] CharacterDataSO type;
    [SerializeField] TextMeshProUGUI tmp_name; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!type.owned)
        {
            Button b = GetComponentInChildren<Button>();
            b.interactable = false;
        }
        ColorUtility.TryParseHtmlString(selectedColorString, out selectedColor);
    }
    void Update()
    {
        int curType = PlayerPrefs.GetInt("Fish Type");
        Debug.Log("fish type: " + curType);
        if(curType == type.typeNumber)
        {
            tmp_name.color = selectedColor;
        }
        else
        {
            tmp_name.color = Color.white;
        }
    }

    public void OnSelected()
    {
        PlayerPrefs.SetInt("Fish Type", type.typeNumber);
    }
}
