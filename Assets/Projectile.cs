using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float life = 5f;
    // Start is called before the first frame update
    private void Awake()
    {
        Destroy(gameObject, life);
    }
}
