using UnityEngine;
using System.Collections.Generic;

//each mesh vertex uses this struct to hold its unique data
public struct VertexData
{
    public Vector3 Position;
    public Vector2 Uv;
    public Vector3 Normal;
    public bool Side; //what side of plane is this on
}

public static class VertexUtility
{
    //gets point between two uvs for creating new vertices
    public static Vector2 InterpolateUvs(Vector2 uv1, Vector2 uv2, float distance)
    {
        Vector2 uv = Vector2.Lerp(uv1, uv2, distance);
        return uv;
    }

    //computes a new normal vector based on two arms of triangle
    public static Vector3 ComputeNormal(VertexData vertexA, VertexData vertexB, VertexData vertexC)
    {
        Vector3 sideL = vertexB.Position - vertexA.Position;
        Vector3 sideR = vertexC.Position - vertexA.Position;
        Vector3 normal = Vector3.Cross(sideL, sideR);
        return normal.normalized;
    }

    public static Vector3 GetHalfwayPoint(List<VertexData> pointsAlongPlane)
    {
        if(pointsAlongPlane.Count > 0)
        {
            Vector3 firstPoint = pointsAlongPlane[0].Position;
            Vector3 furthestPoint = Vector3.zero;
            float distance = 0.0f;

            for(int index = 0; index < pointsAlongPlane.Count; index++)
            {
                Vector3 point = pointsAlongPlane[index].Position;
                float currentDistance = 0f;
                currentDistance = Vector3.Distance(firstPoint, point);

                if(currentDistance > distance)
                {
                    distance = currentDistance;
                    furthestPoint = point;
                }
            }
            return Vector3.Lerp(firstPoint, furthestPoint, 0.5f);
        }
        else
        {
            return Vector3.zero;
        }
    }
}
