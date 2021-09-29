using UnityEngine;


public class SlimePreset
{
    public string presetName;
    public int stepsPerFrame;
    public int width, height;
    public int numAgents;
    public float trailWeight;
    public float decayRate;
    public float diffuseRate;
    public SlimeSettings.SpeciesSettings[] speciesSettings;

    public SlimePreset
        (string _presetName, int _stepsPerFrame, int _width, int _height,
         int _numAgents, float _trailWeight, 
         float _decayRate, float _diffuseRate, 
         SlimeSettings.SpeciesSettings[] _speciesSettings)
    {
        presetName = _presetName;
        stepsPerFrame = _stepsPerFrame;
        width = _width;
        height = _height;
        numAgents = _numAgents;
        trailWeight = _trailWeight;
        decayRate = _decayRate;
        diffuseRate = _diffuseRate;
        speciesSettings = _speciesSettings;
        Debug.Log($"Initialised Slime Preset: {presetName}");
    }
}
