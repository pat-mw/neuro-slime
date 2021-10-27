using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using Cysharp.Threading.Tasks;
using RetinaNetworking.Server;
using ScriptableObjectArchitecture;

[DefaultExecutionOrder(-5)]
public class SlimePresetLoader : SerializedMonoBehaviour
{
    [Header("DATA")]
    public ConnectionParams connectionParams;
    public GameEvent onMoodChanged;

    [Header("PRESETS")]
    [SerializeField] private string saveFolder = "Presets";

    [SerializeField] private string positiveSubFolder = "Pos";
    [SerializeField] private string negativeSubFolder = "Neg";
    [SerializeField] private string neutralSubFolder = "Neut";

    [ListDrawerSettings(ShowIndexLabels = true, ShowPaging = true, ShowItemCount = true, HideRemoveButton = false, ListElementLabelName = "presetName")]
    [OdinSerialize] private List<SlimePreset> allPresets = new List<SlimePreset>();
    [ListDrawerSettings(ShowIndexLabels = true, ShowPaging = true, ShowItemCount = true, HideRemoveButton = false, ListElementLabelName = "presetName")]
    [OdinSerialize] private List<SlimePreset> positivePresets = new List<SlimePreset>();
    [ListDrawerSettings(ShowIndexLabels = true, ShowPaging = true, ShowItemCount = true, HideRemoveButton = false, ListElementLabelName = "presetName")]
    [OdinSerialize] private List<SlimePreset> negativePresets = new List<SlimePreset>();
    [ListDrawerSettings(ShowIndexLabels = true, ShowPaging = true, ShowItemCount = true, HideRemoveButton = false, ListElementLabelName = "presetName")]
    [OdinSerialize] private List<SlimePreset> neutralPresets = new List<SlimePreset>();


    [Header("CHANGE PRESET")]
    [OnValueChanged("ChangePreset")]
    [ValueDropdown("GetAllLoadedPresets", DropdownTitle = "Select preset", IsUniqueList = true)]
    [OdinSerialize] private SlimePreset currentPreset;


    [Header("SLIME SETTINGS")]
    [InlineEditor(InlineEditorModes.FullEditor)]
    [System.NonSerialized] [OdinSerialize] public SlimeSettings settings;


    private void Awake()
    {
        onMoodChanged.AddListener(MoodChanged);
    }

    private void Start()
    {
        RefreshSlimePresetList();
    }


    void MoodChanged()
    {
        Mood mood = connectionParams.FetchMood();
        switch (mood)
        {
            case Mood.POSITIVE:
                RandomPositivePreset();
                break;
            case Mood.NEGATIVE:
                RandomNegativePreset();
                break;
            case Mood.NEUTRAL:
                RandomNeutralPreset();
                break;
        }
    }

    async private void UpdateSlimeValues(float transitionDuration)
    {
        float time = 0f;

        Debug.Log($"transition duration: {transitionDuration}");

        while (time <= transitionDuration)
        {
            time += Time.fixedDeltaTime;
            
            // sim settings
            settings.numAgents = (int)Mathf.SmoothStep(settings.numAgents, currentPreset.numAgents, time / transitionDuration);
            settings.trailWeight = Mathf.SmoothStep(settings.trailWeight, currentPreset.trailWeight, time / transitionDuration);
            settings.decayRate = Mathf.SmoothStep(settings.decayRate, currentPreset.decayRate, time / transitionDuration);
            settings.diffuseRate = Mathf.SmoothStep(settings.diffuseRate, currentPreset.diffuseRate, time / transitionDuration);

            // species settings
            for (int i = 0; i < settings.speciesSettings.Length; i++)
            {
                try
                {
                    settings.speciesSettings[i].colour = Color.LerpUnclamped(settings.speciesSettings[i].colour, currentPreset.speciesSettings[i].colour, time/transitionDuration);
                    settings.speciesSettings[i].moveSpeed = Mathf.SmoothStep(settings.speciesSettings[i].moveSpeed, currentPreset.speciesSettings[i].moveSpeed, time / transitionDuration);
                    settings.speciesSettings[i].turnSpeed = Mathf.SmoothStep(settings.speciesSettings[i].turnSpeed, currentPreset.speciesSettings[i].turnSpeed, time / transitionDuration);
                    settings.speciesSettings[i].sensorAngleSpacing = Mathf.SmoothStep(settings.speciesSettings[i].sensorAngleSpacing, currentPreset.speciesSettings[i].sensorAngleSpacing, time / transitionDuration);
                    settings.speciesSettings[i].sensorOffsetDst = Mathf.SmoothStep(settings.speciesSettings[i].sensorOffsetDst, currentPreset.speciesSettings[i].sensorOffsetDst, time / transitionDuration);
                    settings.speciesSettings[i].sensorSize = (int)Mathf.SmoothStep(settings.speciesSettings[i].sensorSize, currentPreset.speciesSettings[i].sensorSize, time / transitionDuration);
                }
                catch (System.Exception)
                {

                    throw;
                }
            }

            // loop delay
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }

        // fix the values at the end
        settings.numAgents = currentPreset.numAgents;
        settings.trailWeight = currentPreset.trailWeight;
        settings.decayRate = currentPreset.decayRate;
        settings.diffuseRate = currentPreset.diffuseRate;
        settings.speciesSettings = currentPreset.speciesSettings;

        await UniTask.WaitForEndOfFrame();
        Debug.Log($"FINISHED TRANSITIONING");
    }

