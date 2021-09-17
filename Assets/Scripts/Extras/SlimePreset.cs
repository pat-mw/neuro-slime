using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;


public class SlimePreset
{
    public string presetName;
    public int stepsPerFrame;
    public int width, height;
    public int numAgents;
    public Simulation.SpawnMode spawnMode;
    public float trailWeight;
    public float decayRate;
    public float diffuseRate;
    public SlimeSettings.SpeciesSettings[] speciesSettings;
    public Texture2D spawnBitmap;

    public SlimePreset
        (string _presetName, int _stepsPerFrame, int _width, int _height,
         int _numAgents, Simulation.SpawnMode _spawnMode, float _trailWeight, 
         float _decayRate, float _diffuseRate, 
         SlimeSettings.SpeciesSettings[] _speciesSettings, Texture2D _spawnBitmap)
    {
        presetName = _presetName;
        stepsPerFrame = _stepsPerFrame;
        width = _width;
        height = _height;
        numAgents = _numAgents;
        spawnMode = _spawnMode;
        trailWeight = _trailWeight;
        decayRate = _decayRate;
        diffuseRate = _diffuseRate;
        speciesSettings = _speciesSettings;
        spawnBitmap = _spawnBitmap;

        Debug.Log($"Initialised Slime Preset: {presetName}");
    }
}
