using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float health = 100;
    private bool isDead = false;
    private bool pursue = false;

    private Transform target;

    [SerializeField] private AudioClip dieSound;

    private Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        target = FindObjectOfType<CharacterManager>().transform;
    }
    private void Update()
    {
        if ((target.position - transform.position).magnitude < 40 && !pursue) pursue = true;
        if (pursue)
        {
            rb.AddForce((target.position - transform.position).normalized * 10);

            if ((target.position - transform.position).magnitude > 30)
            {
                rb.AddForce((target.position - transform.position).normalized * 1000);
            }
        }
    }
    public enum EnemyType
    {
        Rookie,
        Veteran,
        Elite,
    }
    public EnemyType type;
    public virtual void Damage(float damageAmount)
    {
        if (isDead) return;
        health -= damageAmount;
        Debug.Log("Inflicted damage is " + damageAmount);
        if(health <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        isDead = true;
        pursue = false;
        Debug.Log(gameObject.name + " has died.");
        GetComponent<AudioSource>().PlayOneShot(dieSound);
        FindObjectOfType<CharacterManager>().HealthChange(+50);
        Destroy(gameObject, 4f);
    }

    public virtual void Stop()
    {
        pursue = false;
        rb.velocity = Vector3.zero;
    }
}
