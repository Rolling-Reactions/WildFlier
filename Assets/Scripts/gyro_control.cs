using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class gyro_control : MonoBehaviourPun
{
    //[SerializeField]
    //private float speed = 10f;
    //[SerializeField]
    //private float upSpeed = 2f;
    [SerializeField]
    private float maxSpeed = 10f;
    public float angleSpeed = 2.0f;   // Speed of tilting
    public float maxAngle = 90.0f; // Maximum tilt angle
    public float moveSpeedMultiplier = 3.0f; // Speed multiplier based on tilt angle
    public float smoothingFactor = 2.0f; // Adjust this factor for smoothing
    private float smoothTiltAngle = 0.0f;
    private float smoothRollAngle = 0.0f;
    public float rotationSpeed = 5.0f;
    private float direction = 0.0f;
    public GameObject HelicopterDir;

    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Input.gyro.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        //photonView.RPC("RPC_HandleGyroscopeInput", RpcTarget.All, Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
    }

    [PunRPC]
    private void RespawnSpaceship(float x, float y, float z, float w)
    {
        //transform.rotation = new Quaternion(x, y, -z, -w);
        Quaternion phone_rotation = new Quaternion(-x, -y, z, w);

        float roll = phone_rotation.eulerAngles.x;
        float tilt = phone_rotation.eulerAngles.y;
        roll = (roll > 180f) ? roll - 360f : roll;
        tilt = (tilt > 180f) ? tilt - 360f : tilt;

        roll = Mathf.Clamp(roll, -45, 90);
        tilt = Mathf.Clamp(tilt, -90, 90);


        float sensitivity = 0.75f;
        direction = direction + tilt*0.8f;
        // Apply the rotation to the object
        Quaternion targetRotation = Quaternion.Euler(roll * sensitivity, -direction * sensitivity, 0F);
        transform.rotation = targetRotation;

        smoothRollAngle = Mathf.Lerp(smoothRollAngle, roll, smoothingFactor * Time.deltaTime);
        smoothTiltAngle = Mathf.Lerp(smoothTiltAngle, tilt, smoothingFactor * Time.deltaTime);

        MoveLongitudinally(smoothRollAngle, roll);
        //MoveSideways(smoothTiltAngle);
        Debug.Log($"Received Gyroscope Data: x={roll}, y={tilt}");
    }

    void MoveSideways(float tiltAngle)
    {
        float lateralMovement = tiltAngle / maxAngle;

        float speed = Mathf.Abs(tiltAngle) * moveSpeedMultiplier;

        transform.Translate(Vector3.left * lateralMovement * speed * Time.deltaTime, Space.World);
            }
    void MoveLongitudinally(float RollAngle, float xRot)
    {
        HelicopterDir.transform.eulerAngles = new Vector3(0, HelicopterDir.transform.eulerAngles.y, HelicopterDir.transform.eulerAngles.z);
        float lateralMovement = RollAngle / maxAngle; 
        Debug.Log(RollAngle);

        float speed = Mathf.Abs(RollAngle) * moveSpeedMultiplier;

        //transform.Translate(HelicopterDir.transform.forward * speed * Time.deltaTime, Space.Self);

            rb.AddForce(HelicopterDir.transform.forward * xRot * lateralMovement * 1000 * Time.deltaTime);

    }
}
