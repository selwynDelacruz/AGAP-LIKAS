using System;
using UnityEngine;

public class Waves : MonoBehaviour
{

    public int Dimensions = 0;
    public float UVScale;
    public Octave[] Octaves;

    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Mesh Setup
        Mesh = new Mesh();
        Mesh.name = gameObject.name;

        Mesh.vertices = GenerateVerts(); //we specify the vertices of the mesh
        Mesh.triangles = GenerateTries(); //we specify the triangles of the mesh
        Mesh.uv = GenerateUVs(); //we specify the UVs of the mesh
        Mesh.RecalculateBounds(); //we recalculate the bounds of the mesh
        Mesh.RecalculateNormals(); //we recalculate the normals of the mesh for correct lighting

        MeshFilter = gameObject.AddComponent<MeshFilter>(); //we add a MeshFilter component to the game object to render
        MeshFilter.mesh = Mesh; //we assign the mesh to the MeshFilter component
    }

    public float GetHeight(Vector3 position)
    {
        //scale factor and position in local space
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((position - transform.position), scale);

        //get edge points
        var p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        //clamp if the position is outside the plane
        p1.x = Mathf.Clamp(p1.x, 0, Dimensions);
        p1.z = Mathf.Clamp(p1.z, 0, Dimensions);
        p2.x = Mathf.Clamp(p2.x, 0, Dimensions);
        p2.z = Mathf.Clamp(p2.z, 0, Dimensions);
        p3.x = Mathf.Clamp(p3.x, 0, Dimensions);
        p3.z = Mathf.Clamp(p3.z, 0, Dimensions);
        p4.x = Mathf.Clamp(p4.x, 0, Dimensions);
        p4.z = Mathf.Clamp(p4.z, 0, Dimensions);

        //get the max distance to one of the edges and use it to compute max distance
        var max = Mathf.Max(Vector3.Distance(p1, localPos), Vector3.Distance(p2, localPos), Vector3.Distance(p3, localPos), Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        var dist = (max - Vector3.Distance(p1, localPos))
                 + (max - Vector3.Distance(p2, localPos))
                 + (max - Vector3.Distance(p3, localPos))
                 + (max - Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        //weighted sum
        var height = Mesh.vertices[index((int)p1.x, (int)p1.z)].y * (max - Vector3.Distance(p1, localPos))
                   + Mesh.vertices[index((int)p2.x, (int)p2.z)].y * (max - Vector3.Distance(p2, localPos))
                   + Mesh.vertices[index((int)p3.x, (int)p3.z)].y * (max - Vector3.Distance(p3, localPos))
                   + Mesh.vertices[index((int)p4.x, (int)p4.z)].y * (max - Vector3.Distance(p4, localPos));

        //scale
        return height * transform.lossyScale.y / dist;
    }

    private Vector3[] GenerateVerts()
    {
        var verts = new Vector3[(Dimensions + 1) * (Dimensions + 1)];

        //equally space the vertices in a grid
        for (int x = 0; x <= Dimensions; x++)
            for (int z = 0; z <= Dimensions; z++)
                verts[index(x, z)] = new Vector3(x, 0, z);
            
        return verts;
    }

    private int index(int x, int z)
    {
        return x * (Dimensions + 1) + z;
    }

    private int[] GenerateTries()
    {
        var tries = new int[Mesh.vertices.Length * 6];

        //create two triangles for each square in the grid
        for (int x = 0; x < Dimensions; x++)
        {
            for (int z = 0; z < Dimensions; z++)
            {
                tries[index(x, z) * 6 + 0] = index(x, z);
                tries[index(x, z) * 6 + 1] = index(x + 1, z + 1);
                tries[index(x, z) * 6 + 2] = index(x + 1, z);
                tries[index(x, z) * 6 + 3] = index(x, z);
                tries[index(x, z) * 6 + 4] = index(x, z + 1);
                tries[index(x, z) * 6 + 5] = index(x + 1, z + 1);
            }
        }
        return tries;
    }

    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[Mesh.vertices.Length];

        //always set one uv over n tiles than flip the uv and set it again
        for (int x = 0; x <= Dimensions; x++)
        {
            for (int z = 0; z <= Dimensions; z++)
            {
                var vec = new Vector2((x / UVScale) % 2, (z / UVScale) % 2);
                uvs[index(x, z)] = new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y);
            }
        }

        return uvs;
    }

    // Update is called once per frame
    void Update()
    {
        var verts = Mesh.vertices; //we get the vertices of the mesh
        for (int x = 0; x <= Dimensions; x++)
        {
            for (int z = 0; z <= Dimensions; z++)
            {
                var y = 0f;
                for (int o = 0; o < Octaves.Length; o++)
                {
                    if (Octaves[o].alternate)
                    {
                        var perl = Mathf.PerlinNoise((x * Octaves[o].Scale.x) / Dimensions, (z * Octaves[o].Scale.y) / Dimensions) * Mathf.PI * 2f;
                        y += Mathf.Cos(perl + Octaves[o].Speed.magnitude * Time.time) * Octaves[o].Height;
                    }
                    else
                    {
                        var perl = Mathf.PerlinNoise((x * Octaves[o].Scale.x + Time.time * Octaves[o].Speed.x) / Dimensions, (z * Octaves[o].Scale.y + Time.time * Octaves[o].Speed.y) / Dimensions) - 0.5f;
                        y += perl * Octaves[o].Height;
                    }
                }
                verts[index(x, z)] = new Vector3(x, y, z);
            }
        }

        Mesh.vertices = verts; //we assign the modified vertices back to the mesh
        Mesh.RecalculateNormals(); //we recalculate the normals of the mesh for correct lighting
    }

    [Serializable]
    public struct Octave
    {
        public Vector2 Speed;
        public Vector2 Scale;
        public float Height;
        public bool alternate;
    }
}
