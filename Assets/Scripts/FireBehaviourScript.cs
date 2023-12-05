using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBehaviourScript : MonoBehaviour
{
    public GameObject firePrefab;
    public GameObject deadTree;

    private enum FireState
    {
        Started, Spread
    }

    public TerrainData td;
    public TreeGrid tg;
    // make sure this is set on instantiation, before Start()
    public int treeIndex;

    public static float minSpreadTime = 3.0f;
    public static float maxSpreadTime = 6.0f;
    public float spreadTime;

    public static float minDestroyTime = 6.0f;
    public static float maxDestroyTime = 9.0f;
    public float destroyTime;

    private float startTime;
    private FireState state;

    public static float maxSpreadDistance = 15.0f;

    void Start()
    {
        gameObject.name = "Fire (tree " + treeIndex + ")";
        transform.position = tg.Tree2Pos(treeIndex);
        spreadTime = Random.Range(minSpreadTime, maxSpreadTime);
        destroyTime = Random.Range(Mathf.Max(spreadTime, minDestroyTime), maxDestroyTime);

        startTime = Time.time;
        state = FireState.Started;
        tg.SetBurning(treeIndex);
}

    // Update is called once per frame
    void Update()
    {
        float elapsedTime = Time.time - startTime;
        if(state == FireState.Started && elapsedTime > spreadTime)
        {
            SpreadFire();
        } else if(state == FireState.Spread && elapsedTime > destroyTime)
        {
            SetTreeDead();
        }
    }

    void SpreadFire()
    {
        // spread fire to next tree if it is Alive and close enough
        // Do we want to spread to only one tree are all trees within maxSpreadDistance?
        // Maybe we can have some randomness in spreading
        //List<int> treeList = tg.GetDirectNeighbours(treeIndex);
        List <int> trees = tg.GetDirectNeighbours(treeIndex);

        for (int i = 0; i < trees.Count; i += 2)
        {
            if (tg.IsHealthy(trees[i])) { 
                transform.position = tg.Tree2Pos(trees[i]);
                GameObject nextfire = Instantiate(firePrefab, transform.parent);
                nextfire.GetComponent<FireBehaviourScript>().treeIndex = trees[i];
                nextfire.GetComponent<FireBehaviourScript>().td = td;
                nextfire.GetComponent<FireBehaviourScript>().tg = tg;
                tg.SetBurning(trees[i]);
            }
        }
        state = FireState.Spread;
    }

    void DestroyFire()
    {
        Destroy(gameObject);  
    }

    void SetTreeDead()
    {
        tg.SetDead(treeIndex);
        GameObject burnedTree = Instantiate(deadTree, transform.parent);
        burnedTree.transform.position = tg.Tree2Pos(treeIndex);
        burnedTree.transform.rotation = Quaternion.Euler(0, td.GetTreeInstance(treeIndex).rotation, 0);
        
        DestroyFire();
    }

    // Use this for water collision
    private void OnCollisionEnter(Collision collision)
    {
        DestroyFire();
    }
}
/**/