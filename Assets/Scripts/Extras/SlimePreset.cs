using UnityEngine;

public class SlimePreset
{
    public string presetName;

    public int width, height;
    public int depth;
    public int numAgents;
    public float moveSpeed;
    public float diffuseSpeed;
    public float evaporateSpeed;

    public int senseRange;
    public float sensorLength;
    public float sensorAngleSpacing;
    public float turnSpeed;
    public float marchingError;

    public SlimePreset
        (string _presetName, int _width, int _height, int _depth,
         int _numAgents, float _moveSpeed, 
         float _diffuseSpeed, float _evaporateSpeed, 
         int _senseRange, float _sensorLength, 
         float _sensorAngleSpacing, float _turnSpeed, 
         float _marchingError)
    {
        presetName = _presetName;
        width = _width;
        height = _height;
        depth = _depth;
        numAgents = _numAgents;
        moveSpeed = _moveSpeed;
        diffuseSpeed = _diffuseSpeed;
        evaporateSpeed = _evaporateSpeed;
        senseRange = _senseRange;
        sensorLength = _sensorLength;
        sensorAngleSpacing = _sensorAngleSpacing;
        turnSpeed = _turnSpeed;
        marchingError = _marchingError;

        Debug.Log($"Initialised Slime Preset: {presetName}");
    }
}
