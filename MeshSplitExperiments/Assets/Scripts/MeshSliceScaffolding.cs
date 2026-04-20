using UnityEngine;
using System.Collections.Generic;

public class MeshSliceScaffolding : MonoBehaviour
{

    [Header("Slicing")]
    [SerializeField]
    private MeshFilter _meshFilter;
    [SerializeField]
    private Vector3 _origin;
    [SerializeField]
    private Vector3 _normal;
    [HideInInspector] public int sliceCount = 0;
    [SerializeField] private int sliceLimit = 5;

    [Header("Debug")]
    [SerializeField]
    private bool _drawDebug = false;

    [Header("Physics")]
    [SerializeField]
    private bool _addPhysics = true;
    [Range(0.0f, 1000.0f)]
    public float sliceForce = 200.0f;

    [Header("Materials")]
    [SerializeField]
    private bool useDifferentMat = false;
    [SerializeField] private Material mat1;
    [SerializeField] private Material mat2;

    private bool isPlaying = false; //are we playing or in-editorr?

    private void Start()
    {
        isPlaying = true;
    }

    public void SliceMesh()
    {
        Mesh[] meshes = MeshSlicer.SliceMesh(_meshFilter.sharedMesh, _origin, _normal, useDifferentMat);

        for (int index = 0; index < meshes.Length; index++){

            Debug.Log("sliced mesh!");

            Mesh mesh = meshes[index];
            GameObject submesh = Instantiate(this.gameObject);
            if (!isPlaying) { submesh.transform.position += submesh.transform.right * 2; }
            submesh.GetComponent<MeshFilter>().sharedMesh = mesh;

            if (useDifferentMat && mat1 != null && mat2 != null) {
                AssignMaterials(ref submesh);
            }

            if (sliceCount >= sliceLimit - 1)
            {
               Debug.Log("Mesh piece has reached slice limit: " + gameObject.name);
                if(submesh.transform.TryGetComponent<MeshSliceScaffolding>(out MeshSliceScaffolding scaffold))
                {
                    if (isPlaying) { Destroy(scaffold); }
                    else { DestroyImmediate(scaffold); }
                }
            }
            else{
                
                submesh.GetComponent<MeshSliceScaffolding>().sliceCount = sliceCount + 1; //iterate on slice count
            }


            if(submesh.transform.TryGetComponent<MeshCollider>(out MeshCollider collider))
            {
                if (isPlaying) { Destroy(collider); }
                else { DestroyImmediate(collider); }
            }

            if (!_addPhysics)
            {
                if(submesh.transform.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    Debug.Log("got rigidbody for: " + submesh.name);
                    if (isPlaying) { Destroy(rb); }
                    else { DestroyImmediate(rb); }
                }
            }
            else
            {
                CalculatePhysics(ref submesh, ref mesh);
            }
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
        }


        if (isPlaying)
        {
            Destroy(gameObject); //delete the current gameobject once all copies are made
        }
    }

    private void AssignMaterials(ref GameObject submesh)
    {
        Material[] mats = { mat1, mat2 };
        MeshRenderer meshRenderer = submesh.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = mats;
    }

    private void CalculatePhysics(ref GameObject submesh, ref Mesh mesh)
    {
        MeshCollider newCollider = submesh.AddComponent(typeof(MeshCollider)) as MeshCollider;
        newCollider.sharedMesh = mesh;
        newCollider.convex = true;

        Rigidbody rigid;
        if (submesh.transform.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rigid = rb;
        }
        else
        {
            rigid = submesh.AddComponent(typeof(Rigidbody)) as Rigidbody;
        }

        if (sliceForce > 0 && isPlaying)
        {
            Vector3 trueCenter = submesh.transform.TransformPoint(mesh.bounds.center);
            Vector3 targetDir = (transform.position - trueCenter) * -1;
            targetDir.Normalize();
            rigid.AddForce(targetDir * sliceForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!_drawDebug) return;

        //construct new gizmos matrix taking _normal as forward position
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.LookRotation(_normal), Vector3.one);

        Color colA = new Color(0, 1, 0, 0.4f);
        Color colB = new Color(0, 1, 0, 1.0f);

        //draw cubes to represent the slicing plane
        Vector3 scale = transform.localScale;
        Gizmos.color = colA;
        Gizmos.DrawCube(_origin, new Vector3(2 * scale.x, 2 * scale.y, 0.01f * scale.z));
        Gizmos.color = colB;
        Gizmos.DrawWireCube(_origin, new Vector3(2 * scale.x, 2 * scale.y, 0.01f * scale.z));
    }
}
