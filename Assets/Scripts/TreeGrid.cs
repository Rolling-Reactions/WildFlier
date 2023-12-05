using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TreeStatus
{
    Healthy, Burning, Dead
}

public enum TreePrototypesIndex
{
    Fir, Oak, Dead
}

public struct TreeGrid
{
    private TerrainData td;
    private float cellSize;
    private Vector2Int gridDims;
    private List<int>[,] grid;
    private TreeStatus[] treeStatus;


    public TreeGrid(TerrainData td, float cellSize)
    {
        this.td = td;
        this.cellSize = cellSize;
        gridDims = Vector2Int.CeilToInt(new Vector2(td.size.x, td.size.z) / cellSize);
        grid = new List<int>[gridDims.x, gridDims.y];
        treeStatus = new TreeStatus[td.treeInstanceCount];

        for (int i = 0; i < td.treeInstanceCount; i++)
        {
            treeStatus[i] = TreeStatus.Healthy;
            Vector2Int gridPos = Tree2Grid(i);
            if (grid[gridPos.x, gridPos.y] == null)
                grid[gridPos.x, gridPos.y] = new List<int>();

            grid[gridPos.x, gridPos.y].Add(i);
        }
    }

    public TreeStatus GetTreeStatus(int treeIndex) { return treeStatus[treeIndex]; }
    public bool IsHealthy(int treeIndex) { return treeStatus[treeIndex] == TreeStatus.Healthy; }
    public bool IsBurning(int treeIndex) { return treeStatus[treeIndex] == TreeStatus.Burning; }
    public bool IsDead(int treeIndex) { return treeStatus[treeIndex] == TreeStatus.Dead; }
    public void SetHealthy(int treeIndex) { treeStatus[treeIndex] = TreeStatus.Healthy; }
    public void SetBurning(int treeIndex) { treeStatus[treeIndex] = TreeStatus.Burning; }
    public void SetDead(int treeIndex) {
        treeStatus[treeIndex] = TreeStatus.Dead;

        // We cannot set prototypeIndex so we just set height to 0 to make it invisible,
        // and replace it with dead tree prefab in the script that calls this
        TreeInstance ti = td.GetTreeInstance(treeIndex);
        ti.heightScale = 0;
        td.SetTreeInstance(treeIndex, ti);
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
                if (GetTrees(sampleGridPos) == null) continue;
                neighbours.AddRange(GetTrees(sampleGridPos));
                if (i == 0 && j == 0)
                {
                    neighbours.Remove(treeIndex);
                }
            }
        }
        return neighbours;
    }

    // Note: takes 2D distance projected on xz-plane
    public List<int> GetTreesWithinDistance(int treeIndex, float distance)
    {
        Vector2 treePos = Tree2Pos2D(treeIndex);
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
                    Vector2 sampleTreePos = Tree2Pos2D(sampleTree);
                    if ((sampleTreePos - treePos).sqrMagnitude < distance * distance && sampleTree != treeIndex)
                    {
                        neighbours.Add(sampleTree);
                    }
                }
            }
        }
        return neighbours;
    }

    // Note: takes 2D distance projected on xz-plane
    public List<int> GetTreesWithinDistance(Vector3 worldPos, float distance)
    {
        return GetTreesWithinDistance(new Vector2(worldPos.x, worldPos.z), distance);
    }

    // Note: takes 2D distance projected on xz-plane
    public List<int> GetTreesWithinDistance(Vector2 worldPos, float distance)
    {
        Vector2Int gridPos = Pos2Grid(worldPos);
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
                    Vector2 sampleTreePos = Tree2Pos2D(sampleTree);
                    if ((sampleTreePos - worldPos).sqrMagnitude < distance * distance)
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
        Vector2 treePos = Tree2Pos2D(treeIndex);
        List<int> neighbours = GetDirectNeighbours(treeIndex);
        float minDist = float.MaxValue;
        int nn = -1;
        foreach (int n in neighbours)
        {
            Vector2 nPos = Tree2Pos2D(n);
            float distSq = (treePos - nPos).sqrMagnitude;
            if (distSq < minDist * minDist)
            {
                minDist = Mathf.Sqrt(distSq);
                nn = n;
            }
        }
        return nn;
    }

    public Vector2 Tree2NormPos2D(int treeIndex)
    {
        return new Vector2(td.GetTreeInstance(treeIndex).position.x, td.GetTreeInstance(treeIndex).position.z);
    }

    public Vector3 Tree2NormPos(int treeIndex)
    {
        return td.GetTreeInstance(treeIndex).position;
    }

    public Vector2 Pos2NormPos(Vector2 pos)
    {
        return new Vector2(pos.x, pos.y) / new Vector2(td.size.x, td.size.z);
    }

    public Vector2 Pos2NormPos(Vector3 pos)
    {
        return new Vector2(pos.x, pos.z) / new Vector2(td.size.x, td.size.z);
    }

    public Vector2 Tree2Pos2D(int treeIndex)
    {
        return Tree2NormPos2D(treeIndex) * new Vector2(td.size.x, td.size.z);
    }

    public Vector3 Tree2Pos(int treeIndex)
    {
        return Vector3.Scale(Tree2NormPos(treeIndex), td.size);
    }

    public Vector2Int Tree2Grid(int treeIndex)
    {
        return Vector2Int.FloorToInt(Tree2NormPos2D(treeIndex) * (Vector2)gridDims);
    }

    public Vector2Int Pos2Grid(Vector3 pos)
    {
        return Vector2Int.FloorToInt(Pos2NormPos(pos) * (Vector2)gridDims);
    }

    public Vector2Int Pos2Grid(Vector2 pos)
    {
        return Vector2Int.FloorToInt(Pos2NormPos(pos) * (Vector2)gridDims);
    }
}
