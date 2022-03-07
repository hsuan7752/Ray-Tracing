using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cone : MonoBehaviour
{
    Mesh mesh;
    MeshRenderer meshRenderer;
    public Material material;

    public float height = 3.0f;
    public float radius = 5.0f;
    public int segment = 36;

    Vector3 pos;

    float angle = 0.0f;
    float angleAmount = 0.0f;

    List<Vector3> vertices = new List<Vector3>();    
    List<int> triangles = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        angleAmount = 2 * Mathf.PI / segment;

        pos = new Vector3(0.0f, height, 0.0f);
        vertices.Add(pos);

        pos = new Vector3(0.0f, 0.0f, 0.0f);
        vertices.Add(pos);

        for (int i = 0; i < segment; ++i)
        {
            pos.x = radius * Mathf.Sin(angle);
            pos.z = radius * Mathf.Cos(angle);

            vertices.Add(pos);

            angle -= angleAmount;
        }

        mesh.vertices = vertices.ToArray();

        Debug.Log(mesh.vertices.Length);

        for (int i = 2; i < segment + 1; ++i)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i);
        }
        
        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(segment + 1);

        for (int i = segment + 1; i > 2; --i)
        {
            triangles.Add(1);
            triangles.Add(i - 1);
            triangles.Add(i);
        }

        triangles.Add(1);
        triangles.Add(segment + 1);
        triangles.Add(2);

        mesh.triangles = triangles.ToArray();
    }
}
