using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct TreeGrid
{
    public TerrainData td;
    public float cellSize;
    public Vector2Int gridDims;
    public List<int>[,] grid;


    public TreeGrid(TerrainData td, float cellSize)
    {
        this.td = td;
        this.cellSize = cellSize;
        gridDims = Vector2Int.CeilToInt(new Vector2(td.size.x, td.size.z) / cellSize);
        grid = new List<int>[gridDims.x, gridDims.y];

        for (int i = 0; i < td.treeInstanceCount; i++)
        {
            Vector2Int gridPos = Tree2Grid(i);
            if (grid[gridPos.x, gridPos.y] == null)
                grid[gridPos.x, gridPos.y] = new List<int>();

            grid[gridPos.x, gridPos.y].Add(i);
        }
    }

    public bool HasTrees(Vector2Int gridPos)
    {
        return grid[gridPos.x, gridPos.y] != null;
    }
    public List<int> GetTrees(Vector2Int gridPos)
    {
        return grid[gridPos.x, gridPos.y];
    }

    public int GetTreeCount(Vector2Int gridPos)
    {
        return grid[gridPos.x, gridPos.y].Count;
    }

    private bool InGrid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < gridDims.x && gridPos.y < gridDims.y;
    }

    public List<int> GetDirectNeighbours(int treeIndex)
    {
        Vector2Int gridPos = Tree2Grid(treeIndex);
        // Make capacity large enough so we hopefully don't need to resize list
        List<int> neighbours = new List<int>(9 * td.treeInstanceCount / (gridDims.x * gridDims.y));

        for (int j = -1; j <= 1; j++)
        {
            for (int i = -1; i <= 1; i++)
            {
                Vector2Int sampleGridPos = gridPos + new Vector2Int(i, j);
                if (!InGrid(sampleGridPos)) continue;
                neighbours.AddRange(GetTrees(sampleGridPos));
                if (i == 0 && j == 0)
                {
                    neighbours.Remove(treeIndex);
                }
            }
        }
        return neighbours;
    }

    public List<int> GetTreesWithinDistance(int treeIndex, float distance)
    {
        Vector2 treePos = Tree2Pos(treeIndex);
        Vector2Int gridPos = Tree2Grid(treeIndex);
        int searchMax = Mathf.CeilToInt(distance / cellSize);
        // Make capacity large enough so we hopefully don't need to resize list
        List<int> neighbours = new List<int>((searchMax + 1) * (searchMax + 1) * td.treeInstanceCount / (gridDims.x * gridDims.y));

        for (int j = -searchMax; j <= searchMax; j++)
        {
            for (int i = -searchMax; i <= searchMax; i++)
            {
                Vector2Int sampleGridPos = gridPos + new Vector2Int(i, j);
                if (!InGrid(sampleGridPos) || !HasTrees(sampleGridPos)) continue;
                foreach (int sampleTree in GetTrees(sampleGridPos))
                {
                    Vector2 sampleTreePos = Tree2Pos(sampleTree);
                    if ((sampleTreePos - treePos).sqrMagnitude < distance * distance && sampleTree != treeIndex)
                    {
                        neighbours.Add(sampleTree);
                    }
                }
            }
        }
        return neighbours;
    }

    // Only correct if there are trees in the nearest neighbouring cell,
    // more complicated implementation for full correctness but should work well enough for our density
    // Make sure to keep cellSize > poissonDiskRadius at least
    public int GetNearestTree(int treeIndex)
    {
        Vector2 treePos = Tree2Pos(treeIndex);
        List<int> neighbours = GetDirectNeighbours(treeIndex);
        float minDist = float.MaxValue;
        int nn = -1;
        foreach (int n in neighbours)
        {
            Vector2 nPos = Tree2Pos(n);
            float distSq = (treePos - nPos).sqrMagnitude;
            if (distSq < minDist * minDist)
            {
                minDist = Mathf.Sqrt(distSq);
                nn = n;
            }
        }
        return nn;
    }

    public Vector2 Tree2NormPos(int treeIndex)
    {
        return new Vector2(td.treeInstances[treeIndex].position.x, td.treeInstances[treeIndex].position.z);
    }

    public Vector2 Tree2Pos(int treeIndex)
    {
        return Tree2NormPos(treeIndex) * new Vector2(td.size.x, td.size.z);
    }

    public Vector2Int Tree2Grid(int treeIndex)
    {
        return Vector2Int.FloorToInt(Tree2NormPos(treeIndex) * (Vector2)gridDims);
    }
}
