using UnityEngine;
using UnityEngine.Experimental.Rendering;
using ComputeShaderUtility;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using UnityEditor;
using Cysharp.Threading.Tasks;
using RetinaNetworking.Server;
//using System;

[DefaultExecutionOrder(-1)]
public class Simulation : SerializedMonoBehaviour
{ 
	public enum SpawnMode { Random, Point, InwardCircle, RandomCircle, Bitmap }

	const int updateKernel = 0;
	const int diffuseMapKernel = 1;
	const int colourKernel = 2;

	[Header("DATA")]
	[InlineEditor(InlineEditorModes.FullEditor)]
	[System.NonSerialized] [OdinSerialize] public ConnectionParams connectionParams;

	[Header("SETTINGS")]
	[InlineEditor(InlineEditorModes.FullEditor)]
	[System.NonSerialized] [OdinSerialize] public SlimeSettings settings;

	[Header("MAPPINGS")]
	public bool activateMappings = false;
	public List<IMapping> Mappings;

	[Header("SHADERS")]
	public ComputeShader compute;
	public ComputeShader drawAgentsCS;

	[Header("DISPLAY")]
	public bool showAgentsOnly;

	public FilterMode filterMode = FilterMode.Point;
	public GraphicsFormat format = ComputeHelper.defaultGraphicsFormat;


	[Header("COLORS")]

	public bool changeLogoColorWithMood = true;
	[SerializeField] private Color positiveColor = new Color(0, 1, 0, 1);
	[SerializeField] private Color neutralColor = new Color(1, 1, 1, 1);
	[SerializeField] private Color negativeColor = new Color(1, 0, 0, 1);

	public void ShowAgentsOnly()
    {
		showAgentsOnly = true;
    }
	public void ShowTrails()
	{
		showAgentsOnly = false;
	}

	[Header("SPAWN SHAPE (Optional)")]
	public Texture2D spawnBitmap;


	[SerializeField, HideInInspector] protected RenderTexture trailMap;
	[SerializeField, HideInInspector] protected RenderTexture diffusedTrailMap;
	[SerializeField, HideInInspector] protected RenderTexture displayTexture;

	ComputeBuffer agentBuffer;
	ComputeBuffer settingsBuffer;

	bool isSimActive = false;

	protected virtual void Start()
	{
		Wenzil.Console.Console.Log($"SLIME SIM STARTING");
		PreActivateSim();
	}

	void PreActivateSim()
    {
		transform.GetComponentInChildren<MeshRenderer>().material.mainTexture = displayTexture;
		Init();
	}

	void Init()
	{
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
						Wenzil.Console.Console.LogError("Attempting to spawn in Bitmap mode but no Bitmap provided in settings");
						return;
					}

					int random_number = Random.Range(1, positions.Count);
					startPos = Scalepoint(positions[random_number], spawnBitmap);
					angle = randomAngle;
				}
				catch (System.Exception ex)
				{
					Wenzil.Console.Console.LogError($"Error reading bitmap image for spawn: {ex}");
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
				int species = Random.Range(1, numSpecies + 1);
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

	public void ActivateSim()
    {
		Wenzil.Console.Console.Log("ACTIVATING SLIME");
		isSimActive = true;
		SlowUpdate();
	}




	[GUIColor(0, 1, 0)]
	[Button(ButtonSizes.Large)]
	[ButtonGroup("SimActivationButtons")]
	public void ActivateSimButton()
	{
		ActivateSim();
	}


	[GUIColor(1, 0, 0)]
	[Button(ButtonSizes.Large)]
	[ButtonGroup("SimActivationButtons")]
	public void DeactivateSim()
    {
		isSimActive = false;
		PreActivateSim();
    }

	

	void FixedUpdate()
	{
		if (isSimActive)
        {
			for (int i = 0; i < settings.stepsPerFrame; i++)
			{
				RunSimulation();
			}
		}
		
	}

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


	/// <summary>
	/// slow update used for parameter mapping
	/// </summary>
	async private void SlowUpdate()
    {
		if (isSimActive)
        {
			if (changeLogoColorWithMood)
            {
				CheckMoodColor();
			}
			if (activateMappings)
            {
				ApplyMappings();
				await UniTask.Delay(100);
				SlowUpdate();
			}
		}
    }


	void CheckMoodColor()
    {
		if (isSimActive)
        {
			Mood currentMood = connectionParams.FetchMood();

			if (settings.speciesSettings.Length >= 1)
			{
				switch (currentMood)
				{
					case Mood.POSITIVE:
						settings.speciesSettings[0].colour = positiveColor;
						break;
					case Mood.NEUTRAL:
						settings.speciesSettings[0].colour = neutralColor;
						break;
					case Mood.NEGATIVE:
						settings.speciesSettings[0].colour = negativeColor;
						break;
				}
			}
		}
    }
	void ApplyMappings()
    {
        foreach (IMapping mapping in Mappings)
        {
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
}
