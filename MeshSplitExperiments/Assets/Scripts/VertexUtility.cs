using UnityEngine;

//each mesh vertex uses this struct to hold its unique data
public struct VertexData
{
    public Vector3 Position;
    public Vector2 Uv;
    public Vector3 Normal;
    public bool Side; //what side of plane is this on
}

public static class vertexUtility
{
    //gets point between two uvs for creating new vertices
    public static Vector2 InterpolateUvs(Vector2 uv1, Vector2 uv2, float distance)
    {
        Vector2 uv = Vector2.Lerp(uv1, uv2, distance);
        return uv;
    }
}
