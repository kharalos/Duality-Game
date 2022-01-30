using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject deathPanel, finishPanel;
    [SerializeField] private float timePassed, time;
    private bool isNight = true;

    // Start is called before the first frame update
    void Start()
    {
        deathPanel.SetActive(false);
        finishPanel.SetActive(false);
        Time.timeScale = 1;
        time = 15;
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        time -= Time.deltaTime;
        FindObjectOfType<HUDManager>().UpdateHUD(time);
        if (time <= 0)
        {
            Cycler();
        }
    }

    private void Cycler()
    {
        time = 15;
        isNight = !isNight;

        if (isNight)
        {
            FindObjectOfType<CycleManager>().Night();
        }
        else
        {
            FindObjectOfType<CycleManager>().Day();
        }
    }

    public void Death()
    {
        Time.timeScale = 0;
        deathPanel.SetActive(true);
        FindObjectOfType<Controller>().DisplayCursor(true);
    }
    public void ToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void Finish()
    {
        Time.timeScale = 0;
        finishPanel.SetActive(true);
        FindObjectOfType<Controller>().DisplayCursor(true);
    }
}
