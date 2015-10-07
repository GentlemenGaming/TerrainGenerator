using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LibNoise;

public class TerrainEditor : EditorWindow {

	// Window Variable
	private static TerrainEditor window;
	
	// Archetype Array/List
	private List<TerrainArchetype> TerrainsList;
	private TerrainArchetype[] Terrains;
	
	// Archetype Input Variables
	private string[] AT_Names;
	private List<string> AT_NamesList;
	
	private string AT_Name = "Enter Title Here";
	
	private int AT_Current;
	private int AT_Selected;

	// Master File Reference
	private Master Master;

	private LibNoise.IModule noise, noise1, noise2, noiseCombine, tempnoise;
	LibNoise.Modifiers.Select selector;

	private Vector3 mousePosition;

	// Set up Menu Item
	[MenuItem ("Window/TerrainGenerator")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		window = (TerrainEditor)EditorWindow.GetWindowWithRect (typeof (TerrainEditor),new Rect(0,0,300,520),false,"Terrain Generator");
		window.Show();
	}

	void Start (){
		if(Terrains.Length > 0){
			for(int i = 0;i < Terrains.Length;i++){
				if(Terrains[i].C_Terrain == null && Terrains[i].C_TerrainObj != null){
					Terrains[i].C_Terrain = Terrains[i].C_TerrainObj.GetComponent<Terrain>();
				}
			}
		}
	}

