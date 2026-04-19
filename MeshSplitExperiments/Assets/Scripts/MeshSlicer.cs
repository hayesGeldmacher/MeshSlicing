using UnityEngine;
using System.Collections.Generic;
//using VertexUtility;

public static class MeshSlicer
{

    public static Mesh[] SliceMesh(Mesh mesh, Vector3 cutOrigin, Vector3 cutNormal)
    {
       
        Plane plane = new Plane(cutNormal, cutOrigin); //create a plane based on defined normal and origin
        MeshConstructionHelper positiveMesh = new MeshConstructionHelper(); //create mesh on positive side of plane
        MeshConstructionHelper negativeMesh = new MeshConstructionHelper(); //create mesh on negative side of plane

        //iterate over every triangle in the mesh, see if its on positive or negative side of normal-oriented plane
        int[] meshTriangles = mesh.triangles; //get array of vertices in order or triangles
        for(int i = 0; i < meshTriangles.Length; i += 3)
        {
            VertexData vertexA = GetVertexData(mesh, plane, meshTriangles[i]);
            VertexData vertexB = GetVertexData(mesh, plane, meshTriangles[i + 1]);
            VertexData vertexC = GetVertexData(mesh, plane, meshTriangles[i + 2]);

            //check if vertices on the triangle are split across plane
            bool isABSameSide = vertexA.Side == vertexB.Side;
            bool isBCSameSide = vertexB.Side == vertexC.Side;

            if(isABSameSide && isBCSameSide)
            {
                //if on the same side, add each vertex to positive or negative side
                MeshConstructionHelper helper = vertexA.Side? positiveMesh : negativeMesh;
                helper.AddMeshSection(vertexA, vertexB, vertexC);
            }
            else //triangle is split between positive and negative normal plane
            {

            }
        }

        return new[] { positiveMesh.ConstructMesh(), negativeMesh.ConstructMesh() }; //return both new objects
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
        float distanceA = vector3.Distance(vertexA.Position, result);
        float distanceB = vector3.Distance(vertexB.Position, result);
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

    //constructor for class
    public MeshConstructionHelper()
    {
        _triangles = new List<int>();
        _vertices = new List<Vector3>();
        _uvs = new List<Vector2>();
        _normals = new List<Vector3>();
    }

    //receives three vertics and produces  a triangle
    public void AddMeshSection(VertexData vertexA, VertexData vertexB, VertexData vertexC)
    {
        int indexA = TryAddVertex(vertexA);
        int IndexB = TryAddVertex(vertexB);
        int IndexC = TryAddVertex(vertexC);

        AddTriangle(indexA, IndexB, IndexC);
    }

    private void AddTriangle(int indexA, int indexB, int indexC)
    {
        _triangles.Add(indexA);
        _triangles.Add(indexB);
        _triangles.Add(indexC);
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
        //fill mesh data
        mesh.vertices = _vertices.ToArray(); //similar to c++ vector, getting &[0]
        mesh.triangles = _triangles.ToArray();
        mesh.normals = _normals.ToArray();
        mesh.uv = _uvs.ToArray();
        return mesh;
    }
}
