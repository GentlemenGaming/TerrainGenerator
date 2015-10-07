using UnityEngine;
using System.Collections;

public class TerrainArchetype : ScriptableObject {
	public string C_Name;
	public Terrain C_Terrain;
	public GameObject C_TerrainObj;
	public int C_tResolution = 32;
	public Vector2 C_Axes = new Vector2(0,0);
	public int C_tHeight = 50;
	public int C_tScale = 1;
	public Texture2D C_tMaterial;
	public double C_nFrequency = .003f, C_nLacunarity = .002f;
	public LibNoise.NoiseQuality C_nQuality = LibNoise.NoiseQuality.Standard;
	public int C_nOctaves = 1, C_nSeed = 555;
	public double C_nPersistence = 1.5f;
	public SetNoise[] C_nNoiseList = new SetNoise[3];
}
public enum SetNoise {
	
	Perlin = 0,
	Billow = 1,
	MultiFractal = 2
}