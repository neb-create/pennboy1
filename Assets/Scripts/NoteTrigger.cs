using UnityEngine;
using TMPro;

public class NoteTrigger : MonoBehaviour
{

    KeyCode keyCode;
    string keyDisplay;

    float nextNoteTime = 0f;
    Vector3 nextNotePosition;

    public TextMeshPro textMesh;

    // Update the keyCode and display
    public void setKeyCode(KeyCode kc)
    {
        keyCode = kc;
        keyDisplay = kc.ToString();
        textMesh.text = keyDisplay;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
