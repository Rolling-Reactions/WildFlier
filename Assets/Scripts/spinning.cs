using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spinning : MonoBehaviour
{
    [SerializeField]
    private Vector3 axis;

    [SerializeField]
    private float spinSpeed = 10f;
    // Start is called before the first frame update

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(axis, spinSpeed);
    }
}
