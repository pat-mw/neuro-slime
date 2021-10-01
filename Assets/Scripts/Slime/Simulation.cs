﻿using UnityEngine;
using UnityEngine.Experimental.Rendering;
using ComputeShaderUtility;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using UnityEditor;
//using System;

[DefaultExecutionOrder(-1)]
public class Simulation : SerializedMonoBehaviour
{ 
	public enum SpawnMode { Random, Point, InwardCircle, RandomCircle, Bitmap }

	const int updateKernel = 0;
	const int diffuseMapKernel = 1;
	const int colourKernel = 2;

	[Header("SLIME SETTINGS")]
	[InlineEditor(InlineEditorModes.FullEditor)]
	[System.NonSerialized] [OdinSerialize] public SlimeSettings settings;

	[Header("MAPPINGS")]
	public List<IMapping> Mappings;

	[Header("SHADERS")]
	public ComputeShader compute;
	public ComputeShader drawAgentsCS;

	[Header("DISPLAY")]
	public bool showAgentsOnly;
	public void ShowAgentsOnly()
    {
		showAgentsOnly = true;
    }
	public void ShowTrails()
	{
		showAgentsOnly = false;
	}
	public FilterMode filterMode = FilterMode.Point;
	public GraphicsFormat format = ComputeHelper.defaultGraphicsFormat;

	[Header("SPAWN SHAPE (Optional)")]
	public Texture2D spawnBitmap;

	[Header("PRESETS")]
	[SerializeField] private string saveFolder = "Presets";
	[ListDrawerSettings(ShowIndexLabels = true, ShowPaging = true, ShowItemCount = true, HideRemoveButton = false, ListElementLabelName = "presetName")]
	[OdinSerialize] private List<SlimePreset> slimePresetList = new List<SlimePreset>();

	[OnValueChanged("ChangePreset")]
	[ValueDropdown("GetAllCurrentlyLoadedPresets", DropdownTitle = "Select current preset", IsUniqueList = true)]
	[OdinSerialize] private SlimePreset currentPreset;


	[Button(ButtonSizes.Large)]
	public void SavePreset(string presetName)
	{
		if (presetName == null)
			presetName = $"NewSlime_{System.DateTime.Now}";

		SlimePreset slimePreset = new SlimePreset(
			presetName, settings.stepsPerFrame, settings.width, settings.height, settings.numAgents, settings.trailWeight, settings.decayRate,
			settings.diffuseRate, settings.speciesSettings);

		SlimeSerializer.SaveSlimePreset(slimePreset, saveFolder);
		slimePresetList.Add(slimePreset);
	}

	[Button(ButtonSizes.Large)]
	public void LoadPreset(string presetName)
	{
		if (presetName == null)
			throw new System.Exception("Preset Name cannot be null, please add name before saving");

		try
		{
			string savePath = $"{saveFolder}/{presetName}.json";
			SlimePreset slimePreset = SlimeSerializer.LoadSlimePreset(savePath);
			slimePresetList.Add(slimePreset);
			ChangePreset(slimePreset);
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Error loading slime preset: {presetName} \n Full Exception: {e}");
		}
	}

    [SerializeField, HideInInspector] protected RenderTexture trailMap;
	[SerializeField, HideInInspector] protected RenderTexture diffusedTrailMap;
	[SerializeField, HideInInspector] protected RenderTexture displayTexture;

	ComputeBuffer agentBuffer;
	ComputeBuffer settingsBuffer;
	Texture2D colourMapTexture;


	bool isSimActive = false;


	protected virtual void Awake()
	{
		PreActivateSim();
	}

	void PreActivateSim()
    {
		transform.GetComponentInChildren<MeshRenderer>().material.mainTexture = displayTexture;
		Init();
	}

	
	public void ActivateSim()
    {
		Debug.Log("ACTIVATING SLIME");
		isSimActive = true;
	}


