using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedValueText;

    [SerializeField] private Slider offsetSlider;
    [SerializeField] private TextMeshProUGUI offsetValueText;

    private void Start()
    {
        float currentSpeed = 9f;
        float currentOffset = 0.22f;
        // 1. Check if GlobalSettings exists
        if (GlobalSettings.Instance != null)
        {
            // 2. Initialize Sliders to match current stored values
            currentSpeed = GlobalSettings.Instance.scrollSpeed;
            currentOffset = GlobalSettings.Instance.audioOffset;

            speedSlider.value = currentSpeed;
            offsetSlider.value = currentOffset;
            
        }
        else
        {
            Debug.LogWarning("GlobalSettings instance not found!");
        }

        // 4. Add Listeners (Or do this in the Inspector via OnValueChanged)
        speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        offsetSlider.onValueChanged.AddListener(OnOffsetChanged);
        
        UpdateSpeedLabel(currentSpeed);
        UpdateOffsetLabel(currentOffset);
    }

    // Called when the Scroll Speed Slider is moved
    public void OnSpeedChanged(float val)
    {
        // Update the Singleton
        if (GlobalSettings.Instance != null)
        {
            GlobalSettings.Instance.scrollSpeed = val;
        }

        // Update the visual text
        UpdateSpeedLabel(val);
    }

    // Called when the Offset Slider is moved
    public void OnOffsetChanged(float val)
    {
        // Update the Singleton
        if (GlobalSettings.Instance != null)
        {
            GlobalSettings.Instance.audioOffset = val;
        }

        // Update the visual text
        UpdateOffsetLabel(val);
    }

    private void UpdateSpeedLabel(float val)
    {
        speedValueText.text = val.ToString("F1");
    }

    private void UpdateOffsetLabel(float val)
    {
        offsetValueText.text = val.ToString("F2");
    }

    public void BackButton()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}