    private void RefreshSlimePresetList()
    {
        RefreshPositivePresets();
        RefreshNegativePresets();
        RefreshNeutralPresets();

        foreach (SlimePreset preset in positivePresets)
        {
            allPresets.Add(preset);
        }
        foreach (SlimePreset preset in neutralPresets)
        {
            allPresets.Add(preset);
        }
        foreach (SlimePreset preset in negativePresets)
        {
            allPresets.Add(preset);
        }

        Wenzil.Console.Console.Log($"Loaded {allPresets.Count} presets in total");
    }

    private void RefreshPositivePresets()
    {
        positivePresets.Clear();

        var folder = $"{saveFolder}/{positiveSubFolder}/";

        Wenzil.Console.Console.Log($"Attempting to load existing presets from folder: {folder}");

        // since we are loading a JSON file we cannot easily filter by type
        // so assume all objects in the folder are presets, and catch any errors later
        TextAsset[] presetRefTextAssets = Resources.LoadAll<TextAsset>(folder);
        string[] presetJsonStrings = new string[presetRefTextAssets.Length];
        for (int i = 0; i < presetJsonStrings.Length; i++)
        {
            presetJsonStrings[i] = presetRefTextAssets[i].ToString();
        }
        foreach (string jsonString in presetJsonStrings)
        {
            try
            {
                SlimePreset preset = SlimeSerializer.LoadSlimePresetFromString(jsonString);
                positivePresets.Add(preset);

                if (preset.presetName == "default")
                {
                    Wenzil.Console.Console.Log("Found default preset");
                    InitialPreset(preset);
                }
            }
            catch (System.Exception e)
            {
                Wenzil.Console.Console.LogError($"Failed to load given slime preset" +
                    $"\n are you sure it is the correct type? " +
                    $"\n Full Exception: {e}");
            }
        }
    }

    private void RefreshNegativePresets()
    {
        negativePresets.Clear();

        var folder = $"{saveFolder}/{negativeSubFolder}/";

        Wenzil.Console.Console.Log($"Attempting to load existing presets from folder: {folder}");

        // since we are loading a JSON file we cannot easily filter by type
        // so assume all objects in the folder are presets, and catch any errors later
        TextAsset[] presetRefTextAssets = Resources.LoadAll<TextAsset>(folder);
        string[] presetJsonStrings = new string[presetRefTextAssets.Length];
        for (int i = 0; i < presetJsonStrings.Length; i++)
        {
            presetJsonStrings[i] = presetRefTextAssets[i].ToString();
        }
        foreach (string jsonString in presetJsonStrings)
        {
            try
            {
                SlimePreset preset = SlimeSerializer.LoadSlimePresetFromString(jsonString);
                negativePresets.Add(preset);

                if (preset.presetName == "default")
                {
                    Wenzil.Console.Console.Log("Found default preset");
                    InitialPreset(preset);
                }
            }
            catch (System.Exception e)
            {
                Wenzil.Console.Console.LogError($"Failed to load given slime preset" +
                    $"\n are you sure it is the correct type? " +
                    $"\n Full Exception: {e}");
            }
        }
    }

    private void RefreshNeutralPresets()
    {
        neutralPresets.Clear();

        var folder = $"{saveFolder}/{neutralSubFolder}/";

        Wenzil.Console.Console.Log($"Attempting to load existing presets from folder: {folder}");

        // since we are loading a JSON file we cannot easily filter by type
        // so assume all objects in the folder are presets, and catch any errors later

        // old technique
        //string filter = "";
        //string[] presetRefs = AssetDatabase.FindAssets(filter, new[] { folder });

        TextAsset[] presetRefTextAssets = Resources.LoadAll<TextAsset>(folder);
        string[] presetJsonStrings = new string[presetRefTextAssets.Length];
        for (int i = 0; i < presetJsonStrings.Length; i++)
        {
            presetJsonStrings[i] = presetRefTextAssets[i].ToString();
        }
        foreach (string jsonString in presetJsonStrings)
        {
            try
            {
                SlimePreset preset = SlimeSerializer.LoadSlimePresetFromString(jsonString);
                neutralPresets.Add(preset);

                if (preset.presetName == "default")
                {
                    Wenzil.Console.Console.Log("Found default preset");
                    InitialPreset(preset);
                }
            }
            catch (System.Exception e)
            {
                Wenzil.Console.Console.LogError($"Failed to load given slime preset " +
                    $"\n are you sure it is the correct type? " +
                    $"\n Full Exception: {e}");
            }
        }
    }