	[GUIColor(1, 0.188f, 0.756f)]
	[Button(ButtonSizes.Large)]
	[LabelText("RANDOMISE")]
	public void RandomiseSettings()
    {
		/// SLIME SETTINGS
		//public int width = 1080;
		//public int height = 1080;
		//[Range(1, 10)] public int stepsPerFrame = 1;
		//[Range(100, 500000)] public int numAgents = 250000;
		//public Simulation.SpawnMode spawnMode = Simulation.SpawnMode.InwardCircle;
		//[Range(0, 100)] public float trailWeight = 1f;
		//[Range(0.1f, 20)] public float decayRate = 1f;
		//[Range(0, 100)] public float diffuseRate = 1f;
		//public SpeciesSettings[] speciesSettings;

		
		// create a new slime preset 
		var stepsPerFrame = Random.Range(1, 5);
		var width = 1080;
		var height = 1080;
		var numAgents = Random.Range(100000, 500000);
		var trailWeight = Random.Range(0f, 100f);
		var decayRate = Random.Range(0.1f, 10f);
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
		slimePresetList.Add(preset);
		ChangePreset(preset);
    }


	[GUIColor(0.8f, 0.6f, 1)]
	[Button(ButtonSizes.Large)]
	[LabelText(" << PREVIOUS PRESET")]
	[ButtonGroup("PresetSwitcher")]
	public void PreviousPreset()
	{
		// get index of current preset
		var index = slimePresetList.IndexOf(currentPreset);

		// iterate index (find previous preset)
		index -= 1;

		// check overflow
		if (index < 0)
		{
			index = slimePresetList.Count - 1;
		}

		// get and set preset
		var preset = slimePresetList[index];
		ChangePreset(preset);
	}

	[GUIColor(1, 0.85f, 0.45f)]
	[Button(ButtonSizes.Large)]
	[LabelText("NEXT PRESET >>")]
	[ButtonGroup("PresetSwitcher")]
	public void NextPreset()
	{
		// get index of current preset
		var index = slimePresetList.IndexOf(currentPreset);

		// iterate index (find next preset)
		index += 1;

		// check overflow
		if (index >= slimePresetList.Count)
		{
			index = 0;
		}

		// get and set preset
		var preset = slimePresetList[index];
		ChangePreset(preset);
	}


	[GUIColor(0, 1, 0)]
	[Button(ButtonSizes.Large)]
	[ButtonGroup("SimActivationButtons")]
	public void ActivateSimButton()
	{
		isSimActive = true;
	}


	[GUIColor(1, 0, 0)]
	[Button(ButtonSizes.Large)]
	[ButtonGroup("SimActivationButtons")]
	public void DeactivateSim()
    {
		isSimActive = false;
		PreActivateSim();
    }

