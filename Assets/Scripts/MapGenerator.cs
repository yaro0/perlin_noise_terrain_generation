using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using TMPro;

public class MapGenerator : MonoBehaviour {

    [Range(0,6)]
    public int lod;
    const int mapChunkSize = 241;
	//public int mapChunkSize;
	//public int mapChunkSize;
	public float noiseScale;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
    public float meshHeightMultiplier = 1;
    public AnimationCurve meshHeightCurve;
	public Vector2 offset;

	public bool autoUpdate;


    //Buttons
    public Slider lodSlider;
    public Slider noiseScaleSlider;
    public TMP_InputField octavesField;
    public Slider persistenceSlider;
    public TMP_InputField lacunarityField;
    public TMP_InputField seedField;
    public TMP_InputField meshHeightMultiplierField;
    public Toggle autoUpdateToggle;
    public Button generateButton;
    public Toggle materialToggle;  // Reference to the Toggle UI element
    public MeshRenderer meshRenderer;  // Reference to the MeshRenderer of the object


    public Material defaultMaterial;  // First material
    public Material materialWithTriangles; 
    public GameObject meshObject;  // Assign your mesh object in the Inspector
    public GameObject planeObject; // Assign your plane object in the Inspector

    private bool isMeshActive = true;

    private static float[,] noiseMap;

	public void GenerateMap() {
		noiseMap = Noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, noiseScale, seed, octaves, persistance, lacunarity, offset);
        

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		display.DrawNoiseMap (noiseMap);
        display.DrawMesh(MeshGenerator.GenerateMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, lod));
	}

    public void Erode() {

        if(noiseMap == null)
            return;

		noiseMap = Erosion.ApplyErosion(noiseMap);
        
		MapDisplay display = FindObjectOfType<MapDisplay> ();
		display.DrawNoiseMap (noiseMap);
        display.DrawMesh(MeshGenerator.GenerateMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, lod));
	}

    	/*void OnValidate() {
		if (mapChunkSize < 1) {
			mapChunkSize = 1;
		}
		if (mapChunkSize < 1) {
			mapChunkSize = 1;
		}
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}*/

    void Start()
    {
        generateButton.onClick.AddListener(OnGenerateButtonClick);

    }
    void OnGenerateButtonClick()
    {
        lod = (int)lodSlider.value; 
        noiseScale = (float)noiseScaleSlider.value;;
        octaves = (string.IsNullOrEmpty(octavesField.text)) ? octaves : int.Parse(octavesField.text);
        persistance = (float)persistenceSlider.value;
        lacunarity = (string.IsNullOrEmpty(lacunarityField.text)) ? lacunarity : float.Parse(lacunarityField.text);
        seed = (string.IsNullOrEmpty(seedField.text)) ? seed : int.Parse(seedField.text);
        meshHeightMultiplier = (string.IsNullOrEmpty(meshHeightMultiplierField.text)) ? meshHeightMultiplier : float.Parse(meshHeightMultiplierField.text);
        autoUpdate = autoUpdateToggle.isOn; 
        GenerateMap();
    }

    public void ToggleObjects()
    {
        isMeshActive = !isMeshActive;

        meshObject.SetActive(isMeshActive);
        planeObject.SetActive(!isMeshActive);
    }

public void SwitchMaterial()
    {
        if (meshRenderer != null)
        {
            meshRenderer.sharedMaterial  = (meshRenderer.sharedMaterial  == defaultMaterial) ? materialWithTriangles : defaultMaterial;
        }
        else
        {
            Debug.LogWarning("MeshRenderer not assigned.");
        }
    }
	
}