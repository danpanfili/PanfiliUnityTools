using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panfili.Observer;

namespace Panfili.Object{
    public class Source{
        Loader loader = new Loader();

        public string name          = "UNLABELED_DATA";
        public string path          = "";
        public Int frame            = new Int(0);
        public List<float> time     = new List<float>();
        public List<string> cams    = new List<string>{ "Camera" };
        public List<string> noDraw  = new List<string>{ "GazeTarget" };     // Names of markers you don't want to be visible

        public Bool isEnabled       = new Bool(true);
        public Bool isVisible       = new Bool(true);
        public bool isInterpolate   = true;

        public Dictionary<string, Marker> markers;
        public GameObject prefab;

        public Source(string _name, string _path, GameObject _prefab, bool _isEnabled = true, bool _isVisible = true, bool _isInterpolate = true){
            name            = _name;
            path            = _path;
            prefab          = _prefab;
            isInterpolate   = _isInterpolate;
            
            Init();
        }

        public void Init(){
            loader.MarkerFromCSV(path, prefab, ref time, ref markers);
            Main.player.time.OnChange( () => Update(Main.player.time.Value) );
            foreach(string cam in cams) markers[cam].AddCamera();
            Main.player.UpdateLastTimestamp( time.Last() );

            // frame.OnChange( () => Update( time[frame.Value] ));
        }

        public void Update(float currentTime){
            float frame = GetFrame(currentTime);
            int f       = (int) Mathf.Floor(frame);
            float r     = frame % 1;

            foreach(Marker marker in markers.Values) marker.Update(f, r);
        }

        public float GetFrame(float currentTime) {
            int frame = -1;
            for(int i = 0; i < time.Count; i++) if(currentTime <= time[i]) { frame = i; break; }
            
            if (time[frame] == time.Last()) return frame;
            if (time[frame] == currentTime) return frame;

            if (isInterpolate) return frame + (currentTime - time[frame]) / (time[frame+1] - time[frame]);
            return frame;
        }
    }

    public class Marker{
        public string name;
        public List<int> index; // Column indices in data file
        public List<Vector3> position;
        public List<Quaternion> rotation;
        public GameObject game_object;

        public Bool enable = new Bool(true);

        public Marker(string _name){
            name            = _name;
            position        = new List<Vector3>();
            rotation        = new List<Quaternion>();

            game_object             = new GameObject();
            game_object.name        = name;

            OnChange();
        }

        public Marker(GameObject _game_object, string _name){
            name            = _name;
            position        = new List<Vector3>();
            rotation        = new List<Quaternion>();

            game_object             = GameObject.Instantiate(_game_object);
            game_object.name        = name;

            OnChange();
        }

        public Marker(GameObject _game_object, string _name, List<int> _index){
            name            = _name;
            index           = _index;
            position        = new List<Vector3>();
            rotation        = new List<Quaternion>();

            game_object             = GameObject.Instantiate(_game_object);
            game_object.name        = name;

            OnChange();
        }

        public Marker(string _name, List<Vector3> _position, List<Quaternion> _rotation, bool _enable = true){
            name            = _name;
            position        = _position;
            rotation        = _rotation;
            enable          = new Bool(_enable);

            game_object             = new GameObject();
            game_object.name        = name;

            // UpdateTransform();
            OnChange();
        }

        public Marker(GameObject _game_object, string _name, List<Vector3> _position, List<Quaternion> _rotation, bool _enable = true){
            name            = _name;
            position        = _position;
            rotation        = _rotation;
            enable          = new Bool(_enable);

            game_object             = GameObject.Instantiate(_game_object);
            game_object.name        = name;

            // UpdateTransform();
            OnChange();
        }

        public void OnChange(){
            enable.OnChange         ( () => ToggleGameObject() );
            // currentFrame.OnChange   ( () => UpdateTransform() );
        }

        public void Update(int frame = 0, float r = 0) {
            if (r == 0) {
                game_object.transform.position = position[frame];
                game_object.transform.rotation = rotation[frame];
            }
            else {
                game_object.transform.position = Vector3.Lerp(      position[frame], position[frame+1], r );
                game_object.transform.rotation = Quaternion.Lerp(   rotation[frame], rotation[frame+1], r );
            }
        }

        public void ToggleGameObject(){ game_object.SetActive(enable.Value); }

        public void AddCamera(bool isMainCamera = true){
            game_object.AddComponent<Camera>(); 
            game_object.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            game_object.GetComponent<Camera>().backgroundColor = Color.black;
            if(isMainCamera) game_object.tag = "MainCamera";
            // game_object.AddComponent<CameraFilter>();
        }
    }

    public class Loader{
        public Loader(){}

        public Dictionary<string, Marker> MarkerFromCSV( string path, GameObject prefab, ref List<float> time, ref Dictionary<string, Marker> markers){
            time        = new List<float>();
            markers     = new Dictionary<string, Marker>();

            var text    = File.ReadAllText(path);
            var lines   = text.Split('\n');
            var headers = lines[0].Split(',');

            for(int i = 0; i < headers.Length; i++){ 
                var header = headers[i];
                if( !header.Contains('_') ) continue;

                var name = header.Split('_');
                if( !markers.ContainsKey(name[0]) ) markers.Add( name[0], new Marker(prefab, name[0], new List<int>()) );
                markers[name[0]].index.Add(i);
            }

            var max_index = 0;
            foreach(Marker marker in markers.Values) max_index = max_index >= marker.index.Last() ? max_index : marker.index.Last();
            Debug.Log($"Max Index: {max_index}");

            for(int i = 1; i < lines.Length; i++){
                var line = lines[i].Split(',');
                if( max_index > line.Length) continue;

                foreach(Marker marker in markers.Values){
                    var data = new List<float>();
                    foreach(int idx in marker.index) data.Add( float.Parse( line[idx] ) );
                    marker.position.Add( new Vector3(data[0], data[1], data[2]) );
                    marker.rotation.Add( Quaternion.Euler(data[3], data[4], data[5]) );
                }

                time.Add( float.Parse(line[0]) );
            }
            
            for(int i = 1; i < time.Count; i++) time[i] = time[i] - time[0];
            time[0] = 0;

            foreach(Marker marker in markers.Values) { marker.Update(); } // Set initial position

            return markers;
        }

        public Dictionary<string, Marker> MarkerFromDB ( string key, GameObject prefab, ref List<float> time, ref Dictionary<string, Marker> markers){
            
            return markers;
        }
    }



    // Camera.transform.LookAt(gazeVector + Camera.transform.position, Camera.transform.rotation * Vector3.up)
    
}