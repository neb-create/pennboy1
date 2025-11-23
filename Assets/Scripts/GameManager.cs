using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Pool;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;

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
    public int bpm = 174;

    // Note Moving System
    List<GameObject> ActiveNotes = new List<GameObject>();
    Dictionary<KeyCode, GameObject> keyToObjMap = new Dictionary<KeyCode, GameObject>();
    public float note_speed = 9f;
    private float og_note_speed;
    public float note_height_origin = 22f;
    float note_z_spawn = 12.0f;
    float note_z_despawn = -6.0f;
    float note_prespawn_time;
    float note_postspawn_time;
    public float time_current = -2.0f;
    public float time_offset = 0.0f;
    float time_nextnote = 5.0f;
    float time_nextnote_deco = 5.0f;
    public float global_offset = 0.0f;

    // Input state var - 0 (space) 1 2 (left) 3 4 (right)
    //bool[] InputState = new bool[5];
    const int KEY_MIDDLE = 0;
    const int KEY_LEFT_1 = 1;
    const int KEY_LEFT_2 = 2;
    const int KEY_RIGHT_1 = 3;
    const int KEY_RIGHT_2 = 4;

    [Header("Note Prefab")]
    [SerializeField] private GameObject NotePrefabTap;
    [SerializeField] private GameObject NotePrefabHold;
    [SerializeField] private GameObject NotePrefabSlide;
    [SerializeField] private GameObject NotePrefabSwap;
    [SerializeField] private GameObject NotePrefabDeco;
    [SerializeField] private Material NoteMaterialSpace;
    [SerializeField] private GameObject NoteTriggerPrefab;
    [SerializeField] private GameObject NoteTriggerPlayerPrefab;

    //VFX Prefabs
    [Header("VFX Prefabs")]
    [SerializeField] private GameObject hitVFXPrefab;       
    [SerializeField] private GameObject travelVFXPrefab;
    [SerializeField] private GameObject slideVFXPrefab;

    [Header("Config")]
    public bool allowEditor = false;
    private bool editorMode = false;
    public bool showTimeline = false;
    public List<NoteEditor> editorSelectedNotes = new List<NoteEditor>();
    [SerializeField] private Slider timeline;
    [SerializeField] private TMP_Text playPauseLabel;
    [SerializeField] private TMP_Text editorPauseLabel;
    [SerializeField] private Button playPauseButton;
    [SerializeField] private TMP_Text editorSnapButtonLabel;
    public float editorSnapScale = 0.25f;
    bool paused = true;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform editorContainer;
    [SerializeField] private Transform timelineContainer;
    // Player Object
    public GameObject player;
    public GameObject gameCamera;

    public static GameManager instance;

    // Note Types
    public enum NoteType { TAP, HOLD, SLIDE, SWAP, DECO };

    public enum GameState { RHYTHM, BULLET };
    public GameState gamestate = GameState.RHYTHM;
    private bool swapToBulletFlag = false;
    private bool swapToRhythmFlag = false;

    // Player Key 
    KeyCode playerKey = KeyCode.Space;
    // List of Keys used in gameplay
    List<KeyCode> keyList = new List<KeyCode>() { KeyCode.W, KeyCode.E, KeyCode.Space, KeyCode.I, KeyCode.O };

    // Beatmap Loading
    private List<Beatmap.NoteInfo> noteInfos;
    private int currNote;
    public bool randomSpawnMode = true;
    public bool spawnSwapNotes = true;
    public bool finishedSong = false;

    [Header("UI/VFX")]
    // UI Text
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI scoreTypeText;
    [SerializeField] private GameObject comboVfx;
    [SerializeField] private TextMeshProUGUI perfectText;
    [SerializeField] private TextMeshProUGUI greatText;
    [SerializeField] private TextMeshProUGUI badText;
    [SerializeField] private TextMeshProUGUI missText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI maxComboText;
    [SerializeField] private TextMeshProUGUI comboTextStatic;
    private Color combo_inc_color;
    private Color combo_init_color;

    [Header("Bullet Hell")]
    public GameObject PlaceholderDefaultEmitter;
    public ObjectPool<GameObject> masterProjectilePool;
    public int defaultPoolSize = 50;
    public int maxPoolSize = 100;
    [SerializeField] GameObject bulletPrefab;
    [Header("int is number beats after prev one spawned or bullet hell began, whichever comes later")]
    public List<Pair> emitters = new List<Pair>();
    private int emitterIndex = 0;
    private float timePerBeat = 0.5f;
    private float timeLastEmitterSetActive;
    private float bulletHellStartTime;
    [SerializeField] GameObject boundingBoxes;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource hitSound;


    void swapGameState()
    {

        swapToRhythmFlag = false;
        swapToBulletFlag = false;

        // Clean Up player
        player.transform.position = Vector3.zero;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        player.GetComponent<Rigidbody>().Sleep();   
        // Clean Up rhythm game stuff
        keyToObjMap.Clear();
        for (int i = player.transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = player.transform.GetChild(i).gameObject;
            if(child.GetComponent<TextMeshPro>() != null) continue; //don't destroy the expression obj

            DestroyImmediate(child);
        }
        for (int i = ActiveNotes.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(ActiveNotes[i]);
        }
        ActiveNotes.Clear();

        if (gamestate == GameState.RHYTHM)
            gamestate = GameState.BULLET;
        else
            gamestate = GameState.RHYTHM;
    }

    void swapToRhythmGame()
    {
        //clear bullet hell stuff
        if (masterProjectilePool != null)
        {
            masterProjectilePool.Clear();
        }
        boundingBoxes.SetActive(false);
        ClearBulletHell();
        
        // Swap: 
        gamestate = GameState.BULLET;
        swapGameState();

        // TODO: cool special effects
        gameCamera.GetComponent<GameCamera>().TransitionToRhythmGame();


        int i = -keyList.Count/2 - 1;
        foreach (KeyCode k in keyList)
        {
            i += 1;
            // Skip Player Key
            if (playerKey == k) continue;
            // Create Note Triggers
            GameObject newObj = Instantiate(NoteTriggerPrefab, player.transform);

            newObj.transform.localPosition += new Vector3( 1.32f * i, 0.12f * Mathf.Abs(i), 0f );

            newObj.GetComponent<NoteTrigger>().setKeyCode(k);

            keyToObjMap[k] = newObj;

        }
        
        // find the player note trigger object
        GameObject pObj = Instantiate(NoteTriggerPlayerPrefab, player.transform);
        pObj.GetComponent<NoteTrigger>().setKeyCode(KeyCode.Space);
        keyToObjMap[KeyCode.Space] = pObj;

    }

    void SpawnNote(KeyCode key, float time, NoteType type, float length = 0.0f, int id = 0) {

        GameObject NoteToSpawn;
        if (length <= 0.0f)
        {
            length = 60f/bpm/1f;
        }
        float time_end = time + length;
        switch (type)
        {
            case NoteType.HOLD:
                NoteToSpawn = NotePrefabHold;
                break;
            case NoteType.SWAP:
                NoteToSpawn = NotePrefabSwap;
                key = KeyCode.Space;
                break;
            case NoteType.SLIDE:
                NoteToSpawn = NotePrefabSlide;
                break;
            case NoteType.DECO:
                NoteToSpawn = NotePrefabDeco;
                key = KeyCode.Space;
                break;
            default:
                NoteToSpawn = NotePrefabTap;
                break;
        }

        if (NoteToSpawn == NotePrefabSlide)
        {
            return;
        }
        
        GameObject new_note = Instantiate(NoteToSpawn, new Vector3(0f,0f,0f), Quaternion.identity);
        new_note.GetComponent<Note3D>().key = key;
        new_note.GetComponent<Note3D>().targetObject = keyToObjMap[key];

        new_note.GetComponent<Note3D>().time = time;

        new_note.GetComponent<Note3D>().id = id;

        if (type == NoteType.HOLD)
            new_note.GetComponent<HoldNote3D>().time_end = time_end;

        // Set Color
        if (key == KeyCode.Space && type != NoteType.DECO)
        {
            Renderer[] childRenderers = new_note.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in childRenderers)
            {
                rend.material = NoteMaterialSpace;
            }
        }
        
        // Attach moving glow
        if (travelVFXPrefab != null && !editorMode && type != NoteType.DECO) 
        {
            GameObject trail = Instantiate(travelVFXPrefab, new_note.transform.position, Quaternion.identity, new_note.transform.GetChild(0));
        }

        // Editor Mode
        if (editorMode && type != NoteType.DECO)
        {
            // Add NoteEditor if missing
            if (new_note.GetComponent<NoteEditor>() == null)
            {
                NoteEditor editor = new_note.AddComponent<NoteEditor>();
                editor.note3D = new_note.GetComponent<Note3D>();
            }
            // Add SphereCollider if missing
            if (new_note.GetComponent<Collider>() == null)
            {
                SphereCollider col = new_note.AddComponent<SphereCollider>();
                col.isTrigger = true; // so it doesn't block anything
                col.radius = 2.5f;    // adjust as needed
            }
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
    public (GameObject, float) GetClosestNote(float t, KeyCode k)
    {
        GameObject hit_note = null;
        float closest_note_distance = float.MaxValue;
        foreach (GameObject note in ActiveNotes) {

            // skip decoration notes
            if (note.GetComponent<Note3D>().IsDecoration()) continue;

            float note_distance = note.GetComponent<Note3D>().GetDistance(t, k);
            if (note_distance < closest_note_distance) {
                hit_note = note;
                closest_note_distance = note_distance;
            }
        }

        return (hit_note, closest_note_distance);
    }
    public (GameObject, float) GetActiveHoldNote(float t, KeyCode k)
    {
        foreach (GameObject note in ActiveNotes) {

            // check if key matches
            if (note.GetComponent<Note3D>().key != k) continue;
            // check if it is a hold note
            if (!(note.GetComponent<Note3D>() is HoldNote3D)) continue;
            // check if it is on hold
            if (!note.GetComponent<HoldNote3D>().on_hold) continue;

            // else, return note
            return (note, note.GetComponent<HoldNote3D>().GetDistance(t, k));
        }

        return (null, float.MaxValue);
    }
    void HandleKeyDown(KeyCode keyId) {
        
        // Find the closest note in lane
        (GameObject hit_note, float hit_note_distance) = GetClosestNote(time_current, keyId);
        hitSound.Play();
        // check if the hit_note is valid
        if (hit_note != null)
        {
            if (hit_note_distance <= JUDGEMENT_BAD_WINDOW)
            {
                
                ScoreNote(hit_note, hit_note_distance);
                // If hold note: don't destroy, else destory
                if (!(hit_note.GetComponent<Note3D>() is HoldNote3D))
                {
                    DestroyNote(hit_note);
                } else
                {
                    HoldNote3D hold_note = hit_note.GetComponent<HoldNote3D>();
                    hold_note.on_hold = true;
                }
            }
        }

        //update visual disk 
        keyToObjMap[keyId].GetComponent<NoteTrigger>().Disk.GetComponent<VisualDisk>().SwitchToPressed();

    }
    void HandleKeyUp(KeyCode keyId) {

        (GameObject hit_note, float hit_note_distance) = GetActiveHoldNote(time_current, keyId);

        if (hit_note != null)
        {
            
            ScoreNote(hit_note, hit_note_distance);
            hit_note.GetComponent<Note3D>().Trigger();
            DestroyNote(hit_note);

        }

        keyToObjMap[keyId].GetComponent<NoteTrigger>().Disk.GetComponent<VisualDisk>().SwitchToUnpressed();


    }

    void ScoreSwapNote(GameObject note) {
        
        swapToBulletFlag = true;

    }

    void ScoreNote(GameObject note, float accuracy) {

        if (note.GetComponent<Note3D>().IsDecoration()) return;

        scoreTypeText.color = Color.white;

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
            scoreTypeText.text = "PERFECT!";
            StartCoroutine(ScoreTypeTextAnimation());
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
            scoreTypeText.text = "GREAT!";
            StartCoroutine(ScoreTypeTextAnimation());

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
            scoreTypeText.text = "BAD";
            scoreTypeText.color = Color.red;
            StartCoroutine(ScoreTypeTextAnimation());

        }
        else
        {
            scoreTypeText.text = "MISS";
            scoreTypeText.color = Color.red; 
            StartCoroutine(ScoreTypeTextAnimation());

            current_combo = 0;
            miss_count += 1;
        }

        // if is swap note: trigger swap
        if (note.GetComponent<Note3D>() is SwapNote3D) {
            Debug.Log("got to swap note");
            ScoreSwapNote(note);
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
    int KeyToLaneID(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return 1;
            case KeyCode.E: return 2;
            case KeyCode.I: return 3;
            case KeyCode.O: return 4;
            default: return 0;
        }
    }

    void HandleNoteSpawning()
    {
        
        // Random Deco Nodes
        while (time_current >= (time_nextnote_deco - note_prespawn_time) ) {
            if (editorMode)
            {
                SpawnNote(LaneIDToKey(0), time_nextnote_deco, NoteType.DECO);
            }
            time_nextnote_deco += 60.0f/bpm;
        }

        // Spawn Notes
        if (randomSpawnMode) {

            // Random Spawn Mode
            while (time_current >= (time_nextnote - note_prespawn_time) ) {

                time_nextnote += 60.0f/bpm;

                int note_lane = UnityEngine.Random.Range(1, 25);

                if (note_lane <= 4 && note_lane >= 1) {

                    SpawnNote(LaneIDToKey(note_lane), time_nextnote, NoteType.TAP);

                } else if (note_lane <= 15 && note_lane >= 12) {

                    int hold_lane = note_lane - 11;
                    SpawnNote(LaneIDToKey(hold_lane), time_nextnote, NoteType.HOLD);
                    time_nextnote += 60.0f/bpm;

                }  else if (note_lane == 16 || note_lane == 17) {

                    int note_lane1 = UnityEngine.Random.Range(1, 5);
                    int note_lane1_limiter = note_lane1;
                    if (note_lane1_limiter == 3 || note_lane1_limiter == 1) note_lane1_limiter += 1;
                    int note_lane2 = UnityEngine.Random.Range(1, 3);
                    note_lane2 = (note_lane1_limiter + note_lane2 - 1) % 4 + 1;

                    SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.HOLD);
                    time_nextnote += 60.0f/bpm;
                    SpawnNote(LaneIDToKey(note_lane2), time_nextnote, NoteType.TAP);

                } else if (note_lane == 23) {

                    int note_lane1 = UnityEngine.Random.Range(1, 5);
                    int note_lane1_limiter = note_lane1;
                    if (note_lane1_limiter == 3 || note_lane1_limiter == 1) note_lane1_limiter += 1;
                    int note_lane2 = UnityEngine.Random.Range(1, 3);
                    note_lane2 = (note_lane1_limiter + note_lane2 - 1) % 4 + 1;
                    int note_lane3 = UnityEngine.Random.Range(1, 3);
                    note_lane3 = (note_lane1_limiter + note_lane3 - 1) % 4 + 1;

                    SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.HOLD);
                    SpawnNote(LaneIDToKey(note_lane2), time_nextnote + 30.0f/bpm, NoteType.TAP);
                    SpawnNote(LaneIDToKey(note_lane3), time_nextnote + 60.0f/bpm, NoteType.TAP);
                    time_nextnote += 60.0f/bpm;

                } else if (note_lane == 5 || note_lane == 8 || note_lane == 10 || note_lane == 21 || note_lane == 24) {

                    int note_lane1 = UnityEngine.Random.Range(1, 5);
                    int note_lane2 = UnityEngine.Random.Range(1, 4);
                    note_lane2 = (note_lane1 + note_lane2 - 1) % 4 + 1;

                    SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.TAP);
                    SpawnNote(LaneIDToKey(note_lane2), time_nextnote, NoteType.TAP);

                } else if (note_lane == 6 || note_lane == 9 || note_lane == 11 || note_lane == 22) {
                    int note_lane1 = UnityEngine.Random.Range(1, 5);
                    int note_lane2 = UnityEngine.Random.Range(1, 5);

                    SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.TAP);
                    SpawnNote(LaneIDToKey(note_lane2), time_nextnote + 30.0f/bpm, NoteType.TAP);
                } else if (note_lane == 7) {
                    SpawnNote(LaneIDToKey(0), time_nextnote, NoteType.TAP);
                } else if (note_lane == 18) {
                    SpawnNote(LaneIDToKey(0), time_nextnote, NoteType.HOLD);
                    time_nextnote += 60.0f/bpm;
                } else if (note_lane == 19) {

                    if (spawnSwapNotes) {
                        SpawnNote(LaneIDToKey(0), time_nextnote, NoteType.SWAP);
                        time_nextnote += 480.0f/bpm;
                    } else
                    {
                        int note_lane1 = UnityEngine.Random.Range(1, 5);
                        int note_lane2 = UnityEngine.Random.Range(1, 4);
                        note_lane2 = (note_lane1 + note_lane2 - 1) % 4 + 1;

                        SpawnNote(LaneIDToKey(note_lane1), time_nextnote, NoteType.HOLD);
                        SpawnNote(LaneIDToKey(note_lane2), time_nextnote, NoteType.HOLD);
                        time_nextnote += 60.0f/bpm;
                    }

                }

            }

        } else {

            // Load Beatmap Mode
            while (currNote < noteInfos.Count && time_current >= (noteInfos[currNote].start_time - note_prespawn_time) ) {
            
                switch (noteInfos[currNote].note_type)
                {
                    case Beatmap.NoteInfo.BASIC_NOTE:
                        //Debug.Log("BASIC_NOTE_PLAYED");
                        SpawnNote(LaneIDToKey(int.Parse(noteInfos[currNote].extra_info[0])), noteInfos[currNote].start_time, NoteType.TAP, id: currNote);
                        break;
                    case Beatmap.NoteInfo.HOLD_NOTE:
                        //Debug.Log("HOLD_NOTE_PLAYED");
                        //Debug.Log("test: " + noteInfos[currNote].extra_info[1]);
                        SpawnNote(LaneIDToKey(int.Parse(noteInfos[currNote].extra_info[0])), noteInfos[currNote].start_time, NoteType.HOLD, float.Parse(noteInfos[currNote].extra_info[1]), id: currNote);
                        break;
                    case Beatmap.NoteInfo.SPACE_NOTE:
                        //Debug.Log("SPACE_NOTE_PLAYED");
                        SpawnNote(KeyCode.Space, noteInfos[currNote].start_time, NoteType.TAP, id: currNote);
                        break;
                    case Beatmap.NoteInfo.SWAP_NOTE:
                        //Debug.Log("SPACE_NOTE_PLAYED");
                        SpawnNote(KeyCode.Space, noteInfos[currNote].start_time, NoteType.SWAP, id: currNote);
                        break;
                    case Beatmap.NoteInfo.SLIDE_NOTE:
                        //Debug.Log("SLIDE_NOTE_PLAYED");
                        Debug.Log("SlideNote handle attempt ");
                        float slideEndTime = float.Parse(noteInfos[currNote].extra_info[0]);
                        KeyCode slideStartKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), noteInfos[currNote].extra_info[1], true);
                        KeyCode slideEndKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), noteInfos[currNote].extra_info[2], true);

                        float posX = float.Parse(noteInfos[currNote].extra_info[3]);
                        float posY = float.Parse(noteInfos[currNote].extra_info[4]);
                        float posZ = float.Parse(noteInfos[currNote].extra_info[5]);

                        // Create the spawn position Vector3 from the parsed data
                        Vector3 spawnPos = new Vector3(posX, posY, posZ);

                        SpawnSlideNote(
                            noteInfos[currNote].start_time,
                            slideEndTime,
                            slideStartKey,
                            slideEndKey,
                            spawnPos, id: currNote
                        );
                        break;
                }
                currNote++;
            }
        }
    }
    void HandleNotes() {

        // Setup
        List<GameObject> ToRemoveNotes = new List<GameObject>();

        if (currNote == noteInfos.Count)
        {
            currNote++;
        }

        if (time_current > audioSource.clip.length )
        {
            Debug.Log("Finished Song");
            currNote++;
            comboVfx.SetActive(false);
            finishedSong = true;
            keyToObjMap.Clear();
            for (int i = player.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(player.transform.GetChild(i).gameObject);
            }
        }

        if (finishedSong)
        {
            scoreText.text = "";
            comboText.text = "";
            comboTextStatic.text = "";
            perfectText.text    = "PERFECT:   " + perfect_count;
            greatText.text      = "GREAT:     " + great_count;
            badText.text        = "BAD:       " + bad_count;
            missText.text       = "MISS:      " + miss_count;
            finalScoreText.text = "SCORE:     " + score;
            maxComboText.text   = "MAX COMBO: " + max_combo + "\nPress space to go Menu!";
            return;
        }

        // Spawn Notes
        HandleNoteSpawning();

        //Update Notes
        foreach (GameObject note in ActiveNotes) {
            
            note.GetComponent<Note3D>().UpdatePosition(time_current, note_speed);

            // handle miss & despawn feature
            if (note.GetComponent<Note3D>().GetDespawnTime() + JUDGEMENT_BAD_WINDOW < time_current) {
                if (!note.GetComponent<Note3D>().triggered) {
                    ScoreNote(note, 100.0f);
                    note.GetComponent<Note3D>().Trigger();
                }
                if (note.transform.position.z < note_z_despawn) {
                    ToRemoveNotes.Add(note);
                }
            }

        }

        // Cleanup 
        foreach (GameObject note in ToRemoveNotes) {

            DestroyNote(note);

        }

    }

    public void SpawnSlideNote(float startTime, float endTime, KeyCode startKey, KeyCode endKey, Vector3 spawnPosition, int id = 0)
    {
        Debug.Log("SlideNote spawn attempt " + startKey + " " + endKey);
        GameObject slideNoteObj = Instantiate(NotePrefabSlide, spawnPosition, Quaternion.Euler(50f, 0.0f, 0.0f));

        SlideNote slideNote = slideNoteObj.GetComponent<SlideNote>();
        if (slideNote == null)
        {
            Debug.LogError("SlideNotePrefab does not have a SlideNote component!");
            return;
        }

        slideNote.startTime = startTime;
        slideNote.endTime = endTime;
        slideNote.startKey = startKey;
        slideNote.endKey = endKey;
        slideNote.id = id;

        slideNote.transform.position = spawnPosition;

        //ActiveNotes.Add(slideNoteObj);
    }

    System.Collections.IEnumerator ComboAnimation()
    {
        float timer = 0f;
        float time_to_finish = 0.2f;
        float start_size = comboText.fontSize;
        Color start_color = comboText.color;
        while (timer < time_to_finish)
        {
            timer += Time.deltaTime;
            // Lerp between the initial scale and the target scale
            comboText.fontSize = Mathf.Lerp(start_size, 72, timer / time_to_finish);
            comboText.color = Color.Lerp(start_color, combo_inc_color, timer / time_to_finish);
            yield return null; // Wait for the next frame
        }

        StartCoroutine(ComboAnimationDec());
    }

    System.Collections.IEnumerator ComboAnimationDec()
    {
        float timer = 0f;
        float time_to_finish = 0.2f;
        while (timer < time_to_finish)
        {
            timer += Time.deltaTime;
            // Lerp between the initial scale and the target scale
            comboText.fontSize = Mathf.Lerp(72, 64, timer / time_to_finish);
            comboText.color = Color.Lerp(combo_inc_color, combo_init_color, timer / time_to_finish);
            yield return null; // Wait for the next frame
        }
        comboText.fontSize = 64; // Ensure the final scale is exactly the target scale
        comboText.color = combo_init_color;
    }

    System.Collections.IEnumerator ScoreTypeTextAnimation()
    {
        float timer = 0f;
        float time_to_finish = 0.3f;
        while (timer < time_to_finish)
        {
            timer += Time.deltaTime;
            // Lerp between the initial scale and the target scale
            scoreTypeText.fontSize = Mathf.Lerp(36, 48, timer / time_to_finish);
            yield return null; // Wait for the next frame
        }
        scoreTypeText.fontSize = 36; // Ensure the final scale is exactly the target scale
        scoreTypeText.text = "";
    }

    // Update is called once per frame
    void UpdateRhythm()
    {

        // Handle Notes
        HandleNotes();
        // Handle Player Input
        if (!paused) {
            HandleInput();
        }
        
        if (finishedSong)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("TitleScreen");
            }
            return;
        }
        

        
        if (int.Parse(comboText.text) < current_combo)
        {
            StartCoroutine(ComboAnimation());
        }

        scoreText.text = "SCORE: " + score;
        comboText.text = current_combo + "";

        if (current_combo > 49)
        {
            comboVfx.SetActive(true);
        } else
        {
            comboVfx.SetActive(false);
        }

        if (swapToBulletFlag)
        {
            swapToBulletHell();
            swapToBulletFlag = false;
        }

    }

    // ============================================== //
    // =========== Bullet Hell Section ============== //
    // ============================================== //

    void swapToBulletHell()
    {
        // Swap: 
        gamestate = GameState.RHYTHM;
        swapGameState();

        // player
        player.GetComponent<Rigidbody>().WakeUp();

        // Todo... bullet hell setup..
        gameCamera.GetComponent<GameCamera>().TransitionToBulletHell();

        boundingBoxes.SetActive(true);

        masterProjectilePool = new ObjectPool<GameObject>(
            CreatePooledBullet,
            OnGetBullet,
            OnReleaseBullet,
            OnDestroyBullet,
            collectionCheck: false,
            defaultCapacity: defaultPoolSize,
            maxSize: maxPoolSize
        );
        bulletHellStartTime = Time.time; //TODO: i am just setting this to the time the bullet hell manager is activated

    }
    void ClearBulletHell()
    {
        foreach(BulletEmitter ae in GetComponentsInChildren<BulletEmitter>())
        {
            ae.gameObject.SetActive(false);
        }
    }
    
    void UpdateBullet()
    {
        //emitters will be set active on a certain beat 
        //ex: 240 bpm, 3 min song = 720 beats and each emitter is released on one of those beats
        //will continuously set projectile emitters to active
        if (emitterIndex >= emitters.Count)
        {
            OnBulletHellComplete();

        } else {

            for(; emitterIndex < emitters.Count(); emitterIndex++)
            {
                Pair p = emitters[emitterIndex];
                //we do not set past emitters to false anymore they just sit there
                //TODO for cleanliness we can possibly set all emitters to false when we reach a stopflag

                string timestamp = p.activateTimestamp;
                int minuteIndex = timestamp.IndexOf(":");
                if(minuteIndex == -1)
                {
                    Debug.Log("timestamp string not set!");
                    emitterIndex++; //move to next
                    break;
                }
                float activateTime = int.Parse(timestamp.Substring(0, minuteIndex)) * 60 + global_offset;
                activateTime += float.Parse(timestamp.Substring(minuteIndex + 1));

                if(time_current >= activateTime) //it's time to activate this emitter
                {
                    Debug.Log(time_current + " " + activateTime);
                    p.emitter.SetActive(true);
                    Debug.Log("set active: " + emitterIndex);
                    timeLastEmitterSetActive = time_current;
                    if(p.emitter.GetComponent<BulletEmitter>().GetEmitType() == BulletEmitter.EmitType.stopFlag)
                    {
                        OnBulletHellComplete();
                        emitterIndex++; //move past the stop flag
                        Debug.Log("reached stop flag");
                        break;
                    }
                }
                else
                {
                    break; //end loop, emitterindex is now at the index of the next emitter to set active
                }
            }
            if (emitterIndex == emitters.Count)
            {
                OnBulletHellComplete();
                Debug.Log("reahced the end w/o reaching a stop flag emitter which should NOT happen");
            }
        }

        if (swapToRhythmFlag)
        {
            swapToRhythmGame();
            swapToRhythmFlag = false;
        }
    }

    void OnBulletHellComplete()
    {
        if(emitterIndex >= emitters.Count) emitterIndex = 0;
        swapToRhythmFlag = true;
        //gameObject.SetActive(false);
        //TODO something (placeholder^^)
    }

    public GameObject CreatePooledBullet()
    {
        GameObject proj = Instantiate(bulletPrefab);
        proj.SetActive(false);
        return proj;
    }

    public void OnGetBullet(GameObject bullet) {
        bullet.SetActive(true);
    }
    public void OnReleaseBullet(GameObject bullet) {
        bullet.SetActive(false);
    }
    public void OnDestroyBullet(GameObject bullet) {
        Destroy(bullet);
    }

    // ============================================== //
    // =========== Beat Editor Section ============== //
    // ============================================== //

    void ToggleEditorMode()
    {
        editorMode = !editorMode;
        GameCamera cam = gameCamera.GetComponent<GameCamera>();
        
        editorContainer.gameObject.SetActive(!editorContainer.gameObject.activeSelf);

        if (cam.IsEditorState)
        {
            note_speed = og_note_speed;
            if (gamestate == GameState.RHYTHM)
                cam.TransitionToRhythmGame();
            else
                cam.TransitionToBulletHell();
        }
        else
        {
            og_note_speed = note_speed;
            cam.TransitionToEditor();
        }

        ReloadAllNotesOnScreen();
    }

    void UpdateEditor()
    {
        if (true)
        {
            // Number keys 1–5 add new TAP notes at current snapped time
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Alpha5))
            {

                float snap = 60f / bpm * editorSnapScale;
                float newTime = Mathf.Round(time_current / snap) * snap;

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    noteInfos.Add(new Beatmap.NoteInfo(Beatmap.NoteInfo.BASIC_NOTE, newTime, new string[] { "1" }));
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    noteInfos.Add(new Beatmap.NoteInfo(Beatmap.NoteInfo.BASIC_NOTE, newTime, new string[] { "2" }));
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    noteInfos.Add(new Beatmap.NoteInfo(Beatmap.NoteInfo.BASIC_NOTE, newTime, new string[] { "0" }));
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    noteInfos.Add(new Beatmap.NoteInfo(Beatmap.NoteInfo.BASIC_NOTE, newTime, new string[] { "3" }));
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    noteInfos.Add(new Beatmap.NoteInfo(Beatmap.NoteInfo.BASIC_NOTE, newTime, new string[] { "4" }));
                }

                updateBeatmapOrder();
                ReloadAllNotesOnScreen();

            } 
            else if (Input.GetKeyDown(KeyCode.W))
            {
                // Shift Up / Dupe Shift Up
                if (editorSelectedNotes.Count > 0) {
                    bool shift = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    var notesToProcess = editorSelectedNotes.OrderByDescending(n => n.note3D.id).ToList();
                    foreach (NoteEditor noteEditor in notesToProcess)
                    {
                        Note3D note = noteEditor.note3D;
                        int id = note.id;

                        if (id < 0 || id >= noteInfos.Count) continue;
                        Beatmap.NoteInfo beatmapNote = noteInfos[id];

                        float snap = 60.0f / bpm * editorSnapScale;
                        float newtime = Mathf.Round( (beatmapNote.start_time + snap) / snap) * snap;

                        if (shift)
                        {
                            Beatmap.NoteInfo newNote = new Beatmap.NoteInfo(beatmapNote.note_type, beatmapNote.start_time, (string[])beatmapNote.extra_info.Clone());
                            noteInfos.Insert(id + 1, newNote);
                            // foreach (NoteEditor ne in editorSelectedNotes)
                            // {
                            //     if (ne.note3D.id > id)
                            //         ne.note3D.id += 1;
                            // }
                            // set new time
                            beatmapNote.start_time = newtime;
                            // find new pos and move
                            // int oldId = id;

                            // noteInfos.RemoveAt(oldId); // Temporarily remove it
                            // int newId = noteInfos.FindIndex(n => n.start_time > beatmapNote.start_time);
                            // if (newId == -1) newId = noteInfos.Count; // Goes at end if no later note
                            // noteInfos.Insert(newId, beatmapNote);
                            //update id of all
                            // foreach (NoteEditor ne in editorSelectedNotes)
                            // {
                            //     if (ne.note3D.id == oldId)
                            //         ne.note3D.id = newId;
                            //     else if (oldId < newId && ne.note3D.id > oldId && ne.note3D.id <= newId)
                            //         ne.note3D.id -= 1; 
                            //     else if (oldId > newId && ne.note3D.id >= newId && ne.note3D.id < oldId)
                            //         ne.note3D.id += 1;
                            // }
                        }
                        else
                        {
                            beatmapNote.start_time = newtime;
                        }
                    }
                    updateBeatmapOrder();
                    ReloadAllNotesOnScreen();
                }

            } 
            else if (Input.GetKeyDown(KeyCode.S))
            {
                // Shift Down / Dupe Shift Down
                if (editorSelectedNotes.Count > 0) {
                    bool shift = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    var notesToProcess = editorSelectedNotes.OrderByDescending(n => n.note3D.id).ToList();
                    foreach (NoteEditor noteEditor in notesToProcess)
                    {
                        Note3D note = noteEditor.note3D;
                        int id = note.id;

                        if (id < 0 || id >= noteInfos.Count) continue;
                        Beatmap.NoteInfo beatmapNote = noteInfos[id];

                        float snap = 60.0f / bpm * editorSnapScale;
                        float newtime = Mathf.Round( (beatmapNote.start_time - snap) / snap) * snap;

                        if (shift)
                        {
                            Beatmap.NoteInfo newNote = new Beatmap.NoteInfo(beatmapNote.note_type, beatmapNote.start_time, (string[])beatmapNote.extra_info.Clone());
                            noteInfos.Insert(id + 1, newNote);
                            // foreach (NoteEditor ne in editorSelectedNotes)
                            // {
                            //     if (ne.note3D.id > id)
                            //         ne.note3D.id += 1;
                            // }
                            // set new time
                            beatmapNote.start_time = newtime;
                            // find new pos and move
                            // int oldId = id;
                            // noteInfos.RemoveAt(oldId); // Temporarily remove it
                            // int newId = noteInfos.FindIndex(n => n.start_time > beatmapNote.start_time);
                            // if (newId == -1) newId = noteInfos.Count; // Goes at end if no later note
                            // noteInfos.Insert(newId, beatmapNote);
                            // //update id of all
                            // foreach (NoteEditor ne in editorSelectedNotes)
                            // {
                            //     if (ne.note3D.id == oldId)
                            //         ne.note3D.id = newId;
                            //     else if (oldId < newId && ne.note3D.id > oldId && ne.note3D.id <= newId)
                            //         ne.note3D.id -= 1; 
                            //     else if (oldId > newId && ne.note3D.id >= newId && ne.note3D.id < oldId)
                            //         ne.note3D.id += 1;
                            // }
                        }
                        else
                        {
                            beatmapNote.start_time = newtime;
                        }
                    }
                    updateBeatmapOrder();
                    ReloadAllNotesOnScreen();
                }

            } 
            else if (Input.GetKeyDown(KeyCode.A))
            {
                // Shift Left / Dupe Shift Left
                if (editorSelectedNotes.Count > 0) {
                    bool shift = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    var notesToProcess = editorSelectedNotes.OrderByDescending(n => n.note3D.id).ToList();
                    foreach (NoteEditor noteEditor in notesToProcess)
                    {
                        Note3D note = noteEditor.note3D;
                        int id = note.id;
                        NoteType type = note.GetNoteType();
                        KeyCode key = note.key;
                        KeyCode newkey = KeyCode.Space;

                        if (id < 0 || id >= noteInfos.Count) continue;
                        Beatmap.NoteInfo beatmapNote = noteInfos[id];

                        if (type == NoteType.TAP || type == NoteType.HOLD)
                        {
                            if (key == keyList[0]) newkey = keyList[4];
                            else if (key == keyList[1]) newkey = keyList[0];
                            else if (key == keyList[2]) newkey = keyList[1];
                            else if (key == keyList[3]) newkey = keyList[2];
                            else if (key == keyList[4]) newkey = keyList[3];
                            else newkey = keyList[2];
                        }

                        if (shift)
                        {
                            Beatmap.NoteInfo newNote = new Beatmap.NoteInfo(beatmapNote.note_type, beatmapNote.start_time, (string[])beatmapNote.extra_info.Clone());
                            noteInfos.Insert(id + 1, newNote);
                            foreach (NoteEditor ne in editorSelectedNotes)
                            {
                                if (ne.note3D.id > id)
                                    ne.note3D.id += 1;
                            }
                            beatmapNote.extra_info[0] = KeyToLaneID(newkey).ToString();
                        }
                        else
                        {   
                            beatmapNote.extra_info[0] = KeyToLaneID(newkey).ToString();
                        }
                    }
                    updateBeatmapOrder();
                    ReloadAllNotesOnScreen();
                }

            } 
            else if (Input.GetKeyDown(KeyCode.D))
            {
                // Shift Right / Dupe Shift Right
                if (editorSelectedNotes.Count > 0) {
                    bool shift = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    var notesToProcess = editorSelectedNotes.OrderByDescending(n => n.note3D.id).ToList();
                    foreach (NoteEditor noteEditor in notesToProcess)
                    {
                        Note3D note = noteEditor.note3D;
                        int id = note.id;
                        NoteType type = note.GetNoteType();
                        KeyCode key = note.key;
                        KeyCode newkey = KeyCode.Space;

                        if (id < 0 || id >= noteInfos.Count) continue;
                        Beatmap.NoteInfo beatmapNote = noteInfos[id];

                        if (type == NoteType.TAP || type == NoteType.HOLD)
                        {
                            if (key == keyList[0]) newkey = keyList[1];
                            else if (key == keyList[1]) newkey = keyList[2];
                            else if (key == keyList[2]) newkey = keyList[3];
                            else if (key == keyList[3]) newkey = keyList[4];
                            else if (key == keyList[4]) newkey = keyList[0];
                            else newkey = keyList[2];
                        }

                        if (shift)
                        {
                            Beatmap.NoteInfo newNote = new Beatmap.NoteInfo(beatmapNote.note_type, beatmapNote.start_time, (string[])beatmapNote.extra_info.Clone());
                            noteInfos.Insert(id + 1, newNote);
                            // foreach (NoteEditor ne in editorSelectedNotes)
                            // {
                            //     if (ne.note3D.id > id)
                            //         ne.note3D.id += 1;
                            // }
                            beatmapNote.extra_info[0] = KeyToLaneID(newkey).ToString();
                        }
                        else
                        {
                            beatmapNote.extra_info[0] = KeyToLaneID(newkey).ToString();
                        }
                    }
                    updateBeatmapOrder();
                    ReloadAllNotesOnScreen();
                }

            } 
            else if (Input.GetKeyDown(KeyCode.N))
            {
                
                if (editorSelectedNotes.Count > 0) {
                    var notesToProcess = editorSelectedNotes.OrderByDescending(n => n.note3D.id).ToList();
                    foreach (NoteEditor noteEditor in notesToProcess)
                    {
                        Note3D note = noteEditor.note3D;
                        int id = note.id;
                        NoteType type = note.GetNoteType();
                        KeyCode key = note.key;

                        if (id < 0 || id >= noteInfos.Count) continue;
                        Beatmap.NoteInfo beatmapNote = noteInfos[id];

                        if (type == NoteType.TAP)
                        {
                            beatmapNote.note_type = Beatmap.NoteInfo.HOLD_NOTE;
                            float len = 60.0f / bpm * editorSnapScale;
                            beatmapNote.extra_info = new string[] { KeyToLaneID(key).ToString(), len.ToString() };
                        } else
                        {
                            beatmapNote.note_type = Beatmap.NoteInfo.BASIC_NOTE;
                            beatmapNote.extra_info = new string[] { KeyToLaneID(key).ToString() };
                        }

                    }
                    ReloadAllNotesOnScreen();
                }

            } 
            else if (Input.GetKeyDown(KeyCode.J))
            {
                
                if (editorSelectedNotes.Count > 0) {
                    var notesToProcess = editorSelectedNotes.OrderByDescending(n => n.note3D.id).ToList();
                    foreach (NoteEditor noteEditor in notesToProcess)
                    {
                        Note3D note = noteEditor.note3D;
                        int id = note.id;
                        NoteType type = note.GetNoteType();

                        if (type != NoteType.HOLD) continue;
                        if (id < 0 || id >= noteInfos.Count) continue;

                        HoldNote3D holdnote = (HoldNote3D)note;

                        Beatmap.NoteInfo beatmapNote = noteInfos[id];

                        float note_len = Mathf.Max(holdnote.time_end - 60.0f / bpm * editorSnapScale - holdnote.time, 0);

                        beatmapNote.extra_info[1] = note_len.ToString();
                        
                    }
                    ReloadAllNotesOnScreen();
                }

            } 
            else if (Input.GetKeyDown(KeyCode.K))
            {
                
                if (editorSelectedNotes.Count > 0) {
                    var notesToProcess = editorSelectedNotes.OrderByDescending(n => n.note3D.id).ToList();
                    foreach (NoteEditor noteEditor in notesToProcess)
                    {
                        Note3D note = noteEditor.note3D;
                        int id = note.id;
                        NoteType type = note.GetNoteType();

                        if (type != NoteType.HOLD) continue;
                        if (id < 0 || id >= noteInfos.Count) continue;

                        HoldNote3D holdnote = (HoldNote3D)note;

                        Beatmap.NoteInfo beatmapNote = noteInfos[id];

                        float note_len = Mathf.Max(holdnote.time_end + 60.0f / bpm * editorSnapScale - holdnote.time, 0);

                        beatmapNote.extra_info[1] = note_len.ToString();
                        
                    }
                    ReloadAllNotesOnScreen();
                }

            }
            else if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (editorSelectedNotes.Count > 0) {
                    var notesToProcess = editorSelectedNotes.OrderByDescending(n => n.note3D.id).ToList();
                    foreach (NoteEditor noteEditor in notesToProcess)
                    {
                        Note3D note = noteEditor.note3D;
                        int id = note.id;
                        NoteType type = note.GetNoteType();

                        if (id < 0 || id >= noteInfos.Count) continue;

                        Beatmap.NoteInfo beatmapNote = noteInfos[id];

                        noteInfos.RemoveAt(id);
                        editorSelectedNotes.Remove(noteEditor);
                        foreach (NoteEditor ne in editorSelectedNotes)
                        {
                            if (ne.note3D.id > id)
                                ne.note3D.id -= 1;
                        }
                    }
                    ReloadAllNotesOnScreen();
                }

            }
        }


        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftShift) ||
    Input.GetKey(KeyCode.RightShift);

            if (ctrl)
            {
                float speed_mod = Mathf.Sign(scroll) * 0.5f;
                note_speed = Mathf.Clamp(note_speed + speed_mod, 1f, 20f);
                updateNotePrespawnTime();
            }
            else
            {
                // Scroll Timeline
                float timeMod = Mathf.Sign(scroll) * 60.0f / bpm * editorSnapScale;
                float newTime = Mathf.Clamp(audioSource.time + timeMod, 0f, audioSource.clip.length);
                SetAudioTime(newTime);
                ReloadAllNotesOnScreen();
            }
        }

        if (Input.GetMouseButtonDown(0)) // Left Mouse Button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftShift) ||
    Input.GetKey(KeyCode.RightShift);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                NoteEditor note = hit.collider.GetComponentInParent<NoteEditor>();

                if (note != null)
                {
                    if (editorSelectedNotes.Contains(note))
                    {
                        if (ctrl)
                        {
                            // LMB + Ctrl on selected -> deselect this note
                            editorSelectedNotes.Remove(note);
                            note.OnDeselect();
                        }
                        else
                        {
                            // LMB on selected -> deselect all
                            EditorDeselectAll();
                        }
                    }
                    else
                    {
                        if (ctrl)
                        {
                            // LMB + Ctrl on unselected -> add to selection
                            editorSelectedNotes.Add(note);
                            note.OnSelect();
                        }
                        else
                        {
                            // LMB on unselected -> select only this note
                            EditorDeselectAll();
                            editorSelectedNotes.Add(note);
                            note.OnSelect();
                        }
                    }
                }
                else if (!ctrl)
                {
                    // Clicked empty space without Ctrl -> deselect all
                    EditorDeselectAll();
                }
            }
            else if (!ctrl)
            {
                // Clicked empty space without Ctrl -> deselect all
                EditorDeselectAll();
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePlayPause();
        }

    }

    
    void EditorDeselectAll()
    {
        
        foreach (var note in editorSelectedNotes) {
            note.OnDeselect();
        }
        editorSelectedNotes.Clear();
    }

    void Update()
    {
        
        // Pre update
        if (!paused) {
            time_current = audioSource.time + time_offset;
        }
        timeline.SetValueWithoutNotify(audioSource.time);
        // editor functions
        if (allowEditor) {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                ToggleTimeLineVisibility();
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleEditorMode();
            }
        }

        if (editorMode)
        {
            UpdateEditor();
        }

        // Main update
        if (gamestate == GameState.RHYTHM)
        {
            UpdateRhythm();
        }
        else
        {
            UpdateBullet();
        }
    }

    void updateNotePrespawnTime()
    {
        note_prespawn_time = (note_z_spawn - note_z_despawn) / note_speed;
        note_postspawn_time = -note_z_despawn / note_speed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //initialize bullet hell variables
        timeLastEmitterSetActive = Time.time;
        emitterIndex = 0;

        instance = this;

        // Check if the GlobalSettings object exists
        if (GlobalSettings.Instance != null)
        {
            Debug.Log("Loading Global Settings...");
            note_speed = GlobalSettings.Instance.scrollSpeed;
            time_offset = GlobalSettings.Instance.audioOffset;
        }
        else
        {
            // Fallback defaults if starting scene directly without Menu
            Debug.LogWarning("GlobalSettings not found, using local defaults.");
            // Only set these if you didn't set them in the Inspector
            if (time_offset == 0) time_offset = 0.42f;
        }

        // CRITICAL: Update og_note_speed so the Editor mode toggle logic works correctly
        og_note_speed = note_speed;

        // Rhythm game setup
        time_offset = 0.42f;
        instance = this;
        updateNotePrespawnTime();
        noteInfos = Beatmap.LoadBeatmap("Beatmap_Main");
        currNote = 0;

        // GUI setup
        if (editorContainer != null)
            editorContainer.gameObject.SetActive(editorMode);
        if (timelineContainer != null)
            timelineContainer.gameObject.SetActive(showTimeline);

        perfectText.text = "";
        greatText.text = "";
        badText.text = "";
        missText.text = "";
        finalScoreText.text = "";
        maxComboText.text = "";

        string combo_inc_hex_color = "#B2ACFF";
        ColorUtility.TryParseHtmlString(combo_inc_hex_color, out combo_inc_color);

        string combo_init_hex_color = "rgba(255, 255, 255, 1)";
        ColorUtility.TryParseHtmlString(combo_init_hex_color, out combo_init_color);

        editorSnapScale = 0.5f;
        OnEditorSnapButtonClick();
        timeline.minValue = 0f;
        timeline.maxValue = audioSource.clip.length;
        SetAudioTime(0f);
        setPause(false);

        swapToRhythmGame();

    }

    public void TogglePlayPause()
    {
        if (!paused)
        {
            setPause(true);
        }
        else
        {
            setPause(false);
        }
    }

    public void StopAudio()
    {
        SetAudioTime(0f);
        setPause(true);
    }
    public void PlayAudio()
    {
        setPause(false);
    }

    public void SetAudioTime(float time)
    {
        time = Mathf.Clamp(time, 0f, audioSource.clip.length);
        audioSource.time = time;
        time_current = time + time_offset;
        timeline.SetValueWithoutNotify(time);
    }
    public void SetAudioTimeFromSlider()
    {
        SetAudioTime(timeline.value);

        ReloadAllNotesOnScreen();
    }
    public void updateBeatmapOrder()
    {
        var indexed = noteInfos.Select((note, i) => new { note, oldIndex = i }).ToList();

        indexed.Sort((a, b) => a.note.start_time.CompareTo(b.note.start_time));

        noteInfos = indexed.Select(x => x.note).ToList();

        int[] oldToNew = new int[indexed.Count];
        for (int newIndex = 0; newIndex < indexed.Count; newIndex++)
        {
            oldToNew[indexed[newIndex].oldIndex] = newIndex;
        }
        
        List<int> selectedIds = new List<int>();
        if (editorMode) {
            foreach (NoteEditor note in editorSelectedNotes)
            {
                selectedIds.Add(note.note3D.id); 
            }
            EditorDeselectAll();
        }

        // Restore editor selection
        if (editorMode) {
            foreach (int id in selectedIds)
            {
                int new_id = oldToNew[id];
                NoteEditor note = ActiveNotes.Find(n => n.GetComponent<NoteEditor>()?.note3D?.id == new_id)?.GetComponent<NoteEditor>();
                if (note != null)
                {
                    editorSelectedNotes.Add(note);    
                    note.OnSelect();
                }
            }
        }

    }
    public void ReloadAllNotesOnScreen()
    {
        
        // Save selected note indices
        List<int> selectedIds = new List<int>();
        if (editorMode) {
            foreach (NoteEditor note in editorSelectedNotes)
            {
                selectedIds.Add(note.note3D.id); 
            }
            EditorDeselectAll();
        }

        // clear prev notes
        for (int i = ActiveNotes.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(ActiveNotes[i]);
        }
        ActiveNotes.Clear();
        // beatmap note
        currNote = 0;
        while (currNote < noteInfos.Count && time_current - note_postspawn_time >= noteInfos[currNote].start_time)
        {
            currNote++;
        }
        // on-beat notes
        float b = 60f / bpm;
        time_nextnote = Mathf.Ceil((time_current - note_postspawn_time) / b) * b;
        time_nextnote_deco = time_nextnote;
        // spawn notes
        HandleNoteSpawning();

        // Restore editor selection
        if (editorMode) {
            foreach (int id in selectedIds)
            {
                NoteEditor note = ActiveNotes.Find(n => n.GetComponent<NoteEditor>()?.note3D?.id == id)?.GetComponent<NoteEditor>();
                if (note != null)
                {
                    editorSelectedNotes.Add(note);    
                    note.OnSelect();
                }
            }
        }
    }

    public void setPause(bool p)
    {
        paused = p;

        SetAudioTime(audioSource.time);
        if (paused)
            audioSource.Pause();
        else
            audioSource.Play();
        
        // update label
        playPauseLabel.text = paused ? ">" : "||";

        if (playPauseButton != null)
        {
            var colors = playPauseButton.colors;
            colors.normalColor = paused ? Color.gray : Color.white;
            playPauseButton.colors = colors;

            var graphic = playPauseButton.targetGraphic;
            if (graphic != null)
                graphic.color = colors.normalColor;
        }

        editorPauseLabel.gameObject.SetActive(!paused);
        
    }

    public void SaveBeatMap()
    {

        setPause(true);

        Debug.Log("Saving Beatmap...");

        void WriteFile(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("Global Offset:");
                writer.WriteLine(global_offset.ToString("F2"));
                writer.WriteLine("Note Data:");

                foreach (var note in noteInfos)
                {
                    string line = note.note_type.ToString();

                    int minutes = Mathf.FloorToInt(note.start_time / 60f);
                    float seconds = note.start_time % 60f;
                    line += $" {minutes}:{seconds:F2}";

                    if (note.extra_info != null && note.extra_info.Length > 0)
                    {
                        string[] formattedInfo = new string[note.extra_info.Length];
                        note.extra_info.CopyTo(formattedInfo, 0);

                        if (note.note_type == Beatmap.NoteInfo.HOLD_NOTE && formattedInfo.Length >= 2)
                        {
                            if (float.TryParse(formattedInfo[1], out float val))
                            {
                                formattedInfo[1] = $"{Mathf.FloorToInt(val / 60f)}:{(val % 60f):00.00}";
                            }
                        }
                        else if (note.note_type == Beatmap.NoteInfo.SLIDE_NOTE && formattedInfo.Length >= 1)
                        {
                            if (float.TryParse(formattedInfo[0], out float val))
                            {
                                formattedInfo[0] = $"{Mathf.FloorToInt(val / 60f)}:{(val % 60f):00.00}";
                            }
                        }
                        line += " " + string.Join(" ", formattedInfo);
                    }
                    writer.WriteLine(line);
                }
            }
            Debug.Log($"Beatmap exported to: {path}");
        }

        string path1 = "Assets/Resources/Beatmap_Main.txt";
        string path2 = Path.Combine("Assets", $"Resources/Beatmap_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        WriteFile(path1);
        WriteFile(path2);

    }
    
    public void ToggleTimeLineVisibility()
    {
        showTimeline = !showTimeline;
        timelineContainer.gameObject.SetActive(showTimeline);
    }

    public void OnEditorSnapButtonClick()
    {
        float[] editorSnapOptions = { 1f, 0.5f, 0.25f, 0.125f };
        int currentIndex = Array.IndexOf(editorSnapOptions, editorSnapScale);
        if (currentIndex < 0) currentIndex = 0; // fallback
        int nextIndex = (currentIndex + 1) % editorSnapOptions.Length;

        // update scale
        editorSnapScale = editorSnapOptions[nextIndex];

        // Update button label
        float snap = editorSnapScale;
        if (snap >= 1f)
            editorSnapButtonLabel.text = snap.ToString("0");
        else
            editorSnapButtonLabel.text = "1/" + (1f / snap).ToString("0");
    }

}