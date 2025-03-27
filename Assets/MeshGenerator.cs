using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{

    Mesh mesh;
    Vector3[] vertices;
    List<Vector3> vertices2;
    int[] triangles;
    List<int> triangles2;

    public int xheight = 20;
    public int zwidth = 50;

    public int resolution = 1;
    public float noiseScale = 0.1f;

    private static int [] permutation;

    public int numOctaves = 3;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        permutation = GeneratePermutation();
        
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
                float y = FractalBrownianMotion((float)x * noiseScale, (float)z * noiseScale, numOctaves);
                //float y = CreatePerlinNoise((float)x * noiseScale, (float)z * noiseScale);
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

    void UpdateMesh(){
        mesh.Clear();
        mesh.vertices = vertices2.ToArray();
        mesh.triangles = triangles2.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();
    }

    private static Vector2 GetConstantVector(int v)
    {
        // v is the value from the permutation table
        int h = v & 3;
        if (h == 0)
            return new Vector2(1.0f, 1.0f);
        else if (h == 1)
            return new Vector2(-1.0f, 1.0f);
        else if (h == 2)
            return new Vector2(-1.0f, -1.0f);
        else
            return new Vector2(1.0f, -1.0f);
    }

    private static float Fade(float t)
    {
        return ((6 * t - 15) * t + 10) * t * t * t;
    }

    private static float Lerp(float t, float a1, float a2)
    {
        return a1 + t * (a2 - a1);
    }

    public static float CreatePerlinNoise(float x, float y)
    {
        int X = Mathf.FloorToInt(x) & 255;
        int Y = Mathf.FloorToInt(y) & 255;

        float xf = x - Mathf.Floor(x);
        float yf = y - Mathf.Floor(y);

        Vector2 topRight = new Vector2(xf - 1.0f, yf - 1.0f);
        Vector2 topLeft = new Vector2(xf, yf - 1.0f);
        Vector2 bottomRight = new Vector2(xf - 1.0f, yf);
        Vector2 bottomLeft = new Vector2(xf, yf);

        // Select a value from the permutation array for each of the 4 corners
        int valueTopRight = permutation[permutation[X + 1] + Y + 1];
        int valueTopLeft = permutation[permutation[X] + Y + 1];
        int valueBottomRight = permutation[permutation[X + 1] + Y];
        int valueBottomLeft = permutation[permutation[X] + Y];

        float dotTopRight = Vector2.Dot(topRight, GetConstantVector(valueTopRight));
        float dotTopLeft = Vector2.Dot(topLeft, GetConstantVector(valueTopLeft));
        float dotBottomRight = Vector2.Dot(bottomRight, GetConstantVector(valueBottomRight));
        float dotBottomLeft = Vector2.Dot(bottomLeft, GetConstantVector(valueBottomLeft));

        float u = Fade(xf);
        float v = Fade(yf);

        return Lerp(u,
            Lerp(v, dotBottomLeft, dotTopLeft),
            Lerp(v, dotBottomRight, dotTopRight)
        );
    }
        public static float FractalBrownianMotion(float x, float y, int numOctaves)
    {
        float result = 0.0f;
        float amplitude = 1.0f;
        float frequency = 0.005f;

        for (int octave = 0; octave < numOctaves; octave++)
        {
            float n = amplitude * CreatePerlinNoise(x * frequency, y * frequency);
            result += n;

            amplitude *= 0.5f;
            frequency *= 2.0f;
        }

        return result;
    }

    /*private void OnDrawGizmos(){

        if(vertices2 != null){
            for(int i = 0; i < vertices2.Count; i++){
                Gizmos.DrawSphere(vertices2[i], .1f);
            }
        }
    }*/

    int[] GeneratePermutation()
    {
        int[] p = new int[256];
        for (int i = 0; i < 256; i++) p[i] = i;

        // Fisher-Yates shuffle
        for (int i = 255; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (p[i], p[swapIndex]) = (p[swapIndex], p[i]); // Swap values
        }

        // Duplicate the array for seamless wrapping
        int[] perm = new int[512];
        for (int i = 0; i < 512; i++) perm[i] = p[i % 256];

        return perm;
    }

}
