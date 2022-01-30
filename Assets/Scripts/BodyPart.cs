using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    [SerializeField]
    private float damageMultiplier = 1;

    public void Damage(float damageRecieved)
    {
        GetComponent<Enemy>().Damage(damageRecieved * damageMultiplier);
    }
}
