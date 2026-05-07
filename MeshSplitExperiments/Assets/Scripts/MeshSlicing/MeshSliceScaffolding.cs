using UnityEngine;
using System.Collections.Generic;


public class MeshSliceScaffolding : MonoBehaviour
{

    [Header("Slicing")]
    [SerializeField]
    private MeshFilter _meshFilter;
    [SerializeField]
    private Vector3 _origin;
    [SerializeField] protected bool isSkinned = false;
    [SerializeField] protected  SkinnedMeshRenderer skinMesh;

    [Range(-1.0f, 1.0f)]
    public float normalX;

    [Range(-1.0f, 1.0f)]
    public float normalY;

    [Range(-1.0f, 1.0f)]
    public float normalZ;


    [SerializeField]
    private Vector3 _normal;
    private Vector3 offsetCenter = Vector3.zero;
    [HideInInspector] public int sliceCount = 0;
    [SerializeField] private int sliceLimit = 5;

    [Header("Debug")]
    [SerializeField]
    private bool _drawDebug = false;
    [SerializeField] protected float planeScaleMult = 1.0f;

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

    [Header("Layers")]
    [SerializeField] private LayerMask debrisMask;
    [SerializeField] protected string debrisLayerName = "debris";

    private bool isPlaying = false; //are we playing or in-editorr?

    private void Start()
    {
        isPlaying = true;
    }

    private void Update()
    {

        _normal = new Vector3(normalX, normalY, normalZ);
    }

    public void StartSlice()
    {
        SliceMesh();
        if (isPlaying)
        {
            Destroy(gameObject); //delete the current gameobject once all copies are made
        }
    }

    public void StartBurst()
    {
        GameObject[] meshes = SliceMesh();
        for(int index = 0; index < meshes.Length; index++)
        {
            GameObject subObject = meshes[index];
            if(subObject.transform.TryGetComponent<MeshSliceScaffolding>(out MeshSliceScaffolding scaffold))
            {
               scaffold.RandomizeRotation();
               scaffold.StartSlice();
            }
          
        }
        if (isPlaying)
        {
            Destroy(gameObject); //delete the current gameobject once all copies are made
        }
    }

    public GameObject[] SliceMesh()
    {
        CheckForZeroRotation();
        Mesh sliceMesh;
        
        
        if (isSkinned) { sliceMesh = skinMesh.sharedMesh; }
        else { sliceMesh = _meshFilter.sharedMesh; }
            Mesh[] meshes = MeshSlicer.SliceMesh(sliceMesh, _origin + offsetCenter, _normal, useDifferentMat);
        List<GameObject> meshObjects = new List<GameObject>();
        for (int index = 0; index < meshes.Length; index++){


            Debug.Log("sliced mesh!");

            Mesh mesh = meshes[index];
            GameObject submesh = Instantiate(this.gameObject);
            if (!isPlaying) { submesh.transform.position += submesh.transform.right * 2; }

            if(submesh.transform.TryGetComponent<MeshFilter>(out MeshFilter filter)){
               filter.sharedMesh = mesh;
            }
            else
            {
                MeshFilter newFilter = submesh.AddComponent(typeof(MeshFilter)) as MeshFilter;
                newFilter.sharedMesh = mesh;
            }

            if(!(submesh.transform.TryGetComponent<MeshRenderer>(out MeshRenderer render)))
            {
                MeshRenderer newRender = submesh.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
                Material[] mat = skinMesh.sharedMaterials;
                newRender.sharedMaterials = mat;
            }
            
            submesh.transform.localScale = transform.lossyScale;
            submesh.transform.position = transform.position;

            if (sliceCount >= sliceLimit - 1)
            {
               Debug.Log("Mesh piece has reached slice limit: " + gameObject.name);
                if(submesh.transform.TryGetComponent<MeshSliceScaffolding>(out MeshSliceScaffolding scaffold))
                {
                    if (isPlaying) { Destroy(scaffold); }
                    else { DestroyImmediate(scaffold); }
                }
                submesh.layer = LayerMask.NameToLayer(debrisLayerName);
            }
            else{

                MeshSliceScaffolding scaffold = submesh.GetComponent<MeshSliceScaffolding>();
                scaffold.sliceCount = sliceCount + 1; //iterate on slice count
                if (isSkinned) { 
                    scaffold.DisableSkinned();
                    scaffold.AssignMeshFilter();
                }
                scaffold.InitializeMesh();
            }

               

            if (useDifferentMat && mat1 != null && mat2 != null) {

                AssignMaterials(ref submesh);
            }

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();


            if(submesh.transform.TryGetComponent<MeshCollider>(out MeshCollider collider))
            {
                if (isPlaying) { Destroy(collider); }
                else { DestroyImmediate(collider); }
            }


            if (submesh.transform.TryGetComponent<Hurtable>(out Hurtable hurt))
            {
                if (isPlaying) { Destroy(hurt); }
                else { DestroyImmediate(hurt); }
            }


            if (!_addPhysics)
            {
                Removecomponents(ref submesh);
            }
            else
            {
                CalculatePhysics(ref submesh, ref mesh);
            }
            meshObjects.Add(submesh);
            
        }
        
        return meshObjects.ToArray();
    }

