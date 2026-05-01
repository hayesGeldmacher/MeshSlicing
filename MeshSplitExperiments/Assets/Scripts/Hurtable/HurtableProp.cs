using UnityEngine;

public class HurtableProp : Hurtable
{
    
    protected enum enExplosionState { NONE, SLICE, EXPLODE }
    [SerializeField] protected enExplosionState explodeState; //tracks current player state

    private MeshSliceScaffolding scaffolding;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        scaffolding = transform.GetComponent<MeshSliceScaffolding>();
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
        base.Die();
        if(scaffolding == null) { Destroy(gameObject); return; }
        switch (explodeState) { 
        
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
    }
}
