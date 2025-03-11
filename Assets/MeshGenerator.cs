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

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        
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
                float y = Mathf.PerlinNoise(x * .3f, z * .3f);
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

        /*vertices = new Vector3[(xheight + 1) * (zwidth + 1)];

        for (int i = 0, z = 0; z <= zwidth; z++)
        {
            for(int x = 0; x <= xheight; x++)
            {
                //float y = Mathf.PerlinNoise(x * .3f, z * .3f);
                vertices[i] = new Vector3(x, 0,z);
                i++;
            }
        }

        triangles = new int[xheight * zwidth * 6];
        int vert = 0;
        int tris = 0;

        for(int z = 0; z < zwidth; z++){
            for(int x = 0; x < xheight; x++){
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xheight + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xheight + 1;
                triangles[tris + 5] = vert + xheight + 2;

                vert++;
                tris +=6;
            } 
            vert++;           
        }*/

    /*    vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1)
        };

        triangles = new int[] 
        { 
            0, 1, 2,
            1, 3, 2
        };*/
    }

    void UpdateMesh(){
        mesh.Clear();
        mesh.vertices = vertices2.ToArray();
        mesh.triangles = triangles2.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();
    }

/*    private void OnDrawGizmos(){

        if(vertices2 != null){
            for(int i = 0; i < vertices2.Count; i++){
                Gizmos.DrawSphere(vertices2[i], .1f);
            }
        }
    }*/
}
