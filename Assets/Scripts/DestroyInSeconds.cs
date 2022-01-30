using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyInSeconds : MonoBehaviour
{
    [SerializeField]
    private float destroyTime = 5;
    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
