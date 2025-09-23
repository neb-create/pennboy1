using UnityEngine;

public class RhythmNote : MonoBehaviour
{

    // Note Settings
    // time: the time frame where you should hit the note
    public float time;
    public int lane;
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
