using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Panfili.Python{
    public class Server{
        string address = "127.0.0.1";
        int port = 12345;
        int bufferSize = 1024^1;

        private TcpClient client;
        private NetworkStream stream;

        public Server() {}
        public Server(int _bufferSize = 1024^1, int _port = 12345, string _address = "127.0.0.1") { 
            bufferSize = _bufferSize; 
            port = _port; 
            address = _address; 
        }
        public Server(int _port = 12345, string _address = "127.0.0.1") { 
            port = _port; 
            address = _address; 
        }

        public void Init(string script_path = null){
            if( script_path == null ) script_path = $"{Application.dataPath}/Scripts/Panfili/PythonSocket.py";

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            // startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/c python {script_path}";

            process.StartInfo = startInfo;
            process.Start();

            Open();

            Debug.Log($"Initialized Server: {script_path}");
        }

        public void Open(){
            try {
                client = new TcpClient(address, port);
                stream = client.GetStream();
            }
            catch (Exception e) { Debug.LogError("Error: " + e.Message); }
        }

        public void Close() { 
            if (client != null) client.Close(); 
            if (stream != null) stream.Close();
        }

        public string Request(string request, int bufferSize = 1024) {
            try {
                // Open();

                byte[] requestData = Encoding.UTF8.GetBytes(request);
                byte[] buffer = new byte[bufferSize];

                stream.Write(requestData, 0, requestData.Length);

                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Close();
                return response;
            }
            catch (Exception e) { Debug.LogError("Error: " + e.Message); return ""; }
        }
    }
}
