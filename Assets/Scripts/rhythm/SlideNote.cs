using UnityEngine;
using System.Collections.Generic;
using System;


public class SlideNote : MonoBehaviour
{

    // Note Settings
    // time: the time frame where you should hit the note
    public float startTime;
    public float endTime;
    public int maxDist = 2;
    public KeyCode startKey;
    public KeyCode endKey;
    public int numKeys;
    public List<(KeyCode, float)> progress;
    const float PERFECT_THRESHOLD = 0.8F;
    const float GREAT_THRESHOLD = 0.5F;

    public float accuracy; // number of keys hit out of total, NOT timing related

    public static Dictionary<KeyCode, (int, int)> keys = new Dictionary<KeyCode, (int, int)>()
    {
        [KeyCode.BackQuote] = (0, 1),
        [KeyCode.Alpha1] = (0, 2),
        [KeyCode.Alpha2] = (0, 3),
        [KeyCode.Alpha3] = (0, 4),
        [KeyCode.Alpha4] = (0, 5),
        [KeyCode.Alpha5] = (0, 6),
        [KeyCode.Alpha6] = (0, 7),
        [KeyCode.Alpha7] = (0, 8),
        [KeyCode.Alpha8] = (0, 9),
        [KeyCode.Alpha9] = (0, 10),
        [KeyCode.Alpha0] = (0, 11),
        [KeyCode.Minus] = (0, 12),
        [KeyCode.Equals] = (0, 13),
        [KeyCode.Q] = (1, 2),
        [KeyCode.W] = (1, 3),
        [KeyCode.E] = (1, 4),
        [KeyCode.R] = (1, 5),
        [KeyCode.T] = (1, 6),
        [KeyCode.Y] = (1, 7),
        [KeyCode.U] = (1, 8),
        [KeyCode.I] = (1, 9),
        [KeyCode.O] = (1, 10),
        [KeyCode.P] = (1, 11),
        [KeyCode.LeftBracket] = (1, 12),
        [KeyCode.RightBracket] = (1, 13),
        [KeyCode.Backslash] = (1, 14),
        [KeyCode.A] = (2, 2),
        [KeyCode.S] = (2, 3),
        [KeyCode.D] = (2, 4),
        [KeyCode.F] = (2, 5),
        [KeyCode.G] = (2, 6),
        [KeyCode.H] = (2, 7),
        [KeyCode.J] = (2, 8),
        [KeyCode.K] = (2, 9),
        [KeyCode.L] = (2, 10),
        [KeyCode.Semicolon] = (2, 11),
        [KeyCode.Quote] = (2, 12),
        [KeyCode.Z] = (3, 2),
        [KeyCode.X] = (3, 3),
        [KeyCode.C] = (3, 4),
        [KeyCode.V] = (3, 5),
        [KeyCode.B] = (3, 6),
        [KeyCode.N] = (3, 7),
        [KeyCode.M] = (3, 8),
        [KeyCode.Comma] = (3, 9),
        [KeyCode.Period] = (3, 10),
        [KeyCode.Slash] = (3, 11)
    };


    // returns a bool saying whether or not the input was added successfully, takes in the key and time of the input
    bool addInput(KeyCode hitKey, float time)
    {
        (int, int) newKeyPos = keys[hitKey];
        (int newVertical, int newHorizontal) = newKeyPos;

        if (progress.Count == 0 && startKey == hitKey)
        {
            progress.Add((hitKey, time));
            return true;
        }

        (KeyCode, float) prevInput = progress[progress.Count - 1];
        (int prevVertical, int prevHorizontal) = keys[prevInput.Item1]; // finds the location of the previous key pressed
        if (prevVertical != newVertical || maxDist > newHorizontal - prevHorizontal) return false;

        progress.Add((hitKey, time));

        evaluateProgress();
        return true;
    }

    // returns a bool saying whether or not the note is complete. Called after each key is added or at end time.
    bool evaluateProgress()
    {
        float percentageDone = (0.0F + progress.Count) / numKeys;

        if (percentageDone >= PERFECT_THRESHOLD) return true; // TODO: DISPLAY PERFECT AND HANDLE SCORING
        if (percentageDone >= GREAT_THRESHOLD) return true; // TODO: DISPLAY GREAT AND HANDLE SCORING

        return false; // TODO: DISPLAY Miss AND HANDLE SCORING
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        List<KeyCode> allDown = new List<KeyCode>();
        foreach (KeyValuePair<KeyCode, (int, int)> entry in keys)
        {
            KeyCode code = entry.Key;
            if (Input.GetKey(code))
                allDown.Add(code);
        }

        foreach (KeyCode c in allDown)
        {
            addInput(c, RhythmGameManager.instance.time_current);
        }


    }
}