	void Init()
	{
		RefreshSlimePresetList();

		// Create render textures
		ComputeHelper.CreateRenderTexture(ref trailMap, settings.width, settings.height, filterMode, format);
		ComputeHelper.CreateRenderTexture(ref diffusedTrailMap, settings.width, settings.height, filterMode, format);
		ComputeHelper.CreateRenderTexture(ref displayTexture, settings.width, settings.height, filterMode, format);

		// Assign textures
		compute.SetTexture(updateKernel, "TrailMap", trailMap);
		compute.SetTexture(diffuseMapKernel, "TrailMap", trailMap);
		compute.SetTexture(diffuseMapKernel, "DiffusedTrailMap", diffusedTrailMap);
		compute.SetTexture(colourKernel, "ColourMap", displayTexture);
		compute.SetTexture(colourKernel, "TrailMap", trailMap);


		List<Vector2> positions = GetBlackDots(spawnBitmap);
		// Create agents with initial positions and angles
		Agent[] agents = new Agent[settings.numAgents];
		for (int i = 0; i < agents.Length; i++)
		{
			Vector2 centre = new Vector2(settings.width / 2, settings.height / 2);
			Vector2 startPos = Vector2.zero;
			float randomAngle = UnityEngine.Random.value * Mathf.PI * 2;
			float angle = 0;

			if (settings.spawnMode == SpawnMode.Point)
			{
				startPos = centre;
				angle = randomAngle;
			}
			else if (settings.spawnMode == SpawnMode.Random)
			{
				startPos = new Vector2(UnityEngine.Random.Range(0, settings.width), UnityEngine.Random.Range(0, settings.height));
				angle = randomAngle;
			}
			else if (settings.spawnMode == SpawnMode.InwardCircle)
			{
				startPos = centre + UnityEngine.Random.insideUnitCircle * settings.height * 0.5f;
				angle = Mathf.Atan2((centre - startPos).normalized.y, (centre - startPos).normalized.x);
			}
			else if (settings.spawnMode == SpawnMode.RandomCircle)
			{
				startPos = centre + UnityEngine.Random.insideUnitCircle * settings.height * 0.15f;
				angle = randomAngle;
			}
			else if (settings.spawnMode == SpawnMode.Bitmap)
            {
                try
                {
					//Debug.Log("Trying to load via bitmap");

					if (!spawnBitmap)
					{
						Debug.LogError("Attempting to spawn in Bitmap mode but no Bitmap provided in settings");
						return;
					}

					int random_number = UnityEngine.Random.Range(1, positions.Count);
					startPos = Scalepoint(positions[random_number], spawnBitmap);
					angle = randomAngle;
				}
                catch (System.Exception ex)
                {
					Debug.LogError($"Error reading bitmap image for spawn: {ex}");
                }
                
			}

			Vector3Int speciesMask;
			int speciesIndex = 0;
			//int numSpecies = settings.speciesSettings.Length;
			int numSpecies = 3;
			if (numSpecies == 1)
			{
				speciesMask = Vector3Int.one;
			}
			else
			{
				int species = UnityEngine.Random.Range(1, numSpecies + 1);
				speciesIndex = species - 1;
				speciesMask = new Vector3Int((species == 1) ? 1 : 0, (species == 2) ? 1 : 0, (species == 3) ? 1 : 0);
			}

			agents[i] = new Agent() { position = startPos, angle = angle, speciesMask = speciesMask, speciesIndex = speciesIndex };
		}

		ComputeHelper.CreateAndSetBuffer<Agent>(ref agentBuffer, agents, compute, "agents", updateKernel);
		compute.SetInt("numAgents", settings.numAgents);
		drawAgentsCS.SetBuffer(0, "agents", agentBuffer);
		drawAgentsCS.SetInt("numAgents", settings.numAgents);


		compute.SetInt("width", settings.width);
		compute.SetInt("height", settings.height);

	}

	void FixedUpdate()
	{
		if (isSimActive)
        {
			for (int i = 0; i < settings.stepsPerFrame; i++)
			{
				ApplyMappings();
				RunSimulation();
			}
		}
		
	}

	// *** LATE UPDATE??
    private void LateUpdate()
    {
		if (isSimActive)
        {
			if (showAgentsOnly)
			{
				ComputeHelper.ClearRenderTexture(displayTexture);
				drawAgentsCS.SetTexture(0, "TargetTexture", displayTexture);
				ComputeHelper.Dispatch(drawAgentsCS, settings.numAgents, 1, 1, 0);
			}
			else
			{
				ComputeHelper.Dispatch(compute, settings.width, settings.height, 1, kernelIndex: colourKernel);
			}
		}
	}

	void ApplyMappings()
    {
        foreach (IMapping mapping in Mappings)
        {
			//Debug.Log($"Applying mapping: {mapping}");
            mapping.Map();
        }
    }

