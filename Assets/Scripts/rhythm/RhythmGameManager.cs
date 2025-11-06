using UnityEngine;
using System.Collections.Generic;

public class RhythmGameManager : MonoBehaviour
{

    // Score System
    public int score = 0;
    public int current_combo = 0;
    public int max_combo = 0;
    public int perfect_count = 0;
    public int great_count = 0;
    public int miss_count = 0;
    const int SCORE_PERFECT = 100;
    const int SCORE_GREAT = 50;
    const int PERFECT = 1;
    const int GREAT = 2;
    const int MISS = 0;
    const float JUDGEMENT_PERFECT_WINDOW = 0.08f;
    const float JUDGEMENT_GREAT_WINDOW = 0.32f;

    // Note Spawning System
    public int bpm = 120;
    public float time_current = -3.0f;
    float time_nextnote = 1.0f;

    // Note Moving System
    List<GameObject> ActiveNotes = new List<GameObject>();
    public float note_speed = 9f;
    public float note_height_origin = 22f;
    float note_height_spawn = 6.0f;
    float note_height_despawn = -6.0f;
    public float note_prespawn_time;

    // Input state var - 0 (space) 1 2 (left) 3 4 (right)
    bool[] InputState = new bool[5];
    const int KEY_MIDDLE = 0;
    const int KEY_LEFT_1 = 1;
    const int KEY_LEFT_2 = 2;
    const int KEY_RIGHT_1 = 3;
    const int KEY_RIGHT_2 = 4;

    public GameObject NotePrefab;
    public GameObject NotePrefabFull;

    //VFX Prefabs
    [Header("VFX Prefabs")]
    public GameObject hitVFXPrefab;       
    public GameObject travelVFXPrefab;   
    public GameObject slideVFXPrefab;    

    public static RhythmGameManager instance;


    // Note Types
    public enum NoteType { TAP, HOLD, SLIDE, FULL };

    void HandleKeyDown(int keyId) {
        
        // Update State
        InputState[keyId] = true;

        // Find the closest note in lane
        int keyLane = keyId;
        GameObject hit_note = null;
        float hit_note_distance = float.MaxValue;
        foreach (GameObject note in ActiveNotes) {
            int note_lane = note.GetComponent<RhythmNote>().lane;
            float note_time = note.GetComponent<RhythmNote>().time;
            bool note_triggered = note.GetComponent<RhythmNote>().triggered;
            if (note_lane == keyLane && !note_triggered) {
                float note_distance = Mathf.Abs(time_current - note_time);
                if (note_distance < hit_note_distance) {
                    hit_note = note;
                    hit_note_distance = note_distance;
                }
            }
        }

        // check if the hit_note is valid
        if (hit_note != null)
        {
            if (hit_note_distance <= JUDGEMENT_GREAT_WINDOW)
            {
                ScoreNote(hit_note, hit_note_distance);
            }
        }
    }
    void HandleKeyUp(int keyId) {

        // Update State
        InputState[keyId] = false;

    }
    void HandleInput() {

        // Placeholder buttons space w e i o
        if (Input.GetKey("space") && !InputState[KEY_MIDDLE]) 
            HandleKeyDown(KEY_MIDDLE);
        if (Input.GetKey("w") && !InputState[KEY_LEFT_1]) 
            HandleKeyDown(KEY_LEFT_1);
        if (Input.GetKey("e") && !InputState[KEY_LEFT_2]) 
            HandleKeyDown(KEY_LEFT_2);
        if (Input.GetKey("i") && !InputState[KEY_RIGHT_1]) 
            HandleKeyDown(KEY_RIGHT_1);
        if (Input.GetKey("o") && !InputState[KEY_RIGHT_2]) 
            HandleKeyDown(KEY_RIGHT_2);

        if (!Input.GetKey("space") && InputState[KEY_MIDDLE]) 
            HandleKeyUp(KEY_MIDDLE);
        if (!Input.GetKey("w") && InputState[KEY_LEFT_1]) 
            HandleKeyUp(KEY_LEFT_1);
        if (!Input.GetKey("e") && InputState[KEY_LEFT_2]) 
            HandleKeyUp(KEY_LEFT_2);
        if (!Input.GetKey("i") && InputState[KEY_RIGHT_1]) 
            HandleKeyUp(KEY_RIGHT_1);
        if (!Input.GetKey("o") && InputState[KEY_RIGHT_2]) 
            HandleKeyUp(KEY_RIGHT_2);

    }

