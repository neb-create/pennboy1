using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;
using TMPro;

public class RhythmRecorder : MonoBehaviour
{
    string data;
    float curr_time;
    bool started = false;
    bool finished = false;
    float SONG_LENGTH;
    float[] startTimes;

    public TextMeshProUGUI timerText;

    const int W = 0;
    const int E = 1;
    const int I = 2;
    const int O = 3;
    const int SPACE = 4;
    const float MAX_BASIC_DELTA = 0.4f;
    const float RECORD_PERCENT = 1.0f;

    public float global_offset = -1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTimes = new float[4];
        data = "Global Offset:\n" + global_offset + "\nNote Data:\n";
        Write();
        GetComponent<AudioSource>().Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (finished) return;
        if (!started)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                started = true;
                curr_time = 0f;
                Debug.Log("Song Started");
                GetComponent<AudioSource>().Play();
                SONG_LENGTH = GetComponent<AudioSource>().clip.length * RECORD_PERCENT;
            }
            return;
        };

        curr_time += Time.deltaTime;
        timerText.text = "Time: " + curr_time.ToString("F2");


        if (Input.GetKeyDown(KeyCode.Backspace) || curr_time > SONG_LENGTH)
        {
            Write();
            finished = true;
            Debug.Log("Song Done");
            if (GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Stop();
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            startTimes[W] = curr_time;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            startTimes[E] = curr_time;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            startTimes[I] = curr_time;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            startTimes[O] = curr_time;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            data += "2 " + FormatTime(curr_time) + "\n";
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            ProcessKey(W);
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            ProcessKey(E);
        }
        if (Input.GetKeyUp(KeyCode.I))
        {
            ProcessKey(I);
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            ProcessKey(O);
        }
    }

    string FormatTime(float time)
    {
        int min = (int)(time / 60);
        int sec = (int) (time-(min*60));
        int ms = (int) ((time - (int)time) * 100);
        return min + ":" + sec + "." + ms;
    }

    void ProcessKey(int keycode)
    {
        float keyDelta = curr_time - startTimes[keycode];
        if (keyDelta < MAX_BASIC_DELTA)
        {
            data += "0 " + FormatTime(startTimes[keycode]) + " " + (keycode + 1) + "\n";
        } else
        {
            data += "1 " + FormatTime(startTimes[keycode]) + " " + (keycode + 1) + " " + FormatTime(curr_time) + "\n";
        }
    }

    void Write()
    {
        using (StreamWriter sw = File.CreateText("Assets/Beatmap.txt"))
        {
            sw.WriteLine(data);
        }
    }
}

