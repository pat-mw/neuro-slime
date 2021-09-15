using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;

public class SlimeSaver : SerializedMonoBehaviour
{
    public ComputeShader shader;

    [Header("SLIME SETTINGS")]
    [PropertyRange(100, 2000)]
    [SerializeField] private int width = 1920;
    [PropertyRange(100, 2000)]
    [SerializeField] private int height = 1080;
    private int depth = 1;
    [SerializeField] [PropertyRange(0, 1000000)]
    private int numAgents = 1000000;
    [SerializeField] [PropertyRange(0, 3000)]
    private float moveSpeed = 50.0f;
    [SerializeField] [PropertyRange(0, 100)]
    private float diffuseSpeed = 10.0f;
    [SerializeField] [PropertyRange(0, 10)]
    private float evaporateSpeed = 0.3f;

    [Header("SENSOR SETTINGS")]
    [SerializeField]
    private int senseRange = 3;
    [SerializeField]
    private float sensorLength = 8.0f;
    [SerializeField]
    private float sensorAngleSpacing = 30.0f;
    [SerializeField]
    private float turnSpeed = 50.0f;
    [SerializeField]
    private float marchingError = 0.1f;

    [Header("PRESETS")]
    [SerializeField] private string saveFolder = "Presets";
    [OdinSerialize] private List<SlimePreset> slimePresetList;

    [OnValueChanged("ChangePreset")]
    [ValueDropdown("GetAllCurrentlyLoadedPresets", DropdownTitle ="Select current preset", IsUniqueList =true)]
    [OdinSerialize] private SlimePreset currentPreset;

        
    [Button]
    public void SavePreset(string presetName)
    {
        if (presetName == null)
            presetName = $"NewSlime_{System.DateTime.Now}";

        SlimePreset slimePreset = new SlimePreset(
            presetName, width, height, depth, numAgents,
            moveSpeed, diffuseSpeed, evaporateSpeed, senseRange,
            sensorLength, sensorAngleSpacing, turnSpeed, marchingError);

        SlimeSerializer.SaveSlimePreset(slimePreset, saveFolder);
        slimePresetList.Add(slimePreset);
    }

    [Button]
    public void LoadPreset(string presetName)
    {
        if (presetName == null)
            throw new System.Exception("Preset Name cannot be null, please add name before saving");

        try
        {
            string savePath = $"{saveFolder}/{presetName}";
            SlimePreset slimePreset = SlimeSerializer.LoadSlimePreset(savePath);
            slimePresetList.Add(slimePreset);
            ChangePreset(slimePreset);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading slime preset: {presetName} \n Full Exception: {e}");
        }
    }

    private RenderTexture trailMap;
    private RenderTexture trailMapProcessed;
    private ComputeBuffer agentsBuffer;

    private Dictionary<string, int> kernelIndices;

    public struct Agent
    {
        public Vector2 position;
        public float angle;
        public Vector4 type;
    } // size = 7 * 4 bytes

    private Agent[] agents;

    void Start()
    {
        kernelIndices = new Dictionary<string, int>();
        kernelIndices.Add("Update", shader.FindKernel("Update")); // Thread Shape [16, 1, 1]
        kernelIndices.Add("Postprocess", shader.FindKernel("Postprocess")); // Thread Shape [8, 8, 1]

        createNewTexture(ref trailMap);

        agents = new Agent[numAgents];
        for (int i = 0; i < agents.Length; i++)
        {
            float angle = Random.Range(0, 2 * Mathf.PI);
            float len = Random.value * height * 0.9f / 2.0f;
            float x = Mathf.Cos(angle) * len;
            float y = Mathf.Sin(angle) * len;

            agents[i].position = new Vector2(width / 2 + x, height / 2 + y);
            agents[i].angle = angle + Mathf.PI;

            Vector4 type = Vector4.zero;
            type[Random.Range(0, 3)] = 1;
            agents[i].type = type;
            
        }

        agentsBuffer = new ComputeBuffer(agents.Length, sizeof(float) * 7);
        agentsBuffer.SetData(agents);


        RefreshSlimePresetList();
    }

