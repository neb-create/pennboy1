using UnityEngine;
using System.Collections.Generic;

public class Beatmap
{
    // <note_id> <min:sec.ms> <note specific inputs>
    // 0 <min:sec.ms> <lane (1-4)>
    // 1 <min:sec.ms> <lane (1-4)> <length seconds>
    // 2 <min:sec.ms>
    // 3 <min:sec.ms> <endtime seconds> <start_key> <end_key>
    public static List<NoteInfo> LoadBeatmap(string filename)
    {
        List<NoteInfo> notes = new List<NoteInfo>();
        // Load from resources folder
        TextAsset ta = Resources.Load<TextAsset>(filename);
        string[] mapLines = ta.text.Split("\n");

        foreach (string line in mapLines)
        {
            string[] tokens = line.Split("\t");
            int minuteIndex = tokens[1].IndexOf(":");
            float time = int.Parse(tokens[1].Substring(0, minuteIndex)) * 60;
            time += float.Parse(tokens[1].Substring(minuteIndex + 1));

            switch (tokens[0])
            {
                case BASIC_NOTE:
                    int lane = int.Parse(tokens[2]);
                    notes.Add(new NoteInfo(BASIC_NOTE, time, new string[]{lane}));
                    break;
                case HOLD_NOTE:
                    int lane = int.Parse(tokens[2]);
                    int length = float.Parse(tokens[3]);
                    notes.Add(new NoteInfo(HOLD_NOTE, time, new string[] { lane, length }));
                    break;
                case SPACE_NOTE:
                    notes.Add(new NoteInfo(SPACE_NOTE, time));
                    break;
                case SLIDE_NOTE:
                    float endtime = float.Parse(tokens[2]);
                    char start_key = tokens[3][0];
                    char end_key = tokens[4][0];
                    notes.Add(new NoteInfo(SLIDE_NOTE, time, new string[] {endtime, start_key, end_key}));
                default:
                    Debug.Log("WARNING: Invalid Tile ID");
            }
        }

        return notes;
    }

    public static class NoteInfo
    {
        public static int BASIC_NOTE = 0;
        public static int HOLD_NOTE = 1;
        public static int SPACE_NOTE = 2;
        public static int SLIDE_NOTE = 3;
        public int note_type;
        public float start_time;
        public string[] extra_info;

        public NoteInfo(int note_type, float start_time, string[] extra_info = null) {
            this.note_type = note_type;
            this.start_time = start_time;
            this.extra_info = extra_info;
        }

    }
}
