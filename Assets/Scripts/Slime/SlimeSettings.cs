using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu()]
[System.Serializable]
public class SlimeSettings : ScriptableObject
{
	[Header("Display")]
	public int width = 1280;
	public int height = 720;

	[Header("Simulation Settings")]
	[Range(1, 5)] public int stepsPerFrame = 1;
	[Range(100000, 500000)] public int numAgents = 250000;
	public Simulation.SpawnMode spawnMode = Simulation.SpawnMode.InwardCircle;

	// You can also control spacing both before and after the PropertySpace attribute.
	[PropertySpace(SpaceBefore = 20)]
	// You can also control spacing both before and after the PropertySpace attribute.
	[Header("Trail Settings")]
	[Range(0, 100f)] public float trailWeight = 1f;
	[Range(0.1f,10f)] public float decayRate = 1f;
	[Range(0, 100f)] public float diffuseRate = 1f;

	// You can also control spacing both before and after the PropertySpace attribute.
	[PropertySpace(SpaceBefore = 20)]
	public SpeciesSettings[] speciesSettings;

	[System.Serializable]
	public struct SpeciesSettings
	{
		[Header("Movement Settings")]
		[Range(0, 100)] public float moveSpeed;
		[Range(-15, 15)] public float turnSpeed;

		[Header("Sensor Settings")]
		[Range(0, 100)] public float sensorAngleSpacing;
		[Range(-75, 75)] public float sensorOffsetDst;
		[Range(0, 10)] public int sensorSize;

		[Header("Display settings")]
		public Color colour;
	}
}
