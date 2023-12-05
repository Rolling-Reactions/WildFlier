using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpawner : MonoBehaviour
{
    TreeGrid tg;
    public TerrainData terrainData;
    private TreeInstance[] trees;
    public GameObject firePrefab;

    // Start is called before the first frame update
    void Start()
    {
        trees = terrainData.treeInstances;
        tg = new TreeGrid(terrainData, 20.0f);

        // Choose a random tree to start the fire
        int randomTree = Random.Range(0, trees.Length);
        GameObject fire = Instantiate(firePrefab);
        fire.transform.position = Vector3.Scale(terrainData.GetTreeInstance(randomTree).position, terrainData.size);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
