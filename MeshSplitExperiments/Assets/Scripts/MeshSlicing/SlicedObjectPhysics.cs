using UnityEngine;
using System.Collections;

public class SlicedObjectPhysics : MonoBehaviour
{

    private Rigidbody rigidBody;
    private float sliceForce;
    private Mesh mesh;
    
    public void Initialize(ref Rigidbody rigid, float force, ref Mesh sharedMesh)
    {
        rigidBody = rigid;
        sliceForce = force;
        mesh = sharedMesh;
    }

    public void CallRigidExplosion()
    {
        StartCoroutine(RigidExplosion());
    }
    private IEnumerator RigidExplosion()
    {
        yield return new WaitForSeconds(0.1f);
        Vector3 trueCenter = transform.TransformPoint(mesh.bounds.center);
        Vector3 targetDir = (transform.position - trueCenter) * -1;
        targetDir.Normalize();
        rigidBody.AddForce(targetDir * sliceForce);
    }

    
}
