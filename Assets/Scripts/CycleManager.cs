using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CycleManager : MonoBehaviour
{
    [SerializeField] Material blueEnergy, redEnergy;
    [SerializeField] Material nightSB, daySB;
    List<MeshRenderer> energyBallMats = new List<MeshRenderer>();
    List<MeshRenderer> enemyMats = new List<MeshRenderer>();

    private void Start()
    {
        GetMaterials();
    }
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Night();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Day();
        }
    }*/

    private void GetMaterials()
    {
        energyBallMats.Clear();
        enemyMats.Clear();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("EnergyBall"))
        {
            energyBallMats.Add(obj.GetComponent<MeshRenderer>());
        }
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("BodyPart"))
        {
            enemyMats.Add(obj.GetComponent<MeshRenderer>());
        }
    }

    public void Night()
    {
        RenderSettings.skybox = nightSB;
        FindObjectOfType<CharacterManager>().isNight = true;
        SetMaterials(true);
    }

    public void Day()
    {
        RenderSettings.skybox = daySB;
        FindObjectOfType<CharacterManager>().isNight = false;
        SetMaterials(false);
    }

    private void SetMaterials(bool isNight)
    {
        foreach(MeshRenderer rend in energyBallMats)
        {
            if (isNight) rend.material = blueEnergy;
            else rend.material = redEnergy;
        }
        foreach (MeshRenderer rend in enemyMats)
        {
            if (!isNight) rend.material = blueEnergy;
            else rend.material = redEnergy;
        }
    }
}
