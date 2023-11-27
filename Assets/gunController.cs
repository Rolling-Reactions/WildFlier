using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    Gyroscope gyro;

    void Awake()
    {
        gyro = Input.gyro;
        gyro.enabled = true;
    }

    void Update()
    {
        // Get the gyroscope rotation
        Quaternion gyroRot = gyro.attitude;

        // Extract rotations around y and x axes
        float gyroY = gyroRot.eulerAngles.y;
        float gyroX = gyroRot.eulerAngles.x;

        // Create a new rotation with the same z rotation and the gyroscope's y and x rotations
        Quaternion newRotation = Quaternion.Euler(gyroX, gyroY, 0);

        // Apply rotation to the object
        transform.rotation = newRotation;
    }
}
