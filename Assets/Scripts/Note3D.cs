using UnityEngine;

public class Note3D : MonoBehaviour
{
    // Note Settings
    // time: the time frame where you should hit the note
    public KeyCode key;
    public GameObject targetObject;
    public float time;
    public bool triggered;

    public void Trigger() {
        triggered = true;
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