    private void RandomizeRotation()
    {
        normalX = Random.Range(-1.0f, 1.0f);
        normalY = Random.Range(-1.0f, 1.0f);
        normalZ = Random.Range(-1.0f, 1.0f);
        _normal.x = normalX;
        _normal.y = normalY;
        _normal.z = normalZ;
    }

    private void CheckForZeroRotation()
    {
        bool foundZeroRotation = false;
        if (_normal.x + _normal.y == 0)
        {
            normalX += (Random.value < .5 ? 0.1f : -0.1f);
            normalY += (Random.value < .5 ? 0.1f : -0.1f);
            _normal.x = normalX;
            _normal.y = normalY;
            foundZeroRotation = true;
        }
        else if (_normal.x + _normal.z == 0)
        {
            normalX += (Random.value < .5 ? 0.1f : -0.1f);
            normalZ += (Random.value < .5 ? 0.1f : -0.1f);
            _normal.x = normalX;
            _normal.z = normalZ;
            foundZeroRotation = true;
        }
        else if(_normal.y + _normal.z == 0)
        {
            normalY += (Random.value < .5 ? 0.1f : -0.1f);
            normalZ += (Random.value < .5 ? 0.1f : -0.1f);
            _normal.y = normalY;
            _normal.z = normalZ;
            foundZeroRotation = true;
        }

        if (foundZeroRotation) { Debug.Log("Tried to slice at zero rotation, tweaked cut angle"); }

    }

    private void Removecomponents(ref GameObject subMesh)
    {
        foreach (var comp in subMesh.transform.GetComponents<Component>())
        {
            if(!(comp is Transform || comp is MeshSliceScaffolding|| 
                comp is MeshRenderer || comp is MeshFilter))
            {
                if (isPlaying) { Destroy(comp); }
                else { DestroyImmediate(comp); }
            }
        }
    }

    private void InitializeMesh()
    {
        if (isSkinned) { offsetCenter = skinMesh.sharedMesh.bounds.center; }
        else { offsetCenter = _meshFilter.sharedMesh.bounds.center; }
           
        Debug.Log("Offset center for: " + gameObject.name + " " + offsetCenter);
        normalX = Random.Range(0.1f, 1.0f) * (Random.value < .5 ? 1 : -1);
        normalZ = Random.Range(0.1f, 1.0f) * (Random.value < .5 ? 1 : -1);
        normalY = Random.Range(0.1f, 1.0f) * (Random.value < .5 ? 1 : -1);
        _origin = Vector3.zero;
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

        SlicedObjectPhysics physics;
        if (submesh.transform.TryGetComponent<SlicedObjectPhysics>(out SlicedObjectPhysics sob))
        {
            physics = sob;
            
        }
        else
        {
           physics = submesh.AddComponent(typeof(SlicedObjectPhysics)) as SlicedObjectPhysics;
        }
        physics.Initialize(ref rigid, sliceForce, ref mesh);
        physics.CallRigidExplosion();
        return;
    }


    private void OnDrawGizmosSelected()
    {
        if (!_drawDebug) return;

        //construct new gizmos matrix taking _normal as forward position
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.LookRotation(_normal), Vector3.one);

        Color colA = new Color(0, 1, 0, 0.4f);
        Color colB = new Color(0, 1, 0, 1.0f);

        //draw cubes to represent the slicing plane
        Vector3 scale = transform.localScale * planeScaleMult;
        Gizmos.color = colA;
        Gizmos.DrawCube(_origin + offsetCenter, new Vector3(2 * scale.x, 2 * scale.y, 0.01f * scale.z));
        Gizmos.color = colB;
        Gizmos.DrawWireCube(_origin + offsetCenter, new Vector3(2 * scale.x, 2 * scale.y, 0.01f * scale.z));
    }

    public void DisableSkinned()
    {
        isSkinned = false;
    }

    public void AssignMeshFilter()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }
}
