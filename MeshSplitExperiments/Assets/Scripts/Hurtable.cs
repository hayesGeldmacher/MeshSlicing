using UnityEngine;

public class Hurtable : MonoBehaviour
{
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float health;
    [SerializeField] protected bool dead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        health = maxHealth;
        dead = false;
    }

    public virtual void TakeDamage(float damageValue)
    {
        health -= damageValue;
        if(health <= 0)
        {
            if (!dead){ Die(); }
        }
        Debug.Log("Hurtable has taken damage!");
    }

    public virtual void Heal(float healValue)
    {
        health += healValue;
        if(health > maxHealth) { health = maxHealth; }
    }

    protected virtual void Die()
    {
        dead = true;
        health = 0;
        Debug.Log("Hurtable has died!");
    }
}