    void ScoreNote(GameObject note, float accuracy) {
        
        note.GetComponent<RhythmNote>().Trigger();

        if (accuracy <= JUDGEMENT_PERFECT_WINDOW) {
            current_combo += 1;
            max_combo = Mathf.Max(max_combo, current_combo);
            score += SCORE_PERFECT;
            perfect_count += 1;

            // =Spawn VFX on Hit
            if (hitVFXPrefab != null)
            {
                Vector3 hitPos = note.transform.position;
                GameObject vfx = Instantiate(hitVFXPrefab, hitPos, Quaternion.identity);
                Destroy(vfx, 1f); // 
            }

            DestroyNote(note);
        }
        else if (accuracy <= JUDGEMENT_GREAT_WINDOW) {
            current_combo += 1;
            max_combo = Mathf.Max(max_combo, current_combo);
            score += SCORE_GREAT;
            great_count += 1;

            // =Spawn VFX on Hit
            if (hitVFXPrefab != null)
            {
                Vector3 hitPos = note.transform.position;
                GameObject vfx = Instantiate(hitVFXPrefab, hitPos, Quaternion.identity);
                Destroy(vfx, 1f); // 
            }

            DestroyNote(note);
        }
        else {
            current_combo = 0;
            miss_count += 1;
        }
        
    }

    void SpawnNote(int lane, float time, NoteType type) {

        GameObject NoteToSpawn;
        switch (type)
        {
            case NoteType.FULL:
                NoteToSpawn = NotePrefabFull;
                lane = 0;
                break;
            default:
                NoteToSpawn = NotePrefab;
                break;
        }
        
        GameObject new_note = Instantiate(NoteToSpawn, new Vector3(0f,0f,0f), Quaternion.identity);
        new_note.GetComponent<RhythmNote>().lane = lane;
        new_note.GetComponent<RhythmNote>().time = time;
        
        // Attach moving glow
        if (travelVFXPrefab != null)
        {
            GameObject trail = Instantiate(travelVFXPrefab, new_note.transform.position, Quaternion.identity, new_note.transform);
        }

        ActiveNotes.Add(new_note);

    }
    void DestroyNote(GameObject note) {
        ActiveNotes.Remove(note);
        Destroy(note);
    }

    Vector3 GetJudgementPosByLaneId(int lane) {

        float x_pos = 0.0f;
        if (lane >= 1 || lane <= 4) x_pos = (lane - 1.0f) * 2.8f - 4.2f;
        return new Vector3(x_pos, -3.0f, 0.0f);
    }

    void HandleNotes() {

        // Setup
        List<GameObject> ToRemoveNotes = new List<GameObject>();

        // Spawn Notes
        time_current += Time.deltaTime;
        while (time_current >= (time_nextnote - note_prespawn_time) ) {

            time_nextnote += 60.0f/bpm;

            int note_lane = UnityEngine.Random.Range(1, 6); // [1 - 6]


            if (note_lane <= 4 && note_lane >= 1) {

                SpawnNote(note_lane, time_nextnote, NoteType.TAP);

            } else if (note_lane == 5) {
                int note_lane1 = UnityEngine.Random.Range(1, 5);
                int note_lane2 = UnityEngine.Random.Range(1, 4);
                note_lane2 = (note_lane1 + note_lane2) % 4 + 1;
                
                SpawnNote(note_lane1, time_nextnote, NoteType.TAP);
                SpawnNote(note_lane2, time_nextnote, NoteType.TAP);

            } else if (note_lane == 6) {
                int note_lane1 = UnityEngine.Random.Range(1, 5);
                int note_lane2 = UnityEngine.Random.Range(1, 5);
                
                SpawnNote(note_lane1, time_nextnote, NoteType.TAP);
                SpawnNote(note_lane2, time_nextnote + 30.0f/bpm, NoteType.TAP);
            } else if (note_lane == 7) {
                SpawnNote(0, time_nextnote, NoteType.FULL);
            }
        
        }

        //Update Notes
        foreach (GameObject note in ActiveNotes) {

            int note_lane = note.GetComponent<RhythmNote>().lane;
            float note_time = note.GetComponent<RhythmNote>().time;
            bool note_triggered = note.GetComponent<RhythmNote>().triggered;

            Vector3 desired_pos = GetJudgementPosByLaneId(note_lane);
            float t = time_current - note_time;
            Vector3 note_direction = (desired_pos - new Vector3(0.0f, note_height_origin, 0.0f)).normalized;

            note.transform.position = desired_pos + t * note_direction * note_speed;

            //despawn notes
            if (note_time + JUDGEMENT_GREAT_WINDOW < time_current) {
                if (!note_triggered) ScoreNote(note, 100.0f);
                if (note.transform.position.y < note_height_despawn) ToRemoveNotes.Add(note);
            }

        }

        // Cleanup 
        foreach (GameObject note in ToRemoveNotes) {

            DestroyNote(note);

        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        note_prespawn_time = (note_height_spawn - note_height_despawn) / note_speed;
    }

    // Update is called once per frame
    void Update()
    {
        
        // Handle Notes
        HandleNotes();

        // Handle Player Input
        HandleInput();
        
    }
}