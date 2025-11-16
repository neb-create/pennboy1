using UnityEngine;

public class Note3D : MonoBehaviour
{
    // Note Settings
    // time: the time frame where you should hit the note
    public KeyCode key;
    public GameObject targetObject;
    public float time;
    public bool triggered;

    // Note ID for editor
    public int id;

    public void Trigger() {
        triggered = true;
    }

    public virtual float GetDespawnTime()
    {
        return time;
    }
    public virtual float GetDistance(float t, KeyCode k)
    {
        if (triggered) return float.MaxValue;
        if (k != key) return float.MaxValue;
        return Mathf.Abs(time - t);
    }

    public virtual void UpdatePosition(float time_current, float note_speed)
    {
        
        Vector3 desired_pos = targetObject.transform.position;
        float t = time_current - this.time;
        Vector3 note_direction = new Vector3(0.0f, 0.0f, -1.0f);

        transform.position = desired_pos + t * note_direction * note_speed;

    }

    public virtual GameManager.NoteType GetNoteType()
    {
        return GameManager.NoteType.TAP;
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
