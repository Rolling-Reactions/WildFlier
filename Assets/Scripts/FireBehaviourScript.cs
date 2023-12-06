using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireBehaviourScript : MonoBehaviour
{
    public GameObject firePrefab;
    public GameObject deadTree;

    public TerrainData td;
    public TreeGrid tg;
    // make sure this is set on instantiation, before Start()
    public int treeIndex;

    public static float minSpreadTime = 3.0f;
    public static float maxSpreadTime = 6.0f;

    public static float minDestroyTime = 6.0f;
    public static float maxDestroyTime = 9.0f;
    public float destroyTime;

    private float startTime;

    public static float maxSpreadDistance = 15.0f;

    private List<int> treesToSpread;
    private List<float> spreadTimes;
    private int currSpreadIdx = 0;

    private const int burntTerrainLayer = 4;
    private const int burnExtents = 2;

    void Start()
    {
        gameObject.name = "Fire (tree " + treeIndex + ")";
        transform.position = tg.Tree2Pos(treeIndex);
        destroyTime = Random.Range(minDestroyTime, maxDestroyTime);

        startTime = Time.time;
        tg.SetBurning(treeIndex);

        treesToSpread = tg.GetTreesWithinDistance(treeIndex, maxSpreadDistance);
        spreadTimes = new List<float>();
        spreadTimes.Capacity = treesToSpread.Count;
        foreach (int tree in treesToSpread)
        {
            spreadTimes.Add(Random.Range(minSpreadTime, maxSpreadTime));
        }

        treesToSpread = treesToSpread.OrderBy(t => spreadTimes.IndexOf(t)).ToList();
        spreadTimes.Sort();
    }

    // Update is called once per frame
    void Update()
    {
        float elapsedTime = Time.time - startTime;
        if (elapsedTime > minSpreadTime && elapsedTime < maxSpreadTime)
        {
            while (currSpreadIdx < spreadTimes.Count && elapsedTime > spreadTimes[currSpreadIdx])
            {
                SpreadFire(treesToSpread[currSpreadIdx]);
                currSpreadIdx++;
            }
        }
        else if (elapsedTime > destroyTime)
        {
            SetTreeDead();
        }
    }

    // Spread fire to tree index
    void SpreadFire(int index)
    {
        // spread fire to next tree if it is Alive and close enough
        // Do we want to spread to only one tree are all trees within maxSpreadDistance?
        // Maybe we can have some randomness in spreading
        //List<int> treeList = tg.GetDirectNeighbours(treeIndex);

        if (tg.IsHealthy(index))
        {
            GameObject nextfire = Instantiate(firePrefab, transform.parent);
            nextfire.GetComponent<FireBehaviourScript>().treeIndex = index;
            nextfire.GetComponent<FireBehaviourScript>().td = td;
            nextfire.GetComponent<FireBehaviourScript>().tg = tg;
            tg.SetBurning(index);
        }
    }

    void DestroyFire()
    {
        Destroy(gameObject);
    }

    void SetTreeDead()
    {
        tg.SetDead(treeIndex);
        GameObject burnedTree = Instantiate(deadTree, transform.parent);
        burnedTree.transform.SetPositionAndRotation(tg.Tree2Pos(treeIndex), Quaternion.Euler(90, td.GetTreeInstance(treeIndex).rotation, 0));

        // use rock terrainlayer for burnt
        Vector2Int alphamapCoord = Vector2Int.FloorToInt(tg.Tree2NormPos2D(treeIndex) * td.alphamapResolution);
        int burnXPlus = Mathf.Min(burnExtents, (td.alphamapResolution - 1) - alphamapCoord.x);
        int burnXMinus = Mathf.Min(burnExtents, alphamapCoord.x);
        int burnYPlus = Mathf.Min(burnExtents, (td.alphamapResolution - 1) - alphamapCoord.y);
        int burnYMinus = Mathf.Min(burnExtents, alphamapCoord.y);
        float[,,] modifiedAlphamap = td.GetAlphamaps(alphamapCoord.x - burnXMinus, alphamapCoord.y - burnYMinus, burnXMinus + burnXPlus + 1, burnYMinus + burnYPlus + 1);
        for (int j = -burnYMinus; j <= burnYPlus; j++)
        {
            for (int i = -burnXMinus; i <= burnXPlus; i++)
            {
                modifiedAlphamap[i + burnXMinus, j + burnYMinus, burntTerrainLayer] += 1.75f / Mathf.Abs(i + j);
            }
        }
        td.SetAlphamaps(alphamapCoord.x - burnXMinus, alphamapCoord.y - burnYMinus, modifiedAlphamap);

        DestroyFire();
    }

    // Use this for water collision
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == SortingLayer.GetLayerValueFromName("Water"))
        {
            tg.SetHealthy(treeIndex);
            DestroyFire();
        }
    }
}
/**/