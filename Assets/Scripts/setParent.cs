using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setParent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        setShooterParent();
    }

    void setShooterParent()
    {
        //createdShooter.transform.SetParent(GameObject.Find("Heli_1").transform, false);
        transform.SetParent(GameObject.Find("Heli_1").transform, false);
    }
}
