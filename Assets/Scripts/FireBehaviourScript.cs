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

    public static float minSpreadTime = 5.0f;
    public static float maxSpreadTime = 15.0f;

    public static float minDestroyTime = maxSpreadTime;
    public static float maxDestroyTime = 15.0f;
    public float destroyTime;

    private float startTime;

    public static float maxSpreadDistance = 15.0f;

    private List<int> treesToSpread;
    private List<float> spreadTimes;
    private int currSpreadIdx = 0;

    private const int grassTerrainLayer = 0;
    private const int burntTerrainLayer = 4;
    private const int burnExtents = 3;

    void Start()
    {
        gameObject.name = "Fire (tree " + treeIndex + ")";
        transform.position = tg.Tree2Pos(treeIndex);
        minDestroyTime = maxSpreadTime;
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
            SpreadFire();
        }
        if (elapsedTime > destroyTime)
        {
            SetTreeDead();
            DestroyFire();
        }
    }

    // Spread fire to neighbouring trees at random times defined by spreadTimes
    void SpreadFire()
    {
        float elapsedTime = Time.time - startTime;

        while (currSpreadIdx < spreadTimes.Count && elapsedTime > spreadTimes[currSpreadIdx])
        {
            int index = treesToSpread[currSpreadIdx];
            if (tg.IsHealthy(index))
            {
                GameObject nextfire = Instantiate(firePrefab, transform.parent);
                nextfire.GetComponent<FireBehaviourScript>().treeIndex = index;
                nextfire.GetComponent<FireBehaviourScript>().td = td;
                nextfire.GetComponent<FireBehaviourScript>().tg = tg;
                tg.SetBurning(index);
            }
            currSpreadIdx++;
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

        // Paint burnt spots
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
                modifiedAlphamap[i + burnXMinus, j + burnYMinus, grassTerrainLayer] *= 0.2f * (i * i + j * j);
                modifiedAlphamap[i + burnXMinus, j + burnYMinus, burntTerrainLayer] += 3.0f / (i*i + j*j + 1.0f);
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
            SetTreeDead();
            DestroyFire();
        }
    }
}
/**/