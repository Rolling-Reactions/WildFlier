using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBehaviourScript : MonoBehaviour
{
    public TerrainData terrainData;

    private TreeInstance[] trees;


    // Start is called before the first frame update
    void Start()
    {
        trees = terrainData.treeInstances;
        trees[0].position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
