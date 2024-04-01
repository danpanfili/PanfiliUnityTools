using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Panfili.OBJ{
    public class Loader{
        // public GameObject Load(string path){}
        string path;
        Dictionary<string, UnityAction> action = new Dictionary<string, UnityAction>();

        public Loader(string _path){
            path = _path;

            action.Add( "mt",  MaterialLibrary );
            action.Add( "g ",  Group );
            action.Add( "v ",  MeshVert );
            action.Add( "vf",  TextureVert);
            action.Add( "f ",  Face);
        }

        void MaterialLibrary(){}
        void Group(){}
        void MeshVert(){}
        void TextureVert(){}
        void Face(){}

        public void ParseText(){
            // ^mtllib (.{1,})|^usemtl (.{1,})|^g (.{1,})|^vt (.{1,}) (.{1,})|^v (.{1,}) (.{1,}) (.{1,})|^f (.{1,})\/(.{1,}) (.{1,})\/(.{1,}) (.{1,})\/(.{1,})
            StreamReader file = new StreamReader(path);
            string line = file.ReadLine();

            while(line != null){
                if( line.Length == 0 ) continue;
                if( line[0] == '#' ) continue;

                line = file.ReadLine();
            }


            file.Close();
            return;
        }

        
    }
}

// using UnityEngine;
// using System.Collections.Generic;
// using System.IO;
// using System.Text.RegularExpressions;

// public class ObjParser : MonoBehaviour
// {
//     public string filePath; // Path to the OBJ file

//     // Arrays to store parsed data
//     public List<string> materialLibraries = new List<string>();
//     public List<string> materials = new List<string>();
//     public List<string> groups = new List<string>();
//     public List<Vector2> textureCoordinates = new List<Vector2>();
//     public List<Vector3> vertices = new List<Vector3>();
//     public List<Vector3> normals = new List<Vector3>();
//     public List<int[]> faces = new List<int[]>();

//     void Start()
//     {
//         ParseObjFile(filePath);
//     }

//     void ParseObjFile(string filePath)
//     {
//         string[] lines = File.ReadAllLines(filePath);

//         Regex regex = new Regex(@"^mtllib (.{1,})|^usemtl (.{1,})|^g (.{1,})|^vt (.{1,}) (.{1,})|^v (.{1,}) (.{1,}) (.{1,})|^f (.{1,})\/(.{1,}) (.{1,})\/(.{1,}) (.{1,})\/(.{1,})");

//         foreach (string line in lines)
//         {
//             Match match = regex.Match(line);
//             if (match.Success)
//             {
//                 if (match.Groups[1].Success)
//                 {
//                     materialLibraries.Add(match.Groups[1].Value);
//                 }
//                 else if (match.Groups[2].Success)
//                 {
//                     materials.Add(match.Groups[2].Value);
//                 }
//                 else if (match.Groups[3].Success)
//                 {
//                     groups.Add(match.Groups[3].Value);
//                 }
//                 else if (match.Groups[4].Success)
//                 {
//                     float x = float.Parse(match.Groups[4].Value);
//                     float y = float.Parse(match.Groups[5].Value);
//                     textureCoordinates.Add(new Vector2(x, y));
//                 }
//                 else if (match.Groups[6].Success)
//                 {
//                     float x = float.Parse(match.Groups[6].Value);
//                     float y = float.Parse(match.Groups[7].Value);
//                     float z = float.Parse(match.Groups[8].Value);
//                     vertices.Add(new Vector3(x, y, z));
//                 }
//                 else if (match.Groups[9].Success)
//                 {
//                     int[] face = new int[6];
//                     for (int i = 0; i < 3; i++)
//                     {
//                         face[i * 2] = int.Parse(match.Groups[9 + i * 2].Value);
//                         face[i * 2 + 1] = int.Parse(match.Groups[10 + i * 2].Value);
//                     }
//                     faces.Add(face);
//                 }
//             }
//         }

//         Debug.Log("Material Libraries: " + string.Join(", ", materialLibraries.ToArray()));
//         Debug.Log("Materials: " + string.Join(", ", materials.ToArray()));
//         Debug.Log("Groups: " + string.Join(", ", groups.ToArray()));
//         Debug.Log("Texture Coordinates: " + string.Join(", ", textureCoordinates.ToArray()));
//         Debug.Log("Vertices: " + string.Join(", ", vertices.ToArray()));
//         Debug.Log("Faces: " + string.Join(", ", faces.ToArray()));
//     }
// }
