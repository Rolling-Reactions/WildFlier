using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[System.Serializable]
public struct TerrainLayerOptions
{
    public TerrainLayer terrainLayer;
    [Range(0.0f, 1000.0f)]
    public float minHeight, maxHeight;
    [Range(0.0f, 90.0f)]
    public float minSteepness, maxSteepness;
    [Range(0.0f, 50.0f)]
    public float blendDist;
}

[System.Serializable]
public struct TreeOptions
{
    public GameObject treePrefab;
    public List<int> includeLayers;

    [Range(0.0f, 1.0f)]
    public float alphaThresh;
    public float minHeightScale;
    public float maxHeightScale;
}

public class DetailGenerator : EditorWindow
{
    Vector2 scrollPos;

    public Terrain terrain;
    private TerrainData td;
    private Vector2 terrainSize;

    public int defaultTerrainLayer;
    [NonReorderable]
    public List<TerrainLayerOptions> terrainLayerOptions;

    public int poissonMaxIters = 10000;
    public float poissonDiskRadius;
    private float poissonDiskRadiusSq;
    public int poissonK;
    private float poissonCellSize;
    private Vector2Int poissonGridDims;
    [NonReorderable]
    public List<TreeOptions> treeOptions;

    [MenuItem("Window/Terrain/Detail Generator")]
    static void Init()
    {
        DetailGenerator window = (DetailGenerator)GetWindow(typeof(DetailGenerator));
        window.Show();
        window.titleContent = new GUIContent("Terrain Detail Generator");
        window.Focus();
        window.ShowUtility();
        if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<Terrain>())
        {
            window.terrain = Selection.activeGameObject.GetComponent<Terrain>();
            window.td = window.terrain.terrainData;
        }
    }

    void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        ScriptableObject ter = this;
        SerializedObject terrainso = new SerializedObject(ter);
        SerializedProperty propTerrain = terrainso.FindProperty("terrain");
        EditorGUILayout.PropertyField(propTerrain, new GUIContent("Terrain Object", "Place your terrain object in here."), true);

        // Terrain Painter
        // ---------------------------------------------------------------
        GUILayout.Label("Procedural Terrain Painter", EditorStyles.boldLabel);

        SerializedProperty propDefaultTerrainLayer = terrainso.FindProperty("defaultTerrainLayer");
        EditorGUILayout.PropertyField(propDefaultTerrainLayer, new GUIContent("Default Terrain Layer", "Place your terrain layer object in here."), true);

        SerializedProperty propTerrainLayers = terrainso.FindProperty("terrainLayerOptions");
        EditorGUILayout.PropertyField(propTerrainLayers, new GUIContent("Terrain Layers", "Add/remove terrain layers"), true);

        if (GUILayout.Button("Set Terrain Layers"))
        {
            SetTerrainLayers();
        }

        if (GUILayout.Button("Paint Terrain"))
        {
            PaintTerrain();
        }
        // ---------------------------------------------------------------

        // Procedural Vegetation
        // ---------------------------------------------------------------
        GUILayout.Label("Procedural Vegetation Placer", EditorStyles.boldLabel);

        SerializedProperty propTreeTypes = terrainso.FindProperty("treeOptions");
        EditorGUILayout.PropertyField(propTreeTypes, new GUIContent("Tree options", "Add/remove trees"), true);
        SerializedProperty propK = terrainso.FindProperty("poissonK");
        EditorGUILayout.PropertyField(propK, new GUIContent("Poisson k"), true);
        SerializedProperty propRad = terrainso.FindProperty("poissonDiskRadius");
        EditorGUILayout.PropertyField(propRad, new GUIContent("Poisson disk rad"), true);
        SerializedProperty propIters = terrainso.FindProperty("poissonMaxIters");
        EditorGUILayout.PropertyField(propIters, new GUIContent("Max poisson iters"), true);

        if (GUILayout.Button("Set Tree Prototypes"))
        {
            SetTreePrototypes();
        }
        if (GUILayout.Button("Generate Trees"))
        {
            GenerateTrees();
        }
        // ---------------------------------------------------------------
        GUILayout.EndScrollView();
        terrainso.ApplyModifiedProperties();
    }

    private void UpdateTerrainVars()
    {
        td = terrain.terrainData;
        terrainSize = new Vector2(td.size.x, td.size.z);
    }

    private void SetTerrainLayers()
    {
        UpdateTerrainVars();
        int layers = terrainLayerOptions.Count;
        TerrainLayer[] tl = new TerrainLayer[layers];
        for (int i = 0; i < layers; i++)
            tl[i] = terrainLayerOptions[i].terrainLayer;

        td.SetTerrainLayersRegisterUndo(tl, "tlundo");
    }

    private void PaintTerrain()
    {
        UpdateTerrainVars();
        int layers = terrainLayerOptions.Count;

        float[,,] splatmap = new float[td.alphamapWidth, td.alphamapHeight, layers];
        Vector2 delta = new Vector2(1.0f / (td.alphamapWidth - 1), 1.0f / (td.alphamapHeight - 1));
        for (int i = 0; i < td.alphamapWidth; i++)
        {
            for (int j = 0; j < td.alphamapHeight; j++)
            {
                bool textureSet = false;
                Vector2 normCoords = new Vector2(i, j) * delta;
                float height = td.GetInterpolatedHeight(normCoords.y, normCoords.x);
                float steepness = td.GetSteepness(normCoords.y, normCoords.x);

                for (int layer = 0; layer < layers; layer++)
                {
                    TerrainLayerOptions tlo = terrainLayerOptions[layer];

                    if (height >= tlo.minHeight && height <= tlo.maxHeight &&
                        steepness >= tlo.minSteepness && steepness <= tlo.maxSteepness)
                    {
                        float heightBoundaryDist = Mathf.Min(height - tlo.minHeight, tlo.maxHeight - height);

                        splatmap[i, j, layer] = (heightBoundaryDist < tlo.blendDist) ? heightBoundaryDist / tlo.blendDist : 1.0f;
                        textureSet = true;
                    }
                }
                if (!textureSet)
                {
                    splatmap[i, j, defaultTerrainLayer] = 1.0f;
                }
            }
        }
        td.SetAlphamaps(0, 0, splatmap);
    }

    private void SetTreePrototypes()
    {
        UpdateTerrainVars();
        int treeTypes = treeOptions.Count;

        TreePrototype[] treePrototypes = new TreePrototype[treeTypes];
        for (int i = 0; i < treeTypes; i++)
        {
            TreePrototype tp = new TreePrototype
            {
                prefab = treeOptions[i].treePrefab
            };
            treePrototypes[i] = tp;
        }

        td.treePrototypes = treePrototypes;
    }

    private TreeInstance GenerateTreeInstance(Vector2 normPos, int pindex)
    {
        TreeOptions to = treeOptions[pindex];
        float scale = Random.Range(to.minHeightScale, to.maxHeightScale);
        TreeInstance ti = new TreeInstance
        {
            position = new Vector3(normPos.x, 0, normPos.y),
            widthScale = scale,
            heightScale = scale,
            rotation = Random.value * Mathf.PI,
            prototypeIndex = pindex
        };

        return ti;
    }

    private Vector2 GetNormPos(Vector2 pos)
    {
        return pos / terrainSize;
    }
    private Vector2Int NormPos2AlphamapCoord(Vector2 normPos)
    {
        return Vector2Int.RoundToInt(normPos * new Vector2(td.alphamapWidth - 1, td.alphamapHeight - 1));
    }

    private Vector2 GenerateSamplePoint()
    {
        Vector2 normPos;
        while (true)
        {
            normPos = new Vector2(Random.value, Random.value);
            if (IsAnyPrototypeAvailableAtPoint(normPos))
                return normPos;
        }
    }

    private bool IsPrototypeAvailableAtPoint(Vector2 normPos, int pidx)
    {
        TreeOptions to = treeOptions[pidx];
        Vector2Int ac = NormPos2AlphamapCoord(normPos);
        foreach (int layer in to.includeLayers)
        {
            if (to.alphaThresh < td.GetAlphamaps(ac.x, ac.y, 1, 1)[0, 0, layer])
                return true;
        }
        return false;
    }

    private bool IsAnyPrototypeAvailableAtPoint(Vector2 normPos)
    {
        for (int i = 0; i < treeOptions.Count; i++)
        {
            if (IsPrototypeAvailableAtPoint(normPos, i))
            {
                return true;
            }
        }
        return false;
    }

    private int RandomValidTreePrototypeIndexAtPoint(Vector2 normPos)
    {
        List<int> validTrees = new List<int>(treeOptions.Count);
        for (int i = 0; i < treeOptions.Count; i++)
        {
            if (IsPrototypeAvailableAtPoint(normPos, i))
                validTrees.Add(i);
        }

        if (validTrees.Count > 0)
        {
            return validTrees[Random.Range(0, validTrees.Count)];
        }
        else
        {
            return -1;
        }
    }

    private Vector2Int NormPos2PoissonGrid(Vector2 normPos)
    {
        return Vector2Int.FloorToInt(normPos * poissonGridDims);
    }

    private bool inPoissonGrid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < poissonGridDims.x && gridPos.y < poissonGridDims.y;
    }

    private bool checkPointValidity(Vector2 normPos, int[,] grid, List<TreeInstance> tis)
    {
        // test against grid
        Vector2Int gridPos = NormPos2PoissonGrid(normPos);
        for (int j = -2; j <= 2; j++)
        {
            for (int i = -2; i <= 2; i++)
            {
                Vector2Int sampleGridPos = gridPos + new Vector2Int(i, j);
                if (!inPoissonGrid(sampleGridPos)) continue;

                int treeidx = grid[sampleGridPos.x, sampleGridPos.y];
                if (treeidx >= 0)
                {
                    Vector2 samplePos = new Vector2(tis[treeidx].position.x, tis[treeidx].position.z) * terrainSize;
                    Vector2 pos = normPos * terrainSize;
                    if ((pos - samplePos).sqrMagnitude < poissonDiskRadiusSq) return false;
                }
            }
        }

        // test against terrain
        if (!IsAnyPrototypeAvailableAtPoint(normPos))
        {
            return false;
        }

        return true;
    }

    private void GenerateTrees()
    {
        UpdateTerrainVars();

        poissonDiskRadiusSq = poissonDiskRadius * poissonDiskRadius;
        poissonCellSize = poissonDiskRadius / Mathf.Sqrt(2);
        poissonGridDims = Vector2Int.CeilToInt(terrainSize / poissonCellSize);
        Debug.Log("Creating Poisson grid of size " + poissonGridDims);
        int[,] poissonGrid = new int[poissonGridDims.x, poissonGridDims.y];
        for (int j = 0; j < poissonGridDims.y; j++)
        {
            for (int i = 0; i < poissonGridDims.x; i++)
            {
                poissonGrid[i, j] = -1;
            }
        }

        int capacity = (int)Mathf.Ceil(terrainSize.x * terrainSize.y / (poissonDiskRadius * poissonDiskRadius));
        List<int> activeList = new List<int>(capacity);
        List<TreeInstance> tis = new List<TreeInstance>(capacity);


        Vector2 x0 = GenerateSamplePoint();
        Vector2Int x0grid = NormPos2PoissonGrid(x0);

        activeList.Add(0);
        poissonGrid[x0grid.x, x0grid.y] = 0;
        tis.Add(GenerateTreeInstance(x0, RandomValidTreePrototypeIndexAtPoint(x0)));
        Debug.LogFormat("Started with initial point {0}", x0);

        int iters = 0;
        while (activeList.Count > 0 && iters < poissonMaxIters)
        {
            int active = activeList[Random.Range(0, activeList.Count)];
            Vector2 activePos = new Vector2(tis[active].position.x, tis[active].position.z) * terrainSize;
            Vector2 currPoint;

            for (int i = 0; i < poissonK; i++)
            {
                float r = Random.Range(poissonDiskRadius, 2 * poissonDiskRadius);
                float theta = Random.Range(0, 2 * Mathf.PI);
                currPoint = GetNormPos(activePos + r * new Vector2(Mathf.Sin(theta), Mathf.Cos(theta)));
                if (checkPointValidity(currPoint, poissonGrid, tis))
                {
                    Vector2Int currGrid = NormPos2PoissonGrid(currPoint);
                    activeList.Add(tis.Count);
                    poissonGrid[currGrid.x, currGrid.y] = tis.Count;
                    tis.Add(GenerateTreeInstance(currPoint, RandomValidTreePrototypeIndexAtPoint(x0)));
                    break;
                }
                if (i == poissonK - 1) activeList.Remove(active);
            }

            iters++;
        }
        td.SetTreeInstances(tis.ToArray(), true);
        Debug.LogFormat("Placed {0} trees", td.treeInstanceCount);
        Debug.LogFormat("Finished after {0} iterations", iters);
        Debug.LogFormat("Active list contains {0} points", activeList.Count);
        TreeGrid tg = new TreeGrid(td, 30.0f);

    }
}
