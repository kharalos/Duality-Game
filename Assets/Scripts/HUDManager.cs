using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HUDManager : MonoBehaviour
{
    [SerializeField] private Image timeImage, lightImage, protectionImage, healthImage;
    [SerializeField] private TextMeshProUGUI timeValue, lightValue, protectionValue, healthValue;

    [SerializeField] private TextMeshProUGUI bulletCount, ammoCount;

    public void UpdateHUD(float time)
    {
        timeImage.fillAmount = time / 15;
        timeValue.text = Mathf.Floor(time).ToString();
    }


    public void UpdateHUD(float bullet, float ammo)
    {
        bulletCount.text = Mathf.Floor(bullet).ToString();
        ammoCount.text = Mathf.Floor(ammo).ToString();
    }

    public void UpdateHUD(float light, float protection, float health)
    {
        lightImage.fillAmount = light / 100;
        protectionImage.fillAmount = protection / 100;
        healthImage.fillAmount = health / 100;

        lightValue.text = Mathf.Floor(light).ToString();
        if (light > 99) lightValue.text = 100.ToString();
        protectionValue.text = Mathf.Floor(protection).ToString();
        healthValue.text = Mathf.Floor(health).ToString();
    }
}
