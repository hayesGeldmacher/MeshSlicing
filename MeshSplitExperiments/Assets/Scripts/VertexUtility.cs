using UnityEngine;

//each mesh vertex uses this struct to hold its unique data
public struct VertexData
{
    public Vector3 Position;
    public Vector2 Uv;
    public Vector3 Normal;
    public bool Side; //what side of plane is this on
}
