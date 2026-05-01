using UnityEngine;

public class HurtableJoint : Hurtable
{

    protected enum enExplosionState { NONE, SLICE, EXPLODE }
    [SerializeField] protected enExplosionState explodeState; //tracks current player state

    [SerializeField] private MeshSliceScaffolding scaffolding;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

    }

    public override void Heal(float healValue)
    {
        base.Heal(healValue);
    }

    protected override void Die()
    {
      
        if (scaffolding == null) { Destroy(gameObject); return; }
        switch (explodeState)
        {

            case enExplosionState.NONE:
                Destroy(gameObject);
                break;
            case enExplosionState.SLICE:
                scaffolding.StartSlice();
                break;
            case enExplosionState.EXPLODE:
                scaffolding.StartBurst();
                break;

        }
        //we don't want to destroy this game 0bject because its a joint, will mess with animations
        //instead, remove joint script, slice mesh, and recursively remove all children bone meshes
        //Destroy(this);
    }
}
