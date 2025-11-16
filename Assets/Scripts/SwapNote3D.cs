using UnityEngine;

public class SwapNote3D : Note3D
{
    public override float GetDespawnTime()
    {
        return time;
    }
    public override GameManager.NoteType GetNoteType()
    {
        return GameManager.NoteType.SWAP;
    }
    public override void UpdatePosition(float time_current, float note_speed)
    {
        
        Vector3 desired_pos = targetObject.transform.position;
        float t = time_current - this.time;
        Vector3 note_direction = new Vector3(0.0f, 0.0f, -1.0f);

        if (t <= 0f)
        {
            transform.position = desired_pos + t * note_direction * note_speed;
        }
        else
        {
            transform.position = desired_pos;
        }

    }
    
}
