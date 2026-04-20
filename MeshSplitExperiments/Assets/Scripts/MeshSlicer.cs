using UnityEngine;
using System.Collections.Generic;
//using VertexUtility;

public static class MeshSlicer
{

    public static Mesh[] SliceMesh(Mesh mesh, Vector3 cutOrigin, Vector3 cutNormal, bool useSubMesh)
    {

        Plane plane = new Plane(cutNormal, cutOrigin); //create a plane based on defined normal and origin
        MeshConstructionHelper positiveMesh = new MeshConstructionHelper(); //create mesh on positive side of plane
        MeshConstructionHelper negativeMesh = new MeshConstructionHelper(); //create mesh on negative side of plane

        positiveMesh.createSubmesh = useSubMesh;
        negativeMesh.createSubmesh = useSubMesh;

        List<VertexData> pointsAlongPlane = new List<VertexData>();
        //iterate over every triangle in the mesh, see if its on positive or negative side of normal-oriented plane
        int[] meshTriangles = mesh.triangles; //get array of vertices in order or triangles
        for (int i = 0; i < meshTriangles.Length; i += 3)
        {
            VertexData vertexA = GetVertexData(mesh, plane, meshTriangles[i]);
            VertexData vertexB = GetVertexData(mesh, plane, meshTriangles[i + 1]);
            VertexData vertexC = GetVertexData(mesh, plane, meshTriangles[i + 2]);

            //check if vertices on the triangle are split across plane
            bool isABSameSide = vertexA.Side == vertexB.Side;
            bool isBCSameSide = vertexB.Side == vertexC.Side;

            if (isABSameSide && isBCSameSide)
            {
                //if on the same side, add each vertex to positive or negative side
                MeshConstructionHelper helper = vertexA.Side ? positiveMesh : negativeMesh;
                helper.AddMeshSection(vertexA, vertexB, vertexC, false);
            }
            else //triangle is split between positive and negative normal plane
            {
                //have to find intersection between triangle and slice plane
                VertexData intersectionD;
                VertexData intersectionE;

                //get the right helper for each triangle corner
                MeshConstructionHelper helperA = vertexA.Side ? positiveMesh : negativeMesh;
                MeshConstructionHelper helperB = vertexB.Side ? positiveMesh : negativeMesh;
                MeshConstructionHelper helperC = vertexC.Side ? positiveMesh : negativeMesh;

                //c on other side
                if (isABSameSide)
                {
                    intersectionD = GetIntersectionVertex(vertexA, vertexC, cutOrigin, cutNormal);
                    intersectionE = GetIntersectionVertex(vertexB, vertexC, cutOrigin, cutNormal);

                    //create new triangle between mesh intersections
                    helperA.AddMeshSection(vertexA, vertexB, intersectionE, false);
                    helperA.AddMeshSection(vertexA, intersectionE, intersectionD, false);
                    helperC.AddMeshSection(intersectionE, vertexC, intersectionD, false);

                }//a on other side
                else if (isBCSameSide)
                {
                    intersectionD = GetIntersectionVertex(vertexB, vertexA, cutOrigin, cutNormal);
                    intersectionE = GetIntersectionVertex(vertexC, vertexA, cutOrigin, cutNormal);

                    //create new triangles between mesh interactions
                    helperB.AddMeshSection(vertexB, vertexC, intersectionE, false);
                    helperB.AddMeshSection(vertexB, intersectionE, intersectionD, false);
                    helperA.AddMeshSection(intersectionE, vertexA, intersectionD, false);


                }
                else //b on other side
                {
                    intersectionD = GetIntersectionVertex(vertexA, vertexB, cutOrigin, cutNormal);
                    intersectionE = GetIntersectionVertex(vertexC, vertexB, cutOrigin, cutNormal);

                    helperA.AddMeshSection(vertexA, intersectionE, vertexC, false);
                    helperA.AddMeshSection(intersectionD, intersectionE, vertexA, false);
                    helperB.AddMeshSection(vertexB, intersectionE, intersectionD, false);
                }

                //add new intersecting vertices to a list
                pointsAlongPlane.Add(intersectionD);
                pointsAlongPlane.Add(intersectionE);


            }
        }

        JoinPointsAlongPlane(ref positiveMesh, ref negativeMesh, cutNormal, pointsAlongPlane);
        return new[] { positiveMesh.ConstructMesh(), negativeMesh.ConstructMesh() }; //return both new objects
    }

    private static void JoinPointsAlongPlane(ref MeshConstructionHelper positive, ref MeshConstructionHelper negative, Vector3 cutNormal, List<VertexData> pointsAlongPlane)
    {
        VertexData halfway = new VertexData()
        {
            Position = VertexUtility.GetHalfwayPoint(pointsAlongPlane)
        };

        for (int i = 0; i < pointsAlongPlane.Count; i += 2)
        {

            VertexData firstVertex = pointsAlongPlane[i];
            VertexData secondVertex = pointsAlongPlane[i + 1];

            Vector3 normal = VertexUtility.ComputeNormal(halfway, secondVertex, firstVertex);
            halfway.Normal = Vector3.forward;

            float dot = Vector3.Dot(normal, cutNormal);

            //check which side of the plane normal is
            //then add new triangle to each construction helper

            if (dot > 0)
            {
                //if normal aligns with plane normal
                positive.AddMeshSection(firstVertex, secondVertex, halfway, true);
                negative.AddMeshSection(secondVertex, firstVertex, halfway, true);
            }
            else
            {
                //if normal is opposite to plane normal
                negative.AddMeshSection(firstVertex, secondVertex, halfway, true);
                positive.AddMeshSection(secondVertex, firstVertex, halfway, true);
            }


        }
    }