	void RunSimulation()
	{
		var speciesSettings = settings.speciesSettings;
		ComputeHelper.CreateStructuredBuffer(ref settingsBuffer, speciesSettings);
		compute.SetBuffer(updateKernel, "speciesSettings", settingsBuffer);
		compute.SetBuffer(colourKernel, "speciesSettings", settingsBuffer);

		// Assign settings
		compute.SetFloat("deltaTime", Time.fixedDeltaTime);
		compute.SetFloat("time", Time.fixedTime);

		compute.SetFloat("trailWeight", settings.trailWeight);
		compute.SetFloat("decayRate", settings.decayRate);
		compute.SetFloat("diffuseRate", settings.diffuseRate);
		compute.SetInt("numSpecies", speciesSettings.Length);

		ComputeHelper.Dispatch(compute, settings.numAgents, 1, 1, kernelIndex: updateKernel);
		ComputeHelper.Dispatch(compute, settings.width, settings.height, 1, kernelIndex: diffuseMapKernel);

		ComputeHelper.CopyRenderTexture(diffusedTrailMap, trailMap);
	}

	void OnDestroy()
	{
		ComputeHelper.Release(agentBuffer, settingsBuffer);
	}

	public struct Agent
	{
		public Vector2 position;
		public float angle;
		public Vector3Int speciesMask;
		int unusedSpeciesChannel;
		public int speciesIndex;
	}


	public static List<Vector2> GetBlackDots(Texture2D myBitmap)
	{
		List<Vector2> BlackList = new List<Vector2>();
		for (int i = 0; i < myBitmap.width; i++)
		{
			for (int j = 0; j < myBitmap.height; j++)
			{
				Color pixelColor = myBitmap.GetPixel(i, j);
				if (pixelColor == Color.black)
				{
					BlackList.Add(new Vector2(i, j));

				}
			}
		}
		return BlackList;
	}

	public Vector2 Scalepoint(Vector2 point, Texture2D myBitmap)
	{
		float scaley = settings.height / myBitmap.height;

		float scalex = settings.width / myBitmap.width;
		Vector2 newpoint = new Vector2();
		newpoint.x = (float)point.x * scalex;

		newpoint.y = (float)point.y * scaley;

		return newpoint;
	}


	private void RefreshSlimePresetList()
	{
		slimePresetList.Clear();

		var folder = $"Assets/{saveFolder}/";

		//Debug.Log($"Attempting to load existing presets from folder: {folder}");

		// since we are loading a JSON file we cannot easily filter by type
		// so assume all objects in the folder are presets, and catch any errors later
		string filter = "";
		string[] presetRefs = AssetDatabase.FindAssets(filter, new[] { folder });

		slimePresetList.Clear();
		foreach (string presetRef in presetRefs)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(presetRef);

			try
			{
				SlimePreset preset = SlimeSerializer.LoadSlimePreset(assetPath);
				//Debug.Log($"Found existing slime preset: {preset.presetName}");
				slimePresetList.Add(preset);

				if (preset.presetName == "default")
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
		if (Application.isPlaying)
        {
			var dropdownItems = new List<ValueDropdownItem>();
			foreach (SlimePreset preset in slimePresetList)
			{
				yield return new ValueDropdownItem(preset.presetName, preset);
			}
		}
	}

	private void ChangePreset(SlimePreset slimePreset)
	{
		// then set the preset
		currentPreset = slimePreset;
		settings.width = currentPreset.width;
		settings.height = currentPreset.height;
		settings.numAgents = currentPreset.numAgents;
		settings.trailWeight = currentPreset.trailWeight;
		settings.decayRate = currentPreset.decayRate;
		settings.diffuseRate = currentPreset.diffuseRate;
		settings.speciesSettings = currentPreset.speciesSettings;
	}

    private void OnDisable()
    {
		currentPreset = null;
		slimePresetList.Clear();

		settings.numAgents = 0;
		settings.spawnMode = SpawnMode.Bitmap;
		settings.trailWeight = 0;
		settings.decayRate = 0;
		settings.diffuseRate = 0;
		settings.speciesSettings = null;
	}

}
