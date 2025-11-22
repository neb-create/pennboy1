using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    public static GlobalSettings Instance;

    [Header("Persistent Settings")]
    // Default values matching your GameManager
    public float scrollSpeed = 9f;
    public float audioOffset = 0.42f;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This makes it survive scene changes
        }
        else
        {
            // If a duplicate exists (e.g., returning to menu), destroy this new one
            Destroy(gameObject);
        }
    }

    // Helper methods for UI Sliders to call
    public void SetScrollSpeed(float value)
    {
        scrollSpeed = value;
    }

    public void SetOffset(float value)
    {
        audioOffset = value;
    }
}