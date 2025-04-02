using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    //Vector3[] vertices;
    List<Vector3> vertices2;
    //int[] triangles;
    List<int> triangles2;

    public int xheight = 20;
    public int zwidth = 50;

    public int resolution = 1;
    public float noiseScale = 0.1f;

    private static int [] permutation;
    public int seed = 1;
    public int numOctaves = 3;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        Noise.Initialize(seed);
    }

    void Update(){
        //resolution = Mathf.Clamp(resolution, 1, 50);
        CreateShape();
        UpdateMesh();
    }

    void CreateShape(){
        vertices2 = new List<Vector3>();
        float xPerStep = (float)xheight / resolution;
        float zPerStep = (float)zwidth / resolution;

        for (int z = 0; z < resolution + 1; z++)
        {
            for(int x = 0; x < resolution + 1; x++)
            {
                float y = Noise.FractalBrownianMotion((float)x * noiseScale, (float)z * noiseScale, numOctaves);
                vertices2.Add(new Vector3(x*xPerStep, y, z*zPerStep));
            }
        }
        
        triangles2 = new List<int>();
        for(int row = 0; row < resolution; row++){
            for(int column = 0; column < resolution; column++){
                int i = (row * resolution) + row + column;

                triangles2.Add(i);
                triangles2.Add(i + resolution + 1);
                triangles2.Add(i + resolution + 2);

                triangles2.Add(i);
                triangles2.Add(i + resolution + 2);
                triangles2.Add(i + 1);
            }          
        }
    }

    public static MeshData GenerateMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail){
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = ((float)heightMap.GetLength(0) - 1) / -2f;
        float topLeftZ = ((float)heightMap.GetLength(1) - 1) / 2f;

        int simplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int vericesPerLine = (width - 1)/ simplificationIncrement + 1;

        MeshData meshData = new MeshData();
        meshData.setUVsSize(width, height);

        for (int z = 0; z < height; z += simplificationIncrement)
        {
            for(int x = 0; x < width; x += simplificationIncrement)
            {
                meshData.AddVertices(new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x,z]) * heightMultiplier, topLeftZ - z));
                //meshData.uvs[meshData.Vertices.Count] = new Vector2(x/(float)width, z/(float)height);

                if(x < width - 1 && z < height - 1) //ignore right and bottom edge verices of the map
                {
                    meshData.AddTrianglePoint(meshData.Vertices.Count - 1);
                    meshData.AddTrianglePoint((meshData.Vertices.Count - 1) + vericesPerLine + 1);
                    meshData.AddTrianglePoint((meshData.Vertices.Count - 1) + vericesPerLine);

                    meshData.AddTrianglePoint((meshData.Vertices.Count - 1) + vericesPerLine + 1);
                    meshData.AddTrianglePoint(meshData.Vertices.Count - 1);
                    meshData.AddTrianglePoint((meshData.Vertices.Count- 1) + 1);
                }
            }
        }

        return meshData;
    }

    void UpdateMesh(){
        mesh.Clear();
        mesh.vertices = vertices2.ToArray();
        mesh.triangles = triangles2.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
}

public class MeshData
{
    public List<Vector3> Vertices { get; private set; }
    public List<int> Triangles { get; private set; }
    public Vector2[] uvs;

    public MeshData()
    {
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        uvs = new Vector2[1];
    }

    public Mesh CreateMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();
        //mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

    public void setUVsSize(int width, int height){
        uvs = new Vector2[width * height];
    }

    public void AddTrianglePoint(int x){
        Triangles.Add(x);
    }

    public void AddVertices(Vector3 vector){
        Vertices.Add(vector);
    }

}

/*
TO-DO:
- Optimize current perlin noise
- Maybe add simplex noise
- Add smoothing
- Add UI to control parameters
- add other stuff...
*/