using UnityEngine;

public class MeshSliceScaffolding : MonoBehaviour
{

    [SerializeField]
    private MeshFilter _meshFilter;
    [SerializeField]
    private Vector3 _origin;
    [SerializeField]
    private Vector3 _normal;

    public void SliceMesh()
    {
        
    }
    private void OnDrawGizmosSelected()
    {
        //construct new gizmos matrix taking _normal as forward position
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.LookRotation(_normal), Vector3.one);

        //draw cubes to represent the slicing plane
        Gizmos.color = new Color(0, 1, 0, 04f);
        Gizmos.DrawCube(_origin, new Vector3(2, 2, 0.01f));
        Gizmos.color = new Color(0, 1, 0, 1f);
        Gizmos.DrawWireCube(_origin, new Vector3(2, 2, 0.01f));

        //set matrix to object matrix and draw all normals
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        for(int i = 0; i < _meshFilter.sharedMesh.normals.Length; i++)
        {
            Vector3 normal = _meshFilter.sharedMesh.normals[i];
            Vector3 vertex = _meshFilter.sharedMesh.vertices[i];
            Gizmos.DrawLine(vertex, vertex +  normal);
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
