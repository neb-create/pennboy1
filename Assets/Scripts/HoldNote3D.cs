using UnityEngine;

public class HoldNote3D : Note3D
{
    public float time_end;
    public bool on_hold = false;

    float scale_const = 0.3f;

    public override float GetDespawnTime()
    {
        return time_end;
    }

    public override GameManager.NoteType GetNoteType()
    {
        return GameManager.NoteType.HOLD;
    }
    
    public override float GetDistance(float t, KeyCode k)
    {
        if (on_hold) {
            if (k != key) return float.MaxValue;
            return Mathf.Abs(time_end - t);
        } else
        {
            if (triggered) return float.MaxValue;
            if (k != key) return float.MaxValue;
            return Mathf.Abs(time - t);
        }
    }

    public override void UpdatePosition(float time_current, float note_speed)
    {

        Vector3 desired_pos = targetObject.transform.position;
        float t = time_current - this.time;
        Vector3 note_direction = new Vector3(0.0f, 0.0f, -1.0f);

        transform.position = desired_pos + t * note_direction * note_speed;

        // Update hold note components
        Transform c0 = transform.Find("C0");
        Transform c1 = transform.Find("C1");
        Transform p0 = transform.Find("P0");

        if (c0 != null && on_hold)
        {
            c0.localPosition = -t * note_direction * note_speed / scale_const;
        }

        if (c1 != null)
        {
            c1.localPosition = - (time_end - time) * note_direction * note_speed / scale_const;
        }

        if (p0 != null)
        {

            if (on_hold)
            {
                float dist = (time_end - time_current) * note_speed / scale_const;

                if (dist < 0f) dist = 0f;
                p0.localPosition = (-t * note_direction * note_speed / scale_const) - (dist / 2f * note_direction);
                p0.localScale = new Vector3(p0.localScale.x, p0.localScale.y, dist);
            }
            else
            {
                float dist = -(time_end - time) * note_speed / scale_const;
                p0.localPosition = dist / 2f * note_direction;
                p0.localScale = new Vector3(p0.localScale.x, p0.localScale.y, dist);
            }


            /* ParticleSystem ps = p0.GetComponent<ParticleSystem>();
            var shape = ps.shape;
            Vector3 boxSize = shape.scale;
            boxSize.z = - (dist * scale_const) - 1.2f;
            shape.scale = boxSize; */

        }

    }
    
}
