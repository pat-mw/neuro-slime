using UnityEngine;
using UnityEngine.Experimental.Rendering;
using ComputeShaderUtility;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using UnityEditor;
using System;

public class Simulation : SerializedMonoBehaviour
{ 
	public enum SpawnMode { Random, Point, InwardCircle, RandomCircle, Bitmap }

	const int updateKernel = 0;
	const int diffuseMapKernel = 1;
	const int colourKernel = 2;

	[Header("SLIME SETTINGS")]
	[InlineEditor(InlineEditorModes.FullEditor)]
	[NonSerialized][OdinSerialize] public SlimeSettings settings;
	
	//[Min(1)] public int stepsPerFrame = 1;
	//public int width = 1280;
	//public int height = 720;
	//public int numAgents = 100;
	//public Simulation.SpawnMode spawnMode;

	//[Header("Trail Settings")]
	//public float trailWeight = 1;
	//public float decayRate = 1;
	//public float diffuseRate = 1;


	[Header("SHADERS")]
	public ComputeShader compute;
	public ComputeShader drawAgentsCS;

	[Header("DISPLAY")]
	public bool showAgentsOnly;
	public FilterMode filterMode = FilterMode.Point;
	public GraphicsFormat format = ComputeHelper.defaultGraphicsFormat;

	[Header("SPAWN SHAPE (Optional)")]
	public Texture2D spawnBitmap;

	//[Header("MAPPINGS")]
	//public List<IMapping> Mappings;

	[SerializeField, HideInInspector] protected RenderTexture trailMap;
	[SerializeField, HideInInspector] protected RenderTexture diffusedTrailMap;
	[SerializeField, HideInInspector] protected RenderTexture displayTexture;

	ComputeBuffer agentBuffer;
	ComputeBuffer settingsBuffer;
	Texture2D colourMapTexture;

	protected virtual void Start()
	{
		Init();
		transform.GetComponentInChildren<MeshRenderer>().material.mainTexture = displayTexture;
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
					if (!spawnBitmap)
					{
						Debug.LogError("Attempting to spawn in Bitmap mode but no Bitmap provided in settings");
						return;
					}

					int random_number = UnityEngine.Random.Range(1, positions.Count);
					startPos = Scalepoint(positions[random_number], spawnBitmap);
					angle = randomAngle;
				}
                catch (System.Exception)
                {
					Debug.LogError("Error reading bitmap image for spawn");
                }
                
			}

			Vector3Int speciesMask;
			int speciesIndex = 0;
			int numSpecies = settings.speciesSettings.Length;

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
		for (int i = 0; i < settings.stepsPerFrame; i++)
		{
			//ApplyMappings();
			RunSimulation();
		}
	}

	void LateUpdate()
	{
		if (showAgentsOnly)
		{
			ComputeHelper.ClearRenderTexture(displayTexture);
			drawAgentsCS.SetTexture(0, "TargetTexture", displayTexture);
			ComputeHelper.Dispatch(drawAgentsCS, settings.numAgents, 1, 1, 0);
		}
		else
		{
			ComputeHelper.Dispatch(compute, settings.width, settings.height, 1, kernelIndex : colourKernel);
		}
	}

	void ApplyMappings()
    {
		//foreach(IMapping mapping in Mappings)
		//{
		//	mapping.Map();
		//}
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


	[Header("PRESETS")]
	[SerializeField] private string saveFolder = "Presets";
	[ListDrawerSettings(ShowIndexLabels = true, ShowPaging = true, ShowItemCount = true, HideRemoveButton = false, ListElementLabelName = "presetName")]
	[OdinSerialize] private List<SlimePreset> slimePresetList = new List<SlimePreset>();

	[OnValueChanged("ChangePreset")]
	[ValueDropdown("GetAllCurrentlyLoadedPresets", DropdownTitle = "Select current preset", IsUniqueList = true)]
	[OdinSerialize] private SlimePreset currentPreset;


	[Button]
	public void SavePreset(string presetName)
	{
		if (presetName == null)
			presetName = $"NewSlime_{System.DateTime.Now}";

		SlimePreset slimePreset = new SlimePreset(
			presetName, settings.stepsPerFrame, settings.width, settings.height, settings.numAgents,
			settings.spawnMode, settings.trailWeight, settings.decayRate, 
			settings.diffuseRate, settings.speciesSettings, spawnBitmap);

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

	private void RefreshSlimePresetList()
	{
		//var folder = Application.dataPath + $"/{saveFolder}/";
		var folder = $"Assets/{saveFolder}/";

		Debug.Log($"Attempting to load existing presets from folder: {folder}");

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
				Debug.Log($"Found existing slime preset: {preset.presetName}");
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
		var dropdownItems = new List<ValueDropdownItem>();
		foreach (SlimePreset preset in slimePresetList)
		{
			yield return new ValueDropdownItem(preset.presetName, preset);
		}
	}

	private void ChangePreset(SlimePreset slimePreset)
	{
		currentPreset = slimePreset;
		settings.width = currentPreset.width;
		settings.height = currentPreset.height;
		settings.numAgents = currentPreset.numAgents;
		settings.spawnMode = currentPreset.spawnMode;
		settings.trailWeight = currentPreset.trailWeight;
		settings.decayRate = currentPreset.decayRate;
		settings.diffuseRate = currentPreset.diffuseRate;
		settings.speciesSettings = currentPreset.speciesSettings;
	}

    private void OnDisable()
    {
		currentPreset = null;
		slimePresetList.Clear();
    }

}