    private void ChangePreset(SlimePreset slimePreset)
    {
        Debug.Log($"CHANGING PRESET: from {currentPreset.presetName} to {slimePreset.presetName}");
        // then set the preset
        currentPreset = slimePreset;
        float transitionDuration = GlobalConfig.TRANSITION_DURATION;
        UpdateSlimeValues(transitionDuration);
    }

    private void InitialPreset(SlimePreset slimePreset)
    {
        Debug.Log($"SETTING INITIAL PRESET: {slimePreset.presetName}");
        float transitionDuration = 0f;
        currentPreset = slimePreset;
        UpdateSlimeValues(transitionDuration);
    }

    private void OnDisable()
    {
        currentPreset = null;
        allPresets.Clear();
        positivePresets.Clear();
        negativePresets.Clear();
        neutralPresets.Clear();
        settings.numAgents = 0;
        settings.spawnMode = Simulation.SpawnMode.Bitmap;
        settings.trailWeight = 0;
        settings.decayRate = 0;
        settings.diffuseRate = 0;
        settings.speciesSettings = null;
    }

    private IEnumerable GetAllLoadedPresets()
    {
        if (Application.isPlaying)
        {
            foreach (SlimePreset preset in positivePresets)
            {
                yield return new ValueDropdownItem($"POS_{preset.presetName}", preset);
            }
            foreach (SlimePreset preset in negativePresets)
            {
                yield return new ValueDropdownItem($"NEG_{preset.presetName}", preset);
            }
            foreach (SlimePreset preset in neutralPresets)
            {
                yield return new ValueDropdownItem($"NEUT_{preset.presetName}", preset);
            }
        }
        else
        {
            yield return null;
        }
    }
   
    private IEnumerable GetAllLoadedPositivePresets()
    {
        if (Application.isPlaying)
        {
            foreach (SlimePreset preset in positivePresets)
            {
                allPresets.Add(preset);
                yield return new ValueDropdownItem($"POS_{preset.presetName}", preset);
            }
        }
    }

    private IEnumerable GetAllLoadedNegativePresets()
    {
        if (Application.isPlaying)
        {
            foreach (SlimePreset preset in negativePresets)
            {
                yield return new ValueDropdownItem($"NEG_{preset.presetName}", preset);
            }
        }
    }

    private IEnumerable GetAllLoadedNeutralPresets()
    {
        if (Application.isPlaying)
        {
            foreach (SlimePreset preset in neutralPresets)
            {
                yield return new ValueDropdownItem($"NEUT_{preset.presetName}", preset);
            }
        }
    }


    [GUIColor(0.8f, 0.6f, 1)]
    [Button(ButtonSizes.Large)]
    [LabelText(" << PREVIOUS PRESET")]
    [ButtonGroup("PresetSwitcher")]
    public void PreviousPreset()
    {
        // get index of current preset
        var index = allPresets.IndexOf(currentPreset);

        // iterate index (find previous preset)
        index -= 1;

        // check overflow
        if (index < 0)
        {
            index = allPresets.Count - 1;
        }

        // get and set preset
        var preset = allPresets[index];
        ChangePreset(preset);
    }

    [GUIColor(1, 0.85f, 0.45f)]
    [Button(ButtonSizes.Large)]
    [LabelText("NEXT PRESET >>")]
    [ButtonGroup("PresetSwitcher")]
    public void NextPreset()
    {
        // get index of current preset
        var index = allPresets.IndexOf(currentPreset);

        // iterate index (find next preset)
        index += 1;

        Debug.Log($"ALL PRESETS COUNT: {allPresets.Count}");
        // check overflow
        if (index >= allPresets.Count)
        {
            index = 0;
        }

        // get and set preset
        var preset = allPresets[index];
        ChangePreset(preset);
    }



