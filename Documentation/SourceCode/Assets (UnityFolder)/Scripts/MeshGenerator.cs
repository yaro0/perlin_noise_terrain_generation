using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;
    public int seed = 1;
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        Noise.Initialize(seed);
    }

    public static MeshData GenerateMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float halfWidth = (width - 1) / 2f;
        float halfHeight = (height - 1) / 2f;

        int stepSize = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        MeshData meshData = new MeshData();
        meshData.setUVsSize(width, height);
        int vertexIndex = 0;
        List<List<int>> vertexGrid = new List<List<int>>();

            for (int z = 0; z < height; z += stepSize) {
                List<int> row = new List<int>();
                for (int x = 0; x < width; x += stepSize) {

                    float y = heightCurve.Evaluate(heightMap[x, z]) * heightMultiplier;
                    meshData.AddVertices(new Vector3(x - halfWidth, y, halfHeight - z));

                    row.Add(vertexIndex);
                    vertexIndex++;

                    if (x > 0 && z > 0) {
                        int topLeft = vertexGrid[vertexGrid.Count - 1][row.Count - 2];
                        int bottomLeft = vertexGrid[vertexGrid.Count - 1][row.Count - 1];
                        int topRight = row[row.Count - 2];
                        int bottomRight = row[row.Count - 1];
                        
                        meshData.AddTrianglePoint(topLeft);
                        meshData.AddTrianglePoint(bottomLeft);
                        meshData.AddTrianglePoint(bottomRight);

                        meshData.AddTrianglePoint(topLeft);
                        meshData.AddTrianglePoint(bottomRight);
                        meshData.AddTrianglePoint(topRight);
                    }
                }
                vertexGrid.Add(row);
        }

        return meshData;
    }

    void UpdateMesh(){
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
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