    void FixedUpdate()
    {
        shader.SetTexture(kernelIndices["Update"], "TrailMap", trailMap);

        shader.SetInt("width", width);
        shader.SetInt("height", height);
        shader.SetInt("numAgents", numAgents);
        shader.SetFloat("moveSpeed", moveSpeed);
        shader.SetFloat("deltaTime", Time.fixedDeltaTime);

        shader.SetInt("senseRange", senseRange);
        shader.SetFloat("sensorLength", sensorLength);
        shader.SetFloat("sensorAngleSpacing", sensorAngleSpacing * Mathf.Deg2Rad);
        shader.SetFloat("turnSpeed", turnSpeed);
        shader.SetFloat("marchingError", marchingError);
        shader.SetBuffer(kernelIndices["Update"], "agents", agentsBuffer);
        shader.Dispatch(kernelIndices["Update"], numAgents / 16, 1, 1);

        createNewTexture(ref trailMapProcessed);

        shader.SetFloat("evaporateSpeed", evaporateSpeed);
        shader.SetFloat("diffuseSpeed", diffuseSpeed);
        shader.SetTexture(kernelIndices["Postprocess"], "TrailMap", trailMap);
        shader.SetTexture(kernelIndices["Postprocess"], "TrailMapProcessed", trailMapProcessed);

        shader.Dispatch(kernelIndices["Postprocess"], width / 8, height / 8, 1);

        trailMap.Release();
        trailMap = trailMapProcessed;
    }

    private void createNewTexture(ref RenderTexture renderTexture)
    {
        renderTexture = new RenderTexture(width, height, depth);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(trailMapProcessed, destination);
    }

    private void OnDestroy()
    {
        trailMap.Release();
        trailMapProcessed.Release();
        agentsBuffer.Release();
    }

    private void RefreshSlimePresetList()
    {
        //var folder = Application.dataPath + $"/{saveFolder}/";
        var folder = $"Assets/{saveFolder}/";

        Debug.Log($"Attempting to load existing presets from folder: {folder}");

        // since we are loading a JSON file we cannot easily filter
        // so assume all objects in the folder are presets, and catch any errors later
        string filter = "";
        string[] presetRefs = AssetDatabase.FindAssets(filter, new[] { folder });

        slimePresetList.Clear();
        foreach(string presetRef in presetRefs)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(presetRef);

            try
            {
                SlimePreset preset = SlimeSerializer.LoadSlimePreset(assetPath);
                Debug.Log($"Found existing slime preset: {preset.presetName}");
                slimePresetList.Add(preset);

                if(preset.presetName == "default")
                {
                    Debug.Log("Found default preset");
                    ChangePreset(preset);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load given slime preset: {assetPath} " +
                    $"\n are you sure it is the correct type? " +
                    $"\n Full Exception: {e}");
            }
        }
    }

    private IEnumerable GetAllCurrentlyLoadedPresets()
    {
        var dropdownItems = new List<ValueDropdownItem>();
        foreach (SlimePreset preset in slimePresetList)
        {
            yield return new ValueDropdownItem(preset.presetName, preset);
        }
    }

    private void ChangePreset(SlimePreset slimePreset)
    {
        currentPreset = slimePreset;
        width = currentPreset.width;
        height = currentPreset.height;
        depth = currentPreset.depth;
        numAgents = currentPreset.numAgents;
        moveSpeed = currentPreset.moveSpeed;
        diffuseSpeed = currentPreset.diffuseSpeed;
        evaporateSpeed = currentPreset.evaporateSpeed;
        senseRange = currentPreset.senseRange;
        sensorLength = currentPreset.sensorLength;
        sensorAngleSpacing = currentPreset.sensorAngleSpacing;
        turnSpeed = currentPreset.turnSpeed;
        marchingError = currentPreset.marchingError;
    }
}
