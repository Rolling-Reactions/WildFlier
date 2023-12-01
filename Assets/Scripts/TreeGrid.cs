using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct TreeGrid
{
    TerrainData td;
    float cellSize;
    Vector2Int gridDims;
    Vector2Int gridSizeP1;
    List<int>[,] grid;


    public TreeGrid(TerrainData td, float cellSize)
    {
        this.td = td;
        this.cellSize = cellSize;
        gridDims = Vector2Int.CeilToInt(new Vector2(td.size.x, td.size.z) / cellSize);
        gridSizeP1 = gridDims + Vector2Int.one;
        grid = new List<int>[gridDims.x, gridDims.y];

        for (int i = 0; i < td.treeInstanceCount; i++)
        {
            Vector2Int gridPos = Tree2Grid(i);
            if (grid[gridPos.x, gridPos.y] == null)
                grid[gridPos.x, gridPos.y] = new List<int>();

            grid[gridPos.x, gridPos.y].Add(i);
        }
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
        List<int> neighbours = new List<int>(2 * td.treeInstanceCount / (gridDims.x * gridDims.y));

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

    public List<int> GetTreeWithinDistance(int treeIndex, float distance)
    {
        Vector2 treePos = Tree2Pos(treeIndex);
        Vector2Int gridPos = Tree2Grid(treeIndex);
        int searchMax = Mathf.CeilToInt(distance / cellSize);
        // Make capacity large enough so we hopefully don't need to resize list
        List<int> neighbours = new List<int>(2 * td.treeInstanceCount / (gridDims.x * gridDims.y));

        neighbours.AddRange(GetTrees(gridPos));
        neighbours.Remove(treeIndex);
        for (int j = -searchMax; j <= searchMax; j++)
        {
            for (int i = -searchMax; i <= searchMax; i++)
            {
                Vector2Int sampleGridPos = gridPos + new Vector2Int(i, j);
                if (!InGrid(sampleGridPos)) continue;
                foreach(int sampleTree in GetTrees(sampleGridPos))
                {
                    Vector2 sampleTreePos = Tree2Pos(sampleTree);
                    if ((sampleTreePos - treePos).sqrMagnitude > distance * distance)
                    {
                        neighbours.Add(sampleTree);
                    }
                }
            }
        }
        return neighbours;
    }

    public Vector2 Tree2Pos(int treeIndex)
    {
        Vector2 normPos = td.treeInstances[treeIndex].position;
        return normPos * new Vector2(td.size.x, td.size.z);
    }

    public Vector2Int Tree2Grid(int treeIndex)
    {
        Vector2 normPos = td.treeInstances[treeIndex].position;
        return Vector2Int.FloorToInt(normPos * (Vector2)gridDims);
    }
}
