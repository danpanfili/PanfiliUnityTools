using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Dummiesman;

namespace Panfili.Data{

    public class Loader
    {
        public void Environment (ref GameObject env, string path, Quaternion rotation){
            if( CheckPath(path) ) {
                GameObject.Destroy(env);

                OBJ(ref env, path);
                env.AddComponent<MeshCollider>();
                env.transform.GetChild(0).GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                env.transform.localScale = Vector3.one;
                env.transform.rotation = rotation;
            }
        }

        public void Environment (ref GameObject env, string path){
            if( CheckPath(path) ) {
                GameObject.Destroy(env);

                OBJ(ref env, path);
                env.transform.localScale = Vector3.one;
            }
        }

        public Quaternion Rotation(Panfili.Python.Server server, string path){ 
            string[] data_string = server.Request($"rotquat,{path}").Split(",");
            if(data_string.Length != 4) return Quaternion.identity;

            float[] data_arr = new float[data_string.Length];
            for(int i = 0; i < data_string.Length; i++) data_arr[i] = float.Parse(data_string[i]);

            return new Quaternion( data_arr[0], data_arr[1], data_arr[2], data_arr[3] );
        }

        public void OBJ (ref GameObject obj, string path)   { if( CheckPath(path) ) { obj = new OBJLoader().Load( path, path.Replace(".obj",".mtl") ); } }
        public void JSON (ref string json, string path)     { if( CheckPath(path) ) json = File.ReadAllText(path); }

        bool CheckPath (string path){
            if ( !File.Exists(path) ) { Debug.Log($"File not found: {path}"); return false; }

            Debug.Log($"Loading: {path}");
            return true;
        }
    }

    public class Converter{
        public void Str2Quat(string s){
            return;
        }
    }

    public class Info{        
    }
}