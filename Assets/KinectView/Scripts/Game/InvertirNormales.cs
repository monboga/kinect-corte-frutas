using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertirNormales : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            Mesh mesh = filter.mesh;

            // Invertir las normales
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i];
            }
            mesh.normals = normals;

            // Invertir los triángulos (las caras)
            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                int[] triangles = mesh.GetTriangles(m);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.SetTriangles(triangles, m);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
