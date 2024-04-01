using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Panfili.Observer;

namespace Panfili.Playback{
    public class Player{
        public enum State{
            Play,
            Pause,
            PlayFrames,
            PlayFramesSequential
        }

        public Float time, speed, timeIncrement;
        public State state = State.Play;

        public bool interpolate = false;
        public float lastTimestamp = 1f;
        
        public List<KeyCode> key = new List<KeyCode>{KeyCode.Space, KeyCode.E, KeyCode.Q, KeyCode.W, KeyCode.O};

        public Dictionary<string, System.Action> Function;

        public Player(float _time = 0, float _speed = 1f, float _timeIncrement = .01f)
        {
            Application.targetFrameRate = -1;

            time            = new Float(_time);
            speed           = new Float(_speed);
            timeIncrement   = new Float(_timeIncrement);

            Function = new Dictionary<string, System.Action>{
                {"Next",    () => Next                  ()},
                {"Last",    () => Last                  ()},
                {"Play",    () => TogglePlay            ()},
                {"Pause",   () => TogglePlay            ()},
                {"Reverse", () => Reverse               ()},
                {"Fast",    () => DoubleTime            ()},
                {"Slow",    () => HalfTime              ()},
                {"Frames",  () => PlayFramesSequential  (true)}
            };
        }

        public void UpdateLastTimestamp(float _lastTimestamp) { lastTimestamp = Mathf.Max(_lastTimestamp, lastTimestamp); }
        
        // public int NextFrameIndex()         { frame = frame + (int) speed.Value; return Math.Clamp(frame, 0, times.Count); }
        // public void NextFrame()             { time.Value = times[NextFrameIndex()]; }

        public void Next()                  { time += timeIncrement * speed; }
        public void Next(float deltaTime)   { time += deltaTime * speed; }
        public void Last()                  { time -= timeIncrement * speed; }
        public void Last(float deltaTime)   { time -= deltaTime * speed; }
        public void AtTime(float _time)     { time.Value = _time; }
        public void TogglePlay()            { state = (state == State.Play) ? State.Pause : State.Play; }
        public void Reverse()               { speed *= -1f; }
        public void DoubleTime()            { speed *= 2; }
        public void HalfTime()              { speed /= 2; }

        public void PlayFramesSequential(bool firstFrame = false){
            var target = Main.shadow;
            if (firstFrame) target.frame.Value = 0;
            if (target.frame.Value == target.time.Count) { state = State.Pause; return; }

            state = State.PlayFramesSequential;
            target.frame.Value += 1;
            AtTime( target.time[target.frame.Value] );
        }

        public void Play(){
            if (time.Value >= lastTimestamp)    return;
            if (time.Value < 0)                 return;
            Next();
        }

        public void Update(){
            if (state == State.Play) Play();
            if (state == State.PlayFramesSequential) PlayFramesSequential();
        }

        public void GUIButtons(Vector2 size, List<string> name, float height = .33f) { foreach(string n in name) if(GUILayout.Button( n, GUILayout.Width(size.x * (1f / name.Count * .95f)), GUILayout.Height( size.y * height ))) Function[n](); }

        public void ControlGUI()
        {
            Vector2 size = new Vector2(300, 100);

            GUILayout.BeginArea(new Rect(10, 10, size.x, size.y));
            GUI.Box(new Rect(0, 0, size.x, size.y), $"Current Time: {time.Value}");

            // Slider for adjusting current time
            GUILayout.Space(size.y * .3f);
            GUILayout.BeginHorizontal();
            time.Value = GUILayout.HorizontalSlider(time.Value, 0.0f, lastTimestamp, GUILayout.Width(size.x*.95f), GUILayout.Height(size.y*.33f));
            GUILayout.EndHorizontal();

            // Buttons for controlling playback
            GUILayout.BeginHorizontal();
            if (state == State.Pause)       GUIButtons(size, new List<string>{"Last", "Play", "Next", "Frames"});
            else if (state == State.Play)   GUIButtons(size, new List<string>{"Slow", "Pause", "Fast", "Reverse"});

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
    
    // public class Recorder{
    //     string path;
    //     float start_time;
    //     var video_settings = new VideoTrackAttributes();

    //     public Recorder(string _path, float _start_time = 0, int _height = 1080, int _width = 1920, bool _includeAlpha = false){
    //         path        = _path;
    //         start_time  = _start_time;

    //         var video_settings = new VideoTrackAttributes {
    //             width           = _width,
    //             height          = _height,
    //             includeAlpha    = _includeAlpha
    //         };
    //     }


    // }
} 


