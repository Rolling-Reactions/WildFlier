using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class gameTimer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] public float remainingTime;
    [SerializeField] TextMeshProUGUI restartTimerText;
    [SerializeField] public float restartRemainingTime = 0;
    public GameObject helicopter;
    public GameObject restartPoint;
    public Camera cameraTempDestroy;



    // Update is called once per frame
    void Update()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        }
        else if (remainingTime < 0)
        {
            remainingTime = 0;
            helicopter.GetComponent<Rigidbody>().useGravity = true;
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (restartRemainingTime > 0)
        {
            restartRemainingTime -= Time.deltaTime;
        }
        else if (restartRemainingTime < 0)
        {
            restartRemainingTime = 0;
            restartTimerText.enabled = false;
        }

        if (restartTimerText.enabled == true)
        {
            int restartSeconds = Mathf.FloorToInt(restartRemainingTime % 60);
            restartTimerText.text = string.Format("Restart in {0:00} seconds", restartSeconds);
        }

    }

    public void gameOverRestart(Camera cameraTemp)
    {
        remainingTime = 0f;
        restartRemainingTime = 5f;
        restartTimerText.enabled = true;
        cameraTempDestroy = cameraTemp;

        Invoke("Restart", 5f);

    }

    public void Restart()
    {
        if (cameraTempDestroy != null)
        {
            Destroy(cameraTempDestroy);
            Vector3 spawnPosition = restartPoint.transform.position;
            helicopter.transform.position = spawnPosition;
            helicopter.SetActive(true);
            helicopter.GetComponent<Rigidbody>().useGravity = false;
            helicopter.GetComponent<Rigidbody>().velocity = Vector3.zero;
            helicopter.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            //Change to enable and move to position GameObject instantiatedObject = Instantiate(helicopter, spawnPosition, Quaternion.identity);
            //instantiatedObject.transform.parent = transform;
            remainingTime = 30f;

        }

    }

}