	// GUI
	void OnGUI () {
		// City Archetype: |New| |Enter Title Here|
		EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
		EditorGUILayout.LabelField ("Terrain Archetype:",GUILayout.Width(120));
		if(Terrains.Length > 0){
			AT_Current = EditorGUILayout.Popup(AT_Current,AT_Names,GUILayout.Width(60));
		}
		if(Terrains.Length == 0 || AT_Current == AT_Names.Length - 1){
			AT_Name = EditorGUILayout.TextField (AT_Name,GUILayout.Width(100));
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();EditorGUILayout.Space();

		// |Add| |Remove| |Select|
		EditorGUILayout.BeginHorizontal();
		// Add Terrain by Name
		if(AT_Current == AT_Names.Length - 1){
			if(GUILayout.Button("Add")){
				if(AT_Name != "Enter Title Here" && AT_Name != "New"){
					TerrainsList = Terrains.ToList();
					TerrainsList.Add(CreateInstance<TerrainArchetype>());
					TerrainsList.Last().C_Name = AT_Name;
					Terrains = TerrainsList.ToArray();
					
					AT_NamesList = AT_Names.ToList();
					AT_NamesList.RemoveAt(AT_NamesList.Count - 1);
					AT_NamesList.Add(AT_Name);
					AT_NamesList.Add("New");
					AT_Names = AT_NamesList.ToArray();

					AT_Current = AT_Names.Length - 2;
					if(AT_Names.Length == 2 || AT_Selected >= AT_Names.Length){
						AT_Selected = 0;
					}

					AssetDatabase.CreateFolder ("Assets/TerrainGenerator/Resources/Archetypes",AT_Names[AT_Current]);
					AssetDatabase.CreateAsset (Terrains[AT_Current],"Assets/TerrainGenerator/Resources/Archetypes/" + AT_Names[AT_Current] + "/" + AT_Names[AT_Current] + ".asset");

					SaveMasters (AT_Current);
				}
			}
		}

		EditorGUILayout.Space();
		if(Terrains.Length > 0){
			if(AT_Selected != AT_Names.Length - 1){
				if(GUILayout.Button("Delete " + Terrains[AT_Selected].C_Name)){
					if(Terrains[AT_Selected].C_Terrain != null){
						DestroyImmediate (Terrains[AT_Selected].C_Terrain.gameObject);
					}

					TerrainsList = Terrains.ToList();
					TerrainsList.RemoveAt(AT_Selected);
					Terrains = TerrainsList.ToArray();
					
					AssetDatabase.DeleteAsset("Assets/TerrainGenerator/Resources/Archetypes/" + AT_Names[AT_Selected]);
					AT_NamesList = AT_Names.ToList();
					AT_NamesList.RemoveAt(AT_Selected);
					AT_Names = AT_NamesList.ToArray();
					SaveMasters ();
					if(AT_Selected != AT_Current){
						AT_Selected = AT_Current;
					}
					//AT_Drawing = false;

				}
				EditorGUILayout.Space();
			}
		}
		if(Terrains.Length > 0){
			if(AT_Current != AT_Names.Length - 1){
				if(GUILayout.Button("Select " + Terrains[AT_Current].C_Name)){
					// Load the Archetype panel
					AT_Selected = AT_Current;
				}
			}
			EditorGUILayout.Space();

			if(GUILayout.Button("Save " + Terrains[AT_Selected].C_Name)){
				// Load the Archetype panel
				SaveMasters (AT_Selected);
			}
		}
		EditorGUILayout.Space ();
		EditorGUILayout.EndHorizontal();

		if(Terrains.Length > 0 && AT_Selected != AT_Names.Length - 1){
			// Full Panel Vertical Call
			EditorGUILayout.BeginVertical("Box");			
			EditorGUILayout.LabelField(AT_Names[AT_Selected]);
			// Full Panel Horizontal Call
			EditorGUILayout.BeginHorizontal();

			// Preset Panel Vertical Call
			EditorGUILayout.BeginVertical("Box", GUILayout.Width(90));

			// Noise
			EditorGUILayout.LabelField("Noise Settings");

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Quality");
			Terrains[AT_Selected].C_nQuality = (LibNoise.NoiseQuality)EditorGUILayout.EnumPopup(Terrains[AT_Selected].C_nQuality);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Combine:");

			Terrains[AT_Selected].C_nNoiseList[0] = (SetNoise)EditorGUILayout.EnumPopup(Terrains[AT_Selected].C_nNoiseList[0]);
			Terrains[AT_Selected].C_nNoiseList[1] = (SetNoise)EditorGUILayout.EnumPopup(Terrains[AT_Selected].C_nNoiseList[1]);
			Terrains[AT_Selected].C_nNoiseList[2] = (SetNoise)EditorGUILayout.EnumPopup(Terrains[AT_Selected].C_nNoiseList[2]);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Seed");
			Terrains[AT_Selected].C_nSeed = EditorGUILayout.IntField(Terrains[AT_Selected].C_nSeed);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Octaves");
			Terrains[AT_Selected].C_nOctaves = EditorGUILayout.IntField(Terrains[AT_Selected].C_nOctaves);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Frequency");
			Terrains[AT_Selected].C_nFrequency = EditorGUILayout.DoubleField(Terrains[AT_Selected].C_nFrequency);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Persistence");
			Terrains[AT_Selected].C_nPersistence = EditorGUILayout.DoubleField(Terrains[AT_Selected].C_nPersistence);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Lacunarity");
			Terrains[AT_Selected].C_nLacunarity = EditorGUILayout.DoubleField(Terrains[AT_Selected].C_nLacunarity);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("__________________");
			// Terrain

			EditorGUILayout.LabelField("Terrain Settings");

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Terrain Prefab");
			Terrains[AT_Selected].C_TerrainObj = (GameObject)EditorGUILayout.ObjectField(Terrains[AT_Selected].C_TerrainObj, typeof(Terrain), true);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Texture");
			Terrains[AT_Selected].C_tMaterial = (Texture2D)EditorGUILayout.ObjectField(Terrains[AT_Selected].C_tMaterial, typeof(Texture2D), true);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Cubic Size:");
			Terrains[AT_Selected].C_tResolution = EditorGUILayout.IntField(Terrains[AT_Selected].C_tResolution);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			Terrains[AT_Selected].C_Axes = EditorGUILayout.Vector2Field("Axes:",Terrains[AT_Selected].C_Axes);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Height:");
			Terrains[AT_Selected].C_tHeight = EditorGUILayout.IntField(Terrains[AT_Selected].C_tHeight);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			EditorGUILayout.LabelField("Scale:");
			Terrains[AT_Selected].C_tScale = EditorGUILayout.IntField(Terrains[AT_Selected].C_tScale);
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
			if(GUILayout.Button("Generate")){				

				if(Terrains[AT_Selected].C_Terrain == null){

					Terrains[AT_Selected].C_Terrain = new Terrain();
					TerrainData DataBuffer = new TerrainData();
					GameObject NewTerrain = Terrain.CreateTerrainGameObject(DataBuffer);
					Undo.RegisterCreatedObjectUndo(NewTerrain,"Generate");
					Terrains[AT_Selected].C_Terrain = NewTerrain.GetComponent<Terrain>();
					AssetDatabase.CreateAsset(Terrains[AT_Selected].C_Terrain.terrainData,"Assets/TerrainGenerator/Resources/Archetypes/" + Terrains[AT_Selected].C_Name + "/TerrainData.asset");

					return;
				} else {
					Undo.RecordObject(Terrains[AT_Selected].C_Terrain, "Generate");
				}

				GenerateTerrain(Terrains[AT_Selected].C_Terrain,Terrains[AT_Selected]);					

			}
			// End of Left Panel
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			// End of Full Panel
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
	}

	public LibNoise.IModule GenerateNoise(TerrainArchetype curTerrain, LibNoise.IModule noiseCombine){
		LibNoise.Perlin Temp = new LibNoise.Perlin();
		tempnoise = Temp;noise = Temp;noise1 = Temp;noise2 = Temp;noiseCombine = Temp;
		// Apply Noise Values
		for(int i = 0; i < curTerrain.C_nNoiseList.Length;i++){			
			switch(curTerrain.C_nNoiseList[i]) {
			case SetNoise.Perlin:
				LibNoise.Perlin perlin = new LibNoise.Perlin();
				perlin.Frequency = curTerrain.C_nFrequency;
				perlin.Lacunarity = curTerrain.C_nLacunarity;
				perlin.NoiseQuality = curTerrain.C_nQuality;
				perlin.OctaveCount = curTerrain.C_nOctaves;
				perlin.Persistence = curTerrain.C_nPersistence;
				perlin.Seed = curTerrain.C_nSeed;
				tempnoise = perlin;
				break;
			case SetNoise.Billow:
				LibNoise.Billow billow = new LibNoise.Billow();
				billow.Frequency = curTerrain.C_nFrequency;
				billow.Lacunarity = curTerrain.C_nLacunarity;
				billow.NoiseQuality = curTerrain.C_nQuality;
				billow.OctaveCount = curTerrain.C_nOctaves;
				billow.Persistence = curTerrain.C_nPersistence;
				billow.Seed = curTerrain.C_nSeed;
				tempnoise = billow;
				break;
			case SetNoise.MultiFractal:
				LibNoise.RidgedMultifractal MultiFrac = new LibNoise.RidgedMultifractal();
				MultiFrac.Frequency = curTerrain.C_nFrequency;
				MultiFrac.Lacunarity = curTerrain.C_nLacunarity;
				MultiFrac.NoiseQuality = curTerrain.C_nQuality;
				MultiFrac.OctaveCount = curTerrain.C_nOctaves;
				//MultiFrac.Persistence = Persistence;
				MultiFrac.Seed = curTerrain.C_nSeed;
				tempnoise = MultiFrac;
				break;
			}
			switch(i){
			case 0:
				noise = tempnoise;
				break;
			case 1:
				noise1 = tempnoise;
				break;
			case 2:
				noise2 = tempnoise;
				break;
			}
		}

		//selector = new LibNoise.Modifiers.Select(noise,noise1,noise2);
		LibNoise.IModule tempNoise1 = new LibNoise.Modifiers.Add(noise,noise1);		
		LibNoise.IModule tempNoise2 = new LibNoise.Modifiers.Add(tempNoise1,noise2);
		noiseCombine = tempNoise2;
		return noiseCombine;
	}

	public void GenerateTerrain(Terrain curTerrain, TerrainArchetype Terrain) {
		LibNoise.Perlin Temp = new LibNoise.Perlin();
		noiseCombine = Temp;
		noiseCombine = GenerateNoise (Terrain, noiseCombine);
		// Procedural Generate a terrain heightmap and apply it to a terrain on a gameobject	
		TerrainData tdata = Terrain.C_Terrain.terrainData;
		tdata.heightmapResolution = Terrain.C_tResolution * Terrain.C_tResolution;
		int xRes = tdata.heightmapWidth;
		int yRes = tdata.heightmapHeight;
		GameObject terrainObj = curTerrain.gameObject;

		tdata.size = new Vector3(xRes * Terrain.C_tScale,Terrain.C_tHeight * Terrain.C_tScale,yRes * Terrain.C_tScale);
		
		float[,] tHeights = tdata.GetHeights(0,0,xRes,yRes);
		
		int fillx;
		int filly;
		int Startx = System.Convert.ToInt32 (Terrain.C_Axes.x);
		int Startz = System.Convert.ToInt32 (Terrain.C_Axes.y);

		

		for (int x = Startx;x < (xRes + Startx);x++){
			for(int z = Startz;z < (yRes + Startz);z++){
				
				// Set up fill numbers to compensate for negative shifts.
				fillx = x - Startx;				
				filly = z - Startz;

				double Value = noiseCombine.GetValue(x, z, 0);

				if(Value < 0){
					double tempvalue = (Value * -1);
					tHeights[fillx,filly] = System.Convert.ToSingle(tempvalue);

				}

			}
		}

		if(Terrain.C_tMaterial != null){
			SplatPrototype[] splatData = new SplatPrototype[1];
			splatData[0] = new SplatPrototype();
			splatData[0].texture = Terrain.C_tMaterial;
			splatData[0].tileOffset = new Vector2(0, 0);
			splatData[0].tileSize = new Vector2(15, 15);
			
			tdata.splatPrototypes = splatData;
		}
		curTerrain.terrainData = tdata;
		curTerrain.gameObject.GetComponent<TerrainCollider>().terrainData = tdata;
		
		curTerrain.terrainData.SetHeights(0,0,tHeights);
		terrainObj.transform.position = new Vector3(Terrain.C_Axes.x,0,Terrain.C_Axes.y);

		Terrain.C_TerrainObj = PrefabUtility.CreatePrefab("Assets/TerrainGenerator/Resources/Archetypes/" + Terrain.C_Name + "/Terrain.prefab",terrainObj);
	}

	// Update is called once per frame
	void Update () {
		if(Master == null){
			Init ();
		}


	}

	// When the window starts
	void OnEnable () {
		
		AT_Selected = EditorPrefs.GetInt("AT_Selected",AT_Selected);
		// If we have no Master File
		if(Resources.Load ("Archetypes/Master") == null){
			Master = CreateInstance<Master>();
			// Set up a Terrain List
			TerrainsList = new List<TerrainArchetype>();
			Terrains = TerrainsList.ToArray();
			Master.Terrains = Terrains;
			// And create one
			AssetDatabase.CreateAsset(Master,"Assets/TerrainGenerator/Resources/Archetypes/Master.asset");

			// Then set up a Name List			
			AT_NamesList = new List<string>();
			AT_NamesList.Add ("New");
			AT_Names = AT_NamesList.ToArray();
		} else {
			// Otherwise Load it to the Master Reference
			Master = (Master)Resources.Load ("Archetypes/Master");
			// Load the Terrains
			Terrains = Master.Terrains;
			// Retrieve their Names
			AT_NamesList = new List<string>();
			for(int i = 0;i < Terrains.Length;i++){
				AT_NamesList.Add(Terrains[i].C_Name);
			}
			AT_NamesList.Add ("New");
			AT_Names = AT_NamesList.ToArray();
		}
	}
	
	void OnDisable () {
		Save ();
	}

	// Save Functions
	void Save (){
		Master.Terrains = Terrains;
		EditorUtility.SetDirty(Master);
		for(int i = 0; i < Master.Terrains.Length;i++){
			EditorUtility.SetDirty(Master.Terrains[i]);
		}
		
		AssetDatabase.SaveAssets();
		EditorPrefs.SetInt("AT_Selected",AT_Selected);
	}

	void SaveMasters (){
		Master.Terrains = Terrains;
		EditorUtility.SetDirty(Master);
		
		AssetDatabase.SaveAssets();
		EditorPrefs.SetInt("AT_Selected",AT_Selected);
	}

	void SaveMasters (int Current){
		Master.Terrains = Terrains;
		EditorUtility.SetDirty(Master);
		
		EditorUtility.SetDirty(Master.Terrains[Current]);
		
		AssetDatabase.SaveAssets();
		EditorPrefs.SetInt("AT_Selected",AT_Selected);
	}
}


