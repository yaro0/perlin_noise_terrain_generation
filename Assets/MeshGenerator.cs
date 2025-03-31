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

    void UpdateMesh(){
        mesh.Clear();
        mesh.vertices = vertices2.ToArray();
        mesh.triangles = triangles2.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();
    }

    /*private void OnDrawGizmos(){

        if(vertices2 != null){
            for(int i = 0; i < vertices2.Count; i++){
                Gizmos.DrawSphere(vertices2[i], .1f);
            }
        }
    }*/

}

/*
TO-DO:
- Optimize current perlin noise
- Maybe add simplex noise
- Add smoothing
- Add UI to control parameters
- add other stuff...
*/