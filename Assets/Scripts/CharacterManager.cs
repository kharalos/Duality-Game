using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private float health = 100, energy = 100, bright = 100;
    [SerializeField] private Light charLight;
    public bool isNight = true;
    private void Update()
    {
        LightChange(-Time.deltaTime * 2);
        if(transform.position.y < -20)
        {
            HealthChange(-1000);
        }
    }
    public void InsideEnergy()
    {
        if (isNight)
        {
            LightChange(Time.deltaTime * 10);
        }
        else
        {
            LightChange(Time.deltaTime * 3);
            HealthChange(-Time.deltaTime * 7);
        }
    }
    public void InsideEnemyEnergy()
    {
        if (!isNight)
        {
            LightChange(Time.deltaTime * 10);
        }
        else
        {
            LightChange(Time.deltaTime * 3);
            HealthChange(-Time.deltaTime * 7);
        }
    }

    public void HealthChange(float amountChanged)
    {
        health += amountChanged;
        health = Mathf.Clamp(health, 0, 100);
        SetHUD();
        if (health == 0) FindObjectOfType<GameManager>().Death();
    }
    public void EnergyChange(float amountChanged)
    {
        energy += amountChanged;
        energy = Mathf.Clamp(energy, 0, 100);
        SetHUD();
    }
    public void LightChange(float amountChanged)
    {
        bright += amountChanged;
        bright = Mathf.Clamp(bright, 0, 100);
        SetHUD();
        charLight.intensity = bright;
    }

    private void SetHUD()
    {
        FindObjectOfType<HUDManager>().UpdateHUD(bright, energy, health);
    }
}
