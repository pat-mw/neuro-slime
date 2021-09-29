using UnityEngine;
using System.IO;
using System;
using Wenzil.Console;

namespace RetinaNetworking
{
    public class DataSave
    {
        public static string saveFolder = "SavedData";
        public static string fileName = "Data";

        public static void SaveByteData(byte[] data, int clientID, out string outPath)
        {
            // saving data locally
            try
            {
                // use persistent path on builds, use normal data path in Editor for easier testing
                #if UNITY_EDITOR
                string path = Path.Combine(Application.dataPath, saveFolder);
                #else
                string path = Path.Combine(Application.persistentDataPath, saveFolder);
                #endif

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, NormaliseFilename(fileName, clientID));
                path = NormalisePath(path);


                File.WriteAllBytes(path, data);

                Wenzil.Console.Console.Log($"saved JSON (bytes) to: {path}");

                outPath = path;
                return;
            }
            catch (Exception ex)
            {
                Wenzil.Console.Console.Log("Error writing file" + ex);
                outPath = null;
                return;
            }
        }

        public static void SaveStringData(string data, int clientID, out string outPath)
        {
            // saving data locally
            try
            {
                // use persistent path on builds, use normal data path in Editor for easier testing
                 #if UNITY_EDITOR
                string path = Path.Combine(Application.dataPath, saveFolder);
                #else
                string path = Path.Combine(Application.persistentDataPath, saveFolder);
                #endif

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, NormaliseFilename(fileName, clientID));
                path = NormalisePath(path);


                File.WriteAllText(path, data);

                Wenzil.Console.Console.Log($"saved JSON (string) to: {path}");

                outPath = path;
                return;
            }
            catch (Exception ex)
            {
                Wenzil.Console.Console.Log("Error writing file" + ex);
                outPath = null;
                return;
            }
        }

        private static string NormalisePath(string path)
        {
            var normalised = path.Replace(@"\", "/");
            return normalised;
        }

        private static string NormaliseFilename(string _fileName, int clientID)
        {
            var _fileExtension = ".txt";
            var _DateAndTime = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
            var _clientID = clientID.ToString();
            return $"{_fileName}_Client{_clientID}_{_DateAndTime}{_fileExtension}";
        }
    }
}

