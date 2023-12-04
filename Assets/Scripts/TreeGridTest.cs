using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGridTest : MonoBehaviour
{
    public TerrainData td;
    public int treeIndex = 0;
    public float cellSize = 20.0f;
    public float neighbourDist = 40.0f;
    private TreeGrid tg;

    void Start()
    {
        tg = new TreeGrid(td, cellSize);

        Vector2 pos = tg.Tree2Pos2D(treeIndex);
        Debug.LogFormat("Start pos: {0}", pos);
        List<int> nei0 = tg.GetTreesWithinDistance(treeIndex, neighbourDist);
        Debug.LogFormat("Neighbour count: {0}", nei0.Count);
        foreach(int nei in nei0)
        {
            Debug.LogFormat("{0} : {1}", nei, Vector2.Distance(pos, tg.Tree2Pos2D(nei)));
        }
    }    
}
