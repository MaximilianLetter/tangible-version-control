using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryDifferences : MonoBehaviour
{
    public MeshFilter meshF1, meshF2;
    private Mesh mesh1, mesh2;
    private Mesh newMesh;

    // Start is called before the first frame update
    void Start()
    {
        newMesh = new Mesh();

        mesh1 = meshF1.mesh;
        mesh2 = meshF2.mesh;

        Vector3[] vertices1 = mesh1.vertices;
        Vector3[] normals1 = mesh1.normals;

        Vector3[] vertices2 = mesh2.vertices;
        Vector3[] normals2 = mesh2.normals;

        List<Vector3> commonVertices = new List<Vector3>();
        List<Vector3> commonNormals = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < vertices1.Length; i++)
        {
            var vert1 = vertices1[i];
            var norm1 = normals1[i];

            for (int j = 0; j < vertices2.Length; j++)
            {
                var vert2 = vertices2[j];
                var norm2 = normals2[j];

                if (vert1 == vert2 || norm1 == norm2)
                {
                    // Vertice & Normal are equal
                    commonVertices.Add(vert1);
                    commonNormals.Add(norm1);

                    // This is garbage code, just checking if anything happens
                    if (commonVertices.Count % 3 == 0)
                    {
                        triangles.Add(commonVertices.Count -3);
                        triangles.Add(commonVertices.Count -2);
                        triangles.Add(commonVertices.Count -1);
                    }

                    break;
                }
            }
        }

        newMesh.Clear();
        newMesh.vertices = commonVertices.ToArray();
        newMesh.normals = commonVertices.ToArray();
        newMesh.triangles = triangles.ToArray();

        GetComponent<MeshFilter>().mesh = newMesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
