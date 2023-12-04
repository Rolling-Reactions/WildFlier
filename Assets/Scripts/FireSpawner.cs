using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpawner : MonoBehaviour
{
    private TreeGrid tg;
    public TerrainData td;
    private TreeInstance[] trees;
    public GameObject firePrefab;

    // Start is called before the first frame update
    void Start()
    {
        trees = td.treeInstances;
        tg = new TreeGrid(td, 20.0f);

        // Choose a random tree to start the fire
        int randomTree = Random.Range(0, trees.Length);
        GameObject fire = Instantiate(firePrefab, transform);
        fire.GetComponent<FireBehaviourScript>().treeIndex = randomTree;
        fire.GetComponent<FireBehaviourScript>().td = td;
        fire.GetComponent<FireBehaviourScript>().tg = tg;
    }
}
