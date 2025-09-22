using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{

    // Note Spawning System
    int bpm = 60;
    int fps = 120;
    float time_current = 0.0f; 
    float time_nextnote = 1.0f;

    // Input state var - 0 (space) 1 2 (left) 3 4 (right)
    bool[] InputState = new bool[5];
    const int KEY_MIDDLE = 0;
    const int KEY_LEFT_1 = 1;
    const int KEY_LEFT_2 = 2;
    const int KEY_RIGHT_1 = 3;
    const int KEY_RIGHT_2 = 4;

    public GameObject NotePrefab;


    void HandleKeyDown(int keyId) {
        // Handle Note Detection Thingies

        // Update State
        InputState[keyId] = true;

        Debug.Log("Key DOWN " + keyId);
    }
    void HandleKeyUp(int keyId) {

        // Update State
        InputState[keyId] = false;

        Debug.Log("Key UP " + keyId);
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

    void SpawnNote(int lane, int type) {

        Instantiate(NotePrefab, new Vector3(0.0f + (lane - 1.5f) * 1.0f, 0f, 0f), Quaternion.identity);

    }

    void HandleNotes() {

        // Spawn Notes
        time_current += Time.deltaTime;
        while (time_current >= time_nextnote) {

            time_nextnote += 60f/bpm;

            int note_lane = Random.Range(1, 5);
            int note_type = 0;
            SpawnNote(note_lane, note_type);
        
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
