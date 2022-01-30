using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCircle : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<CharacterManager>().InsideEnergy();
        }
    }
}
