using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{

    // Score System
    [Header("Game Score")]
    public int score = 0;
    public int current_combo = 0;
    public int max_combo = 0;
    public int perfect_count = 0;
    public int great_count = 0;
    public int bad_count = 0;
    public int miss_count = 0;
    const int SCORE_PERFECT = 100;
    const int SCORE_GREAT = 50;
    const int SCORE_BAD = 0;
    const int PERFECT = 1;
    const int GREAT = 2;
    const int BAD = 3;
    const int MISS = 0;
    const float JUDGEMENT_PERFECT_WINDOW = 0.08f;
    const float JUDGEMENT_GREAT_WINDOW = 0.24f;
    const float JUDGEMENT_BAD_WINDOW = 0.32f;

    [Header("Game Settings")]
    // Note Spawning System
    public int bpm = 120;

    // Note Moving System
    List<GameObject> ActiveNotes = new List<GameObject>();
    Dictionary<KeyCode, GameObject> keyToObjMap = new Dictionary<KeyCode, GameObject>();
    public float note_speed = 9f;
    public float note_height_origin = 22f;
    float note_z_spawn = 12.0f;
    float note_z_despawn = -6.0f;
    float note_prespawn_time;
    public float time_current = -2.0f;
    public float time_offset = -2.0f;
    float time_nextnote = 1.0f;

    // Input state var - 0 (space) 1 2 (left) 3 4 (right)
    //bool[] InputState = new bool[5];
    const int KEY_MIDDLE = 0;
    const int KEY_LEFT_1 = 1;
    const int KEY_LEFT_2 = 2;
    const int KEY_RIGHT_1 = 3;
    const int KEY_RIGHT_2 = 4;

    [Header("Note Prefab")]
    public GameObject NotePrefab;
    public GameObject NotePrefabFull;
    public GameObject NoteTriggerPrefab;
    public GameObject NoteTriggerPlayerPrefab;

    //VFX Prefabs
    [Header("VFX Prefabs")]
    public GameObject hitVFXPrefab;       
    public GameObject travelVFXPrefab;
    public GameObject slideVFXPrefab;

    [Header("Config")]
    // Player Object
    public GameObject player;

    public static GameManager instance;

    // Note Types
    public enum NoteType { TAP, HOLD, SLIDE, FULL };

    public enum GameState { RHYTHM, BULLET };
    public GameState gamestate = GameState.RHYTHM;

    // Player Key 
    KeyCode playerKey = KeyCode.Space;
    // List of Keys used in gameplay
    List<KeyCode> keyList = new List<KeyCode>() { KeyCode.W, KeyCode.E, KeyCode.Space, KeyCode.I, KeyCode.O };

    // Beatmap Loading
    private List<Beatmap.NoteInfo> noteInfos;
    private int currNote;
    public bool randomSpawnMode = true;

    void swapGameState()
    {
        
        // Clean Up
        keyToObjMap.Clear();
        foreach (Transform child in player.transform)
        {
            DestroyImmediate(child.gameObject);
        };

        if (gamestate == GameState.RHYTHM)
            gamestate = GameState.BULLET;
        else
            gamestate = GameState.RHYTHM;
    }

    void swapToBulletHell()
    {
        // Swap: 
        gamestate = GameState.RHYTHM;
        swapGameState();

        // Todo... bullet hell setup..


    }

    void swapToRhythmGame()
    {
        // Swap: 
        gamestate = GameState.BULLET;
        swapGameState();

        // TODO: cool special effects


        int i = -keyList.Count/2 - 1;
        foreach (KeyCode k in keyList)
        {
            i += 1;
            // Skip Player Key
            if (playerKey == k) continue;
            // Create Note Triggers
            GameObject newObj = Instantiate(NoteTriggerPrefab, player.transform);

            newObj.transform.localPosition += new Vector3( 1.3f * i, 0.3f * Mathf.Abs(i), 0f );

            newObj.GetComponent<NoteTrigger>().setKeyCode(k);

            keyToObjMap[k] = newObj;

        }
        
        // find the player note trigger object
        GameObject pObj = Instantiate(NoteTriggerPlayerPrefab, player.transform);
        pObj.GetComponent<NoteTrigger>().setKeyCode(KeyCode.Space);
        keyToObjMap[KeyCode.Space] = pObj;

    }

    void SpawnNote(KeyCode key, float time, NoteType type) {

        GameObject NoteToSpawn;
        switch (type)
        {
            case NoteType.FULL:
                NoteToSpawn = NotePrefabFull;
                key = KeyCode.Space;
                break;
            default:
                NoteToSpawn = NotePrefab;
                break;
        }
        
        GameObject new_note = Instantiate(NoteToSpawn, new Vector3(0f,0f,0f), Quaternion.identity);
        new_note.GetComponent<Note3D>().key = key;
        new_note.GetComponent<Note3D>().targetObject = keyToObjMap[key];

        new_note.GetComponent<Note3D>().time = time;
        
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

    


    // Handle Input Scection - 
    // This part of the code handles the player action of pressing any of the keys.
    // Needs to update to adapt to being able to use any key..
    void HandleInput()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
                if (keyToObjMap.ContainsKey(key))
                    HandleKeyDown(key);

            if (Input.GetKeyUp(key))
                if (keyToObjMap.ContainsKey(key))
                    HandleKeyUp(key);
        }
    }

    void HandleKeyDown(KeyCode keyId) {
        
        // Update State
        //InputState[keyId] = true;

        // Find the closest note in lane
        GameObject hit_note = null;
        float hit_note_distance = float.MaxValue;
        foreach (GameObject note in ActiveNotes) {
            KeyCode noteKey = note.GetComponent<Note3D>().key;
            float note_time = note.GetComponent<Note3D>().time;
            bool note_triggered = note.GetComponent<Note3D>().triggered;
            if (noteKey == keyId && !note_triggered) {
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
            if (hit_note_distance <= JUDGEMENT_BAD_WINDOW)
            {
                ScoreNote(hit_note, hit_note_distance);
            }
        }

        //update visual disk 
        keyToObjMap[keyId].GetComponent<NoteTrigger>().Disk.GetComponent<VisualDisk>().SwitchToPressed();

    }
    void HandleKeyUp(KeyCode keyId) {

        keyToObjMap[keyId].GetComponent<NoteTrigger>().Disk.GetComponent<VisualDisk>().SwitchToUnpressed();

        //Debug.Log("Key Up: " + keyId.ToString());

        // Update State
        //InputState[keyId] = false;

    }
    void ScoreNote(GameObject note, float accuracy) {
        
        note.GetComponent<Note3D>().Trigger();

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
                Destroy(vfx, 1f); 
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
        else if (accuracy <= JUDGEMENT_BAD_WINDOW) {
            current_combo = 0;
            score += SCORE_BAD;
            bad_count += 1;

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




    KeyCode LaneIDToKey(int laneID)
    {
        switch (laneID)
        {
            case 1:
                return KeyCode.W;
            case 2:
                return KeyCode.E;
            case 3:
                return KeyCode.I;
            case 4:
                return KeyCode.O;
            default:
                return KeyCode.Space;
        }
    }
    void HandleNotes() {

        // Setup
        List<GameObject> ToRemoveNotes = new List<GameObject>();

        if (currNote == noteInfos.Count)
        {
            Debug.Log("Finished Song");
            currNote++;
        }

        // Spawn Notes
        time_current += Time.deltaTime;
        if (randomSpawnMode) {

            // Random Spawn Mode
            while (time_current >= (time_nextnote - note_prespawn_time) ) {

                time_nextnote += 60.0f/bpm;

                int note_lane = UnityEngine.Random.Range(1, 12); // [1 - 4]

                if (note_lane <= 4 && note_lane >= 1) {

                    SpawnNote(LaneIDToKey(note_lane), time_nextnote, NoteType.TAP);

                } else if (note_lane == 5 || note_lane == 8 || note_lane == 10) {
                int note_lane1 = UnityEngine.Random.Range(1, 5);
                int note_lane2 = UnityEngine.Random.Range(1, 4);
                note_lane2 = (note_lane1 + note_lane2 - 1) % 4 + 1;

                    SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.TAP);
                    SpawnNote(LaneIDToKey(note_lane2), time_nextnote, NoteType.TAP);

                } else if (note_lane == 6 || note_lane == 9 || note_lane == 11) {
                    int note_lane1 = UnityEngine.Random.Range(1, 5);
                    int note_lane2 = UnityEngine.Random.Range(1, 5);

                    SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.TAP);
                    SpawnNote(LaneIDToKey(note_lane2), time_nextnote + 30.0f/bpm, NoteType.TAP);
                } else if (note_lane == 7) {
                    SpawnNote(LaneIDToKey(0), time_nextnote, NoteType.FULL);
                }

            }

        } else {

            // Load Beatmap Mode
            while (currNote < noteInfos.Count && time_current >= (noteInfos[currNote].start_time - note_prespawn_time) ) {
            
                switch (noteInfos[currNote].note_type)
                {
                    case Beatmap.NoteInfo.BASIC_NOTE:
                        SpawnNote(LaneIDToKey(int.Parse(noteInfos[currNote].extra_info[0])), noteInfos[currNote].start_time, NoteType.TAP);
                        break;
                    case Beatmap.NoteInfo.HOLD_NOTE:
                        Debug.Log("HOLD_NOTE_PLAYED");
                        break;
                    case Beatmap.NoteInfo.SPACE_NOTE:
                        SpawnNote(0, noteInfos[currNote].start_time, NoteType.FULL);
                        break;
                    case Beatmap.NoteInfo.SLIDE_NOTE:
                        Debug.Log("SLIDE_NOTE_PLAYED");
                        break;
                }
                currNote++;
            }
        }

        //Update Notes
        foreach (GameObject note in ActiveNotes) {

            KeyCode note_key = note.GetComponent<Note3D>().key;
            float note_time = note.GetComponent<Note3D>().time;
            bool note_triggered = note.GetComponent<Note3D>().triggered;

            Vector3 desired_pos = keyToObjMap[note_key].transform.position;
            float t = time_current - note_time;
            Vector3 note_direction = new Vector3(0.0f, 0.0f, -1.0f);//(desired_pos - new Vector3(0.0f, note_height_origin, 0.0f)).normalized;

            note.transform.position = desired_pos + t * note_direction * note_speed;

            //despawn notes
            if (note_time + JUDGEMENT_BAD_WINDOW < time_current) {
                if (!note_triggered) ScoreNote(note, 100.0f);
                if (note.transform.position.z < note_z_despawn) ToRemoveNotes.Add(note);
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
        note_prespawn_time = (note_z_spawn - note_z_despawn) / note_speed;
        noteInfos = Beatmap.LoadBeatmap("Beatmap");
        currNote = 0;

        swapToRhythmGame();

    }

    // Update is called once per frame
    void Update()
    {

        // Handle Notes
        HandleNotes();
        // Handle Player Input
        HandleInput();
        
        
        if (time_current > 0.0f && !GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().Play();
            time_current = time_offset;
        }

    }
}