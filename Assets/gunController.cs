using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    Gyroscope gyro;
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
    }

    void Update()
    {

        // Get the gyroscope rotation
        Quaternion gyroRot = gyro.attitude;

        // Extract rotations around y and x axes
        float gyroX = gyroRot.eulerAngles.x;

        transform.eulerAngles = new Vector3(gyroX, transform.eulerAngles.y + Input.acceleration.x, 0);
    }
    public void StartEmitter()
    {
        if (!charging) {
        FireProjectile();
        }
    }
    public void StopEmitter()
    {
        charging = false;

    }
    public void FireProjectile()
    {
        charging = true;
        var projectile = Instantiate(waterBall, launchPoint.position, launchPoint.rotation);
        projectile.GetComponent<Rigidbody>().velocity = launchPoint.up * velocity;
        
    }
}
