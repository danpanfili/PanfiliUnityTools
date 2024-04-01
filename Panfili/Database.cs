using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System;
using System.Linq;

namespace Panfili {
    public class Database{
        public string path;
        public Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

        public Database(string _path){
            path = _path;
        }

        public void Select(string table, string col = "*", string filter=""){
            var request = $"SELECT {col} FROM \"{table}\";";
            if(filter!="") request += $" {filter}";

            var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = request;

            using (var reader = command.ExecuteReader()){
                while(reader.Read()){
                    string name     = reader.GetString(1);
                    string dtype    = reader.GetString(2);
                    byte[] blob     = (byte[]) reader.GetValue(6);
                    string key      = reader.GetString(7);

                    dynamic decodedData = DecodeBlob(blob, dtype);
                    // Debug.Log($"Key: {key}, Decoded Data: {decodedData[0]}");
                    data.Add(key, decodedData);
                }
            }

            var group = Group();
            ConvertAndRemove(group);

            connection.Close();
        }

        private void ConvertAndRemove(Dictionary<string, List<int>> group) {
            foreach(var gk in group.Keys){
                int count = group[gk].Count;
                if(count == 3 || count == 4){
                    Debug.Log($"Key: {gk}");
                    ConvertAndRemoveForKey(gk, group[gk], count);
                }
            }
        }

        private void ConvertAndRemoveForKey(string key, List<int> idx, int count) {
            List<float> values = new List<float>();
            var dkeys = data.Keys.ToList();
            var keys = new List<string>();

            for (int i = 0; i < count; i++) keys.Add(dkeys[idx[i]]);

            foreach(var k in keys)      values.Add((float) data[k]);
            Debug.Log($"Values: {values[0]}");

            // for(int i = 0; i < )

            if (count == 3)             data[key] = new Vector3(values[0], values[1], values[2]);
            else if (count == 4)        data[key] = new Quaternion(values[0], values[1], values[2], values[3]);

            foreach (var k in keys)     data.Remove(k.ToString());

        }

        private dynamic DecodeBlob(byte[] data, string dtype) {
            dynamic arr;
            if (dtype == "float64")     arr = new double[data.Length];
            else if (dtype == "int64")  arr = new long[data.Length];
            else return null;

            Buffer.BlockCopy(data, 0, arr, 0, data.Length);
            return arr;
        }

        public Dictionary<string, List<int>> Group(){
            var group = new Dictionary<string, List<int>>();
            var solo = new Dictionary<string, int>();
            var keys = data.Keys.ToList();

            Debug.Log("Group");

            for(int i = 0; i < keys.Count; i++){
                var key = keys[i];
                key = key.Remove(key.Length - 1, 1);

                if(!solo.ContainsKey(key))          solo.Add(key,i);
                else if(!group.ContainsKey(key))    group.Add(key, new List<int>{solo[key], i});
                else                                group[key].Add(i);

                // Debug.Log($"Key: {key}");
            }

            return group;
        }
    }
}
