using UnityEngine;
using Sirenix.Serialization;
using System.IO;
using System;

public class SlimeSerializer
{
    public static void SaveSlimePreset(SlimePreset slimePreset, string saveFolder)
    {
        // Save to Assets folder
        string path = Application.dataPath + $"/{saveFolder}/{slimePreset.presetName}.json";
        DataFormat dataFormat = DataFormat.JSON;

        // Serialization
        try
        {
            var bytes = SerializationUtility.SerializeValue(slimePreset, dataFormat);
            File.WriteAllBytes(path, bytes);
            Debug.Log($"Saved Slime Preset: {slimePreset.presetName} at path: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving slime preset: {slimePreset.presetName} to path: {path} \n Full Exception: {e}");
        }
    }

    public static SlimePreset LoadSlimePreset(string path)
    {
        DataFormat dataFormat = DataFormat.JSON;
        try
        {
            var bytes = File.ReadAllBytes(path);
            SlimePreset slimePreset = SerializationUtility.DeserializeValue<SlimePreset>(bytes, dataFormat);
            //Debug.Log($"Loading slime preset from: {path}");
            return slimePreset;
        }
        catch(Exception e)
        {
            Debug.LogError($"Error loading slime preset from path: {path} \n Full Exception: {e}");
            return null;
        }
            
    }
}
