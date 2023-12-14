using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    Gyroscope gyro;
    public GameObject emitter1;
    public GameObject emitter2;
    public GameObject waterBall;
    public Transform launchPoint;
    public float velocity = 10f;
    private bool charging = false;

    void Awake()
    {
        gyro = Input.gyro;
        gyro.enabled = true;
    }
    private void Start()
    {
        emitter1.SetActive(false);
        emitter2.SetActive(false);
    }

    void Update()
    {

        // Get the gyroscope rotation
        Quaternion gyroRot = gyro.attitude;

        // Extract rotations around y and x axes
        float gyroX = gyroRot.eulerAngles.x;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + Input.acceleration.x, 0);
    }
    public void StartEmitter()
    {
        if (!charging) {
        FireProjectile();
        }
        //emitter1.SetActive(true);
        //emitter2.SetActive(true);
    }
    public void StopEmitter()
    {
        charging = false;
        emitter1.SetActive(false);
        emitter2.SetActive(false);

    }
    public void FireProjectile()
    {
        charging = true;
        var projectile = Instantiate(waterBall, launchPoint.position, launchPoint.rotation);
        projectile.GetComponent<Rigidbody>().velocity = launchPoint.up * velocity;
        
    }
}
