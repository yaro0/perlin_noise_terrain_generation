using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using TMPro;

public class MapGenerator : MonoBehaviour {

    [Range(0,6)]
    public int lod;
    const int mapChunkSize = 241;
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
    public int erosionNumParticles = 1000;


    [Header("Buttons")]
    public Slider lodSlider;
    public Slider noiseScaleSlider;
    public TMP_InputField octavesField;
    public Slider persistenceSlider;
    public TMP_InputField lacunarityField;
    public TMP_InputField seedField;
    public TMP_InputField meshHeightMultiplierField;
    public Toggle autoUpdateToggle;
    public Button generateButton;
    public Toggle materialToggle;
    public MeshRenderer meshRenderer;
    public TMP_InputField erosionNumParticlesInputField;

    [Header("Materials")]
    public Material defaultMaterial; 
    public Material materialWithTriangles; 

    [Header("Game Objects")]
    public GameObject meshObject;
    public GameObject planeObject;

    private bool isMeshActive = true;

    private static float[,] noiseMap;

	public void GenerateMap() {
		noiseMap = Noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, noiseScale, seed, octaves, persistance, lacunarity);

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		display.DrawNoiseMap (noiseMap);
        display.DrawMesh(MeshGenerator.GenerateMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, lod));
	}

    public void Erode() {

        if(noiseMap == null)
            return;

        if(!string.IsNullOrEmpty(erosionNumParticlesInputField.text) && int.Parse(erosionNumParticlesInputField.text) > 0 && int.Parse(erosionNumParticlesInputField.text) < 100000)
        {
            erosionNumParticles =  int.Parse(erosionNumParticlesInputField.text);
        }
        else
        {
            erosionNumParticlesInputField.text = "10000";
            erosionNumParticles = 10000;
        }

		noiseMap = Erosion.ApplyErosion(noiseMap, erosionNumParticles);
        
		MapDisplay display = FindObjectOfType<MapDisplay> ();
		display.DrawNoiseMap (noiseMap);
        display.DrawMesh(MeshGenerator.GenerateMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, lod));
	}

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