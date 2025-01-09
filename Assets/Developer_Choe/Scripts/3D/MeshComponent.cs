using UnityEngine;

public class MeshComponent
{
    public static Mesh CreatePoylgon(Mesh mesh, Vector3[] edges, int[] orders)
    {
        mesh.vertices = edges;
        mesh.triangles = orders;
        mesh.RecalculateNormals();

        return mesh;
    }
}
