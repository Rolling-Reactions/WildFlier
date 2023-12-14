using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpawner : MonoBehaviour
{
    private TreeGrid tg;
    public TerrainData td;
    private TreeInstance[] trees;
    public GameObject FireSmoke_Prefab;

    // Start is called before the first frame update
    void Start()
    {
        tg = new TreeGrid(td, 20.0f);

        // Choose a random tree to start the fire
        int randomTree = Random.Range(0, td.treeInstanceCount);
        GameObject fire = Instantiate(FireSmoke_Prefab, transform);
        fire.GetComponent<FireBehaviourScript>().treeIndex = randomTree;
        fire.GetComponent<FireBehaviourScript>().td = td;
        fire.GetComponent<FireBehaviourScript>().tg = tg;
    }
}
