using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BugmanAI : MonoBehaviour
{
    public enum EnemyStates
    {
        Idle,
        Alert,
        Engaged,
        Shooting
    }
    public EnemyStates state;
    private Animator animator;
    private NavMeshAgent agent;
    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

}