    [GUIColor(1, 0.188f, 0.756f)]
    [Button(ButtonSizes.Large)]
    [LabelText("RANDOMISE")]
    public void RandomiseSettings()
    {
        /// SLIME SETTINGS
        //public int width = 1080;
        //public int height = 1080;
        //[Range(1, 5)] public int stepsPerFrame = 1;
        //[Range(100000, 500000)] public int numAgents = 250000;
        //public Simulation.SpawnMode spawnMode = Simulation.SpawnMode.InwardCircle;
        //[Range(0, 100)] public float trailWeight = 1f;
        //[Range(0.1f, 10f)] public float decayRate = 1f;
        //[Range(0, 100)] public float diffuseRate = 1f;
        //public SpeciesSettings[] speciesSettings;


        // create a new slime preset 
        var stepsPerFrame = Random.Range(1, 5);
        var width = 1080;
        var height = 1080;
        var numAgents = Random.Range(100000, 500000);
        var trailWeight = Random.Range(10f, 100f);
        var decayRate = Random.Range(0.1f, 6f);
        var diffuseRate = Random.Range(0f, 100f);

        /// SPECIES SETTINGS
        //[Range(0, 100)] public float moveSpeed;
        //[Range(-15, 15)] public float turnSpeed;
        //[Range(0, 100)] public float sensorAngleSpacing;
        //[Range(-75, 75)] public float sensorOffsetDst;
        //[Range(0, 10)] public int sensorSize;
        //public Color colour;

        SlimeSettings.SpeciesSettings[] species = new SlimeSettings.SpeciesSettings[3];

        // first species is the logo (zero movement)
        species[0].moveSpeed = 0;
        species[0].turnSpeed = 0;
        species[0].sensorAngleSpacing = 0;
        species[0].sensorOffsetDst = 0;
        species[0].sensorSize = 0;
        species[0].colour = Color.white;

        // remaining two species are random
        for (int i = 1; i < 3; i++)
        {
            species[i].moveSpeed = Random.Range(0f, 100f);
            species[i].turnSpeed = Random.Range(-15f, 15f);
            species[i].sensorAngleSpacing = Random.Range(0f, 100f);
            species[i].sensorOffsetDst = Random.Range(-75f, 75f);
            species[i].sensorSize = Random.Range(1, 10);
            var randomColor = Random.ColorHSV();
            species[i].colour = Color.HSVToRGB(randomColor[0], Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
        }

        // create slime preset and apply it
        SlimePreset preset = new SlimePreset("random", stepsPerFrame, width, height, numAgents, trailWeight, decayRate, diffuseRate, species);
        allPresets.Add(preset);
        ChangePreset(preset);
    }

    [GUIColor(0, 0.85f, 0)]
    [Button(ButtonSizes.Large)]
    [LabelText("RANDOM POSITIVE")]
    [ButtonGroup("MoodSwitcher")]
    public void RandomPositivePreset()
    {
        SlimePreset preset = positivePresets[Random.Range(0, positivePresets.Count)];
        Wenzil.Console.Console.Log($"SELECTED POSITIVE PRESET: {preset.presetName}");
        ChangePreset(preset);
    }

    [GUIColor(0.5f, 0.5f, 0.5f)]
    [Button(ButtonSizes.Large)]
    [LabelText("RANDOM NEUTRAL")]
    [ButtonGroup("MoodSwitcher")]
    public void RandomNeutralPreset()
    {
        SlimePreset preset = neutralPresets[Random.Range(0, neutralPresets.Count)];
        Wenzil.Console.Console.Log($"SELECTED NEUTRAL PRESET: {preset.presetName}");
        ChangePreset(preset);
    }

    [GUIColor(0.8f, 0.1f, 0)]
    [Button(ButtonSizes.Large)]
    [LabelText("RANDOM NEGATIVE")]
    [ButtonGroup("MoodSwitcher")]
    public void RandomNegativePreset()
    {
        SlimePreset preset = negativePresets[Random.Range(0, negativePresets.Count)];
        Wenzil.Console.Console.Log($"SELECTED NEGATIVE PRESET: {preset.presetName}");
        ChangePreset(preset);
    }


    [Button(ButtonSizes.Large)]
    public void SavePreset(string presetPath)
    {
        if (presetPath == null)
            presetPath = $"NewSlime_{System.DateTime.Now}";

        SlimePreset slimePreset = new SlimePreset(
            presetPath, settings.stepsPerFrame, settings.width, settings.height, settings.numAgents, settings.trailWeight, settings.decayRate,
            settings.diffuseRate, settings.speciesSettings);

        SlimeSerializer.SaveSlimePreset(slimePreset, saveFolder);
        allPresets.Add(slimePreset);
    }

    [Button(ButtonSizes.Large)]
    public void LoadPreset(string presetPath)
    {
        if (presetPath == null)
            throw new System.Exception("Preset Name cannot be null, please add name before saving");

        try
        {
            string savePath = $"{saveFolder}/{presetPath}.json";
            SlimePreset slimePreset = SlimeSerializer.LoadSlimePreset(savePath);
            allPresets.Add(slimePreset);
            ChangePreset(slimePreset);
        }
        catch (System.Exception e)
        {
            Wenzil.Console.Console.LogError($"Error loading slime preset: {presetPath} \n Full Exception: {e}");
        }
    }

}

