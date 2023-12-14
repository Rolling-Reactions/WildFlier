using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class heliDrop : MonoBehaviour
{
    public float dropYDistance = 20f;
    public float repeatInterval = 10f;
    public float shakeIntensity = 5f; 
    public float shakeDuration = 0.5f;
    public float glitchIntensity = 8f;
    public Camera cameraCurrent;
    public GameObject waterSplashPrefab;

    public ShaderEffect_CorruptedVram cameraShader;
    public gameTimer gameTimer;
    // Start is called before the first frame update
    void Start()
    {

        InvokeRepeating("RepeatMovement", 0f, repeatInterval);
        gameTimer = GameObject.Find("timer").GetComponent<gameTimer>();
    }

    void RepeatMovement()
    {
        if (gameTimer.remainingTime > 0)
        {
            StartCoroutine(DecreaseYOverTime(1f)); // Decrease Y over 1 second
        }
        else
        {
            GetComponent<Rigidbody>().useGravity = true;
        }
    }

    IEnumerator DecreaseYOverTime(float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = transform.position;
        float startY = initialPosition.y;

        while (elapsedTime < duration)
        {
            // Calculate the new Y position gradually
            float newY = Mathf.Lerp(startY, startY - dropYDistance, elapsedTime / duration);

            // Update the object's position
            transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null; // Wait for the next frame
        }

        // Ensure the object reaches the final position precisely
        transform.position = new Vector3(initialPosition.x, startY - dropYDistance, initialPosition.z);

        // Start the shake coroutine after decreasing Y
        StartCoroutine(ShakeObject());
    }


    IEnumerator ShakeObject()
    {
        Vector3 originalPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            // Calculate a random offset for the shake
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);

            // Apply the offset to the object's position
            transform.position = originalPosition + new Vector3(offsetX, offsetY, 0f);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;
            cameraShader.shift = Random.Range(0f, glitchIntensity);

            yield return null; // Wait for the next frame
        }

        // Reset the object's position after the shake
        transform.position = originalPosition;
        cameraShader.shift = 0f;
    }


    private void OnTriggerEnter(Collider colliderCrush)
    {
        // duplicate the camera
        Camera cameraTemp = Instantiate(cameraCurrent, cameraCurrent.transform.position, cameraCurrent.transform.rotation);
        cameraTemp.enabled = true;
        cameraTemp.transform.SetParent(gameTimer.gameObject.transform);

        if (colliderCrush.name == "Plane") // fall into water
        {
            GameObject waterSplash = Instantiate(waterSplashPrefab, transform.position, transform.rotation);
        }
        else {
            //todo: set fire
        }
        gameTimer.gameOverRestart(cameraTemp);

        // Destroy(this.gameObject); change to disable the game object
        this.gameObject.SetActive(false);
    }

}