    //gets the center of the mesh by calculating the average position of each vertex
    public static Vector3 GetMeshCenter(Mesh mesh)
    {
        Vector3[] meshVertices = mesh.vertices; //get array of vertices in order or triangles
        Vector3 averagePos = Vector3.zero;
        for (int i = 0; i < meshVertices.Length; i += 1)
        {
            averagePos += meshVertices[i];
        }
        averagePos /= meshVertices.Length;
        return averagePos;
    }

    //extracts vertex data from mesh and places in custom struct for easy use
    private static VertexData GetVertexData(Mesh mesh, Plane plane, int index)
    {
        Vector3 position = mesh.vertices[index];
        VertexData vertexData = new VertexData()
        {
            Position = position,
            Side = plane.GetSide(position),
            Uv = mesh.uv[index],
            Normal = mesh.normals[index]
        };
        return vertexData;
    }

    public static bool PointIntersectsAPlane(Vector3 from, Vector3 to, Vector3 planeOrigin, Vector3 normal, out Vector3 result)
    {
        Vector3 translation = to - from;
        float dot = Vector3.Dot(normal, translation);
        //check if lines are not perpendicular
        if (Mathf.Abs(dot) > System.Single.Epsilon)
        {
            Vector3 fromOrigin = from - planeOrigin;
            float fac = -Vector3.Dot(normal, fromOrigin) / dot;
            translation = translation * fac;
            result = from + translation;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    //create new vertexData based on intersection position
    private static VertexData GetIntersectionVertex(VertexData vertexA, VertexData vertexB, Vector3 planeOrigin, Vector3 normal)
    {
        PointIntersectsAPlane(vertexA.Position, vertexB.Position, planeOrigin, normal, out Vector3 result);
        float distanceA = Vector3.Distance(vertexA.Position, result);
        float distanceB = Vector3.Distance(vertexB.Position, result);
        float t = distanceA / (distanceA + distanceB);

        return new VertexData()
        {
            Position = result,
            Normal = normal,
            Uv = VertexUtility.InterpolateUvs(vertexA.Uv, vertexB.Uv, t)
        };
    }

}


class MeshConstructionHelper
{
    //all properties needed to construct a mesh
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Vector2> _uvs;
    private List<Vector3> _normals;

    public bool createSubmesh = false;
    private List<int> _trianglesSub;

    //constructor for class
    public MeshConstructionHelper()
    {
        _triangles = new List<int>();
        _trianglesSub = new List<int>();
        _vertices = new List<Vector3>();
        _uvs = new List<Vector2>();
        _normals = new List<Vector3>();

    }

    //receives three vertics and produces  a triangle
    public void AddMeshSection(VertexData vertexA, VertexData vertexB, VertexData vertexC, bool addSub)
    {
        int indexA = TryAddVertex(vertexA);
        int indexB = TryAddVertex(vertexB);
        int indexC = TryAddVertex(vertexC);

        if (addSub && createSubmesh)
        {
            AddTriangleSub(indexA, indexB, indexC);
        }
        else
        {
            Debug.Log("added sub triangles!");
            AddTriangle(indexA, indexB, indexC);
        }
    }

    private void AddTriangle(int indexA, int indexB, int indexC)
    {
        _triangles.Add(indexA);
        _triangles.Add(indexB);
        _triangles.Add(indexC); 
    }

    private void AddTriangleSub(int indexA, int indexB, int indexC)
    {
        _trianglesSub.Add(indexA);
        _trianglesSub.Add(indexB);
        _trianglesSub.Add(indexC);
    }

    private int TryAddVertex(VertexData vertex)
    {
        _vertices.Add(vertex.Position);
        _uvs.Add(vertex.Uv);
        _normals.Add(vertex.Normal);
        return _vertices.Count - 1;
    }

    //produces a mesh from the given verts, uvs, normals, triangles
    public Mesh ConstructMesh()
    {
        Mesh mesh = new Mesh();
        Debug.Log(" triangle count " +  _triangles.Count + " sub count: " + _trianglesSub.Count);
        //fill mesh data
        mesh.vertices = _vertices.ToArray(); //similar to c++ vector, getting &[0]
        mesh.normals = _normals.ToArray();
        mesh.uv = _uvs.ToArray();
        mesh.triangles = _triangles.ToArray();

        if (createSubmesh)
        {
            //(List<ushort> triangles, int submesh, int meshLod, bool calculateBounds, int baseVertex)
            mesh.subMeshCount = 2;
            mesh.SetTriangles(_triangles.ToArray(), 0);
            mesh.SetTriangles(_trianglesSub.ToArray(), 1);

        }
        
        return mesh;
    }
}
