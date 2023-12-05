using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBehaviourScript : MonoBehaviour
{
    public GameObject firePrefab;
    public List<GameObject> spawnedObjects = new List<GameObject>();


    private enum FireState
    {
        Started, Spread
    }

    public TerrainData td;
    public TreeGrid tg;
    // make sure this is set on instantiation, before Start()
    public int treeIndex;

    public static float minSpreadTime = 10.0f;
    public static float maxSpreadTime = 20.0f;
    public float spreadTime;

    public static float minDestroyTime = 20.0f;
    public static float maxDestroyTime = 30.0f;
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
            DestroyFire();
        }
    }

    void SpreadFire()
    {
        // spread fire to next tree if it is Alive and close enough
        // Do we want to spread to only one tree are all trees within maxSpreadDistance?
        // Maybe we can have some randomness in spreading
        //List<int> treeList = tg.GetDirectNeighbours(treeIndex);
        List <int> trees = tg.GetDirectNeighbours(treeIndex);
        Debug.Log("Spreading");

        foreach (int treeIndex in trees)
        {
            if (tg.IsHealthy(treeIndex)) { 
                transform.position = tg.Tree2Pos(treeIndex);
                GameObject nextfire = Instantiate(firePrefab);
                Debug.Log("Tree burning: " + transform.position);
                nextfire.GetComponent<FireBehaviourScript>().treeIndex = treeIndex;
                nextfire.GetComponent<FireBehaviourScript>().td = td;
                nextfire.GetComponent<FireBehaviourScript>().tg = tg;
                tg.SetBurning(treeIndex);
            }
        }
        state = FireState.Spread;






    }

    void DestroyFire()
    {
        Debug.Log("Destruction");
        tg.SetDead(treeIndex);
        Destroy(gameObject);
    }

    // Use this for water collision
    private void OnCollisionEnter(Collision collision)
    {
        DestroyFire();
    }
}
/**/