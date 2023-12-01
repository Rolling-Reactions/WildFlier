using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct TreeGrid
{
    public static readonly Vector2Int[] neighbourOffsets =
    {
        new Vector2Int(1,0),
        new Vector2Int(1,1),
        new Vector2Int(0, 1),
        new Vector2Int(-1,1),
        new Vector2Int(-1,0),
        new Vector2Int(-1,-1),
        new Vector2Int(0,-1),
        new Vector2Int(1,-1)

    };
    TerrainData td;
    float cellSize;
    Vector2Int gridSize;
    Vector2Int gridSizeP1;
    List<int>[,] grid;


    public TreeGrid(TerrainData td, float cellSize)
    {
        this.td = td;
        this.cellSize = cellSize;
        gridSize = Vector2Int.CeilToInt(new Vector2(td.size.x, td.size.z) / cellSize);
        gridSizeP1 = gridSize + Vector2Int.one;
        grid = new List<int>[gridSize.x, gridSize.y];

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

    public List<int> GetNeighbours(int treeIndex)
    {
        Vector2Int gridPos = Tree2Grid(treeIndex);
        // Make capacity large enough so we hopefully don't need to resize list
        List<int> neighbours = new List<int>(2 * td.treeInstanceCount / (gridSize.x * gridSize.y));

        neighbours.AddRange(GetTrees(gridPos));
        neighbours.Remove(treeIndex);
        foreach (var no in neighbourOffsets)
        {
            neighbours.AddRange(GetTrees(gridPos + no));
        }
        return neighbours;
    }

    public Vector2Int Tree2Grid(int treeIndex)
    {
        Vector3 pos = td.treeInstances[treeIndex].position;
        Vector2 normPos = new Vector2(pos.x, pos.y) / new Vector2(td.size.x, td.size.z);
        return Vector2Int.FloorToInt(normPos * (Vector2)gridSize);
    }
}
