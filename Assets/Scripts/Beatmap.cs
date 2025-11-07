using UnityEngine;
using System.Collections.Generic;

public class Beatmap
{
    const int BPM = 174 * 2;

    // <note_id> <min:sec.ms> <note specific inputs>
    // 0 <min:sec.ms> <lane (1-4)>
    // 1 <min:sec.ms> <lane (1-4)> <length min:sec.ms>
    // 2 <min:sec.ms>
    // 3 <min:sec.ms> <endtime seconds> <start_key> <end_key>
    public static List<NoteInfo> LoadBeatmap(string filename)
    {
        List<NoteInfo> notes = new List<NoteInfo>();
        // Load from resources folder
        TextAsset ta = Resources.Load<TextAsset>(filename);
        string[] mapLines = ta.text.Split("\n");

        float prev_time_f = -1;
        int prev_lane_f = -1;
        int line_index = -1;
        float global_offset = 0f;
        foreach (string line in mapLines)
        {
            line_index++;

            if (line_index == 0 || line_index == 2)
            {
                continue;
            }
            if (line_index == 1)
            {
                global_offset = float.Parse(line);
                continue;
            }

            if (line.Length <= 1)
            {
                continue;
            }
            //Debug.Log(line);
            string[] tokens = line.Split(" ");
            int minuteIndex = tokens[1].IndexOf(":");
            float time = int.Parse(tokens[1].Substring(0, minuteIndex)) * 60 + global_offset;
            time += float.Parse(tokens[1].Substring(minuteIndex + 1));

            int prev_time = (int)(time / (60f / BPM));
            int next_time = prev_time + 1;

            // if (Mathf.Abs(time - (prev_time * (60f / BPM))) > Mathf.Abs(time - (next_time * (60f / BPM))))
            // {
            //     time = next_time * (60f / BPM);
            // }
            // else
            // {
            //     time = prev_time * (60f / BPM);
            // }

            //Debug.Log(time);



            switch (int.Parse(tokens[0]))
            {
                case NoteInfo.BASIC_NOTE:
                    int lane = int.Parse(tokens[2]);
                    if (!(prev_time_f == time && prev_lane_f == lane))
                    {
                        notes.Add(new NoteInfo(NoteInfo.BASIC_NOTE, time, new string[]{lane + ""}));
                    }
                    //Debug.Log(time);
                    //Debug.Log(new string[]{lane + ""});
                    prev_time_f = time;
                    prev_lane_f = lane;
                    break;
                case NoteInfo.HOLD_NOTE:
                    lane = int.Parse(tokens[2]);
                    string length = tokens[3];
                    if (!(prev_time_f == time && prev_lane_f == lane))
                    {
                        notes.Add(new NoteInfo(NoteInfo.HOLD_NOTE, time, new string[] { lane + "", length }));
                    }
                    prev_time_f = time;
                    prev_lane_f = lane;
                    break;
                case NoteInfo.SPACE_NOTE:
                    if (!(prev_time_f == time && prev_lane_f == 5))
                    {
                        notes.Add(new NoteInfo(NoteInfo.SPACE_NOTE, time));
                    }
                    prev_time_f = time;
                    prev_lane_f = 5;
                    break;
                case NoteInfo.SLIDE_NOTE:
                    float endtime = float.Parse(tokens[2]);
                    char start_key = tokens[3][0];
                    char end_key = tokens[4][0];
                    notes.Add(new NoteInfo(NoteInfo.SLIDE_NOTE, time, new string[] {endtime + "", start_key + "", end_key + ""}));
                    break;
                default:
                    Debug.Log("WARNING: Invalid Tile ID");
                    break;
            }
        }

        return notes;
    }

    public class NoteInfo
    {
        public const int BASIC_NOTE = 0;
        public const int HOLD_NOTE = 1;
        public const int SPACE_NOTE = 2;
        public const  int SLIDE_NOTE = 3;
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
