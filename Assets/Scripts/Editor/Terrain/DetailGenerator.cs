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

    [Range(0.0f, 0.3f)]
    public float density;
    [Range(0.0f, 1.0f)]
    public float alphaThresh;
}

public class DetailGenerator : EditorWindow
{
    Vector2 scrollPos;

    public Terrain terrain;
    public int defaultTerrainLayer;
    [NonReorderable]
    public List<TerrainLayerOptions> terrainLayerOptions;

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
        }
    }

    void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        // Terrain Painter
        // ---------------------------------------------------------------
        GUILayout.Label("Procedural Terrain Painter", EditorStyles.boldLabel);

        ScriptableObject ter = this;
        SerializedObject terrainso = new SerializedObject(ter);
        SerializedProperty propTerrain = terrainso.FindProperty("terrain");
        EditorGUILayout.PropertyField(propTerrain, new GUIContent("Terrain Object:", "Place your terrain object in here."), true);

        SerializedProperty propDefaultTerrainLayer = terrainso.FindProperty("defaultTerrainLayer");
        EditorGUILayout.PropertyField(propDefaultTerrainLayer, new GUIContent("Default Terrain Layer:", "Place your terrain layer object in here."), true);

        SerializedProperty propTerrainLayers = terrainso.FindProperty("terrainLayerOptions");
        EditorGUILayout.PropertyField(propTerrainLayers, new GUIContent("Terrain Layers:", "Add/remove terrain layers"), true);

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
        EditorGUILayout.PropertyField(propTreeTypes, new GUIContent("Tree options:", "Add/remove trees"), true);

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

    private void SetTerrainLayers()
    {
        int layers = terrainLayerOptions.Count;
        TerrainData td = terrain.terrainData;
        TerrainLayer[] tl = new TerrainLayer[layers];
        for (int i = 0; i < layers; i++)
            tl[i] = terrainLayerOptions[i].terrainLayer;

        td.SetTerrainLayersRegisterUndo(tl, "tlundo");
    }

    private void PaintTerrain()
    {
        int layers = terrainLayerOptions.Count;
        TerrainData td = terrain.terrainData;

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
        int treeTypes = treeOptions.Count;
        TerrainData td = terrain.terrainData;

        TreePrototype[] treePrototypes = new TreePrototype[treeTypes];
        for (int i = 0; i < treeTypes; i++)
        {
            TreePrototype tp = new TreePrototype();
            tp.prefab = treeOptions[i].treePrefab;
            treePrototypes[i] = tp;
        }
        td.treePrototypes = treePrototypes;
    }

    private void GenerateTrees()
    {
        int layers = terrainLayerOptions.Count;
        int treeTypes = treeOptions.Count;
        TerrainData td = terrain.terrainData;

        Vector2 posToTile = new Vector2(td.alphamapWidth, td.alphamapHeight) / (Vector2)td.size;
        float alphamapTileArea = ((Vector2)td.size / new Vector2(td.alphamapWidth, td.alphamapHeight)).magnitude;

        List<TreeInstance> tis = new List<TreeInstance>();
        for (int t = 0; t < treeTypes; t++)
        {
            TreeOptions to = treeOptions[t];

            float area = 0.0f;
            for (int i = 0; i < td.alphamapWidth; i++)
            {
                for (int j = 0; j < td.alphamapHeight; j++)
                {
                    foreach (int layer in to.includeLayers)
                    {
                        if (td.GetAlphamaps(i, j, 1, 1)[0, 0, layer] >= to.alphaThresh)
                        {
                            area += alphamapTileArea;
                            break;
                        }
                    }
                }
            }

            int nTrees = (int)(to.density * area);
            TreeInstance[] ptis = new TreeInstance[nTrees];
            for (int i = 0; i < nTrees; i++)
            {
                while (true)
                {
                    Vector2 position = new Vector2(Random.value, Random.value) * (Vector2)td.size;
                    Vector3 normPos = position / td.size;
                    normPos.z = normPos.y;
                    Vector2Int tile = Vector2Int.RoundToInt(position * posToTile);
                    foreach (int layer in to.includeLayers)
                    {
                        if (td.GetAlphamaps(tile.x, tile.y, 1, 1)[0, 0, layer] > to.alphaThresh)
                        {
                            TreeInstance ti = new TreeInstance();
                            ti.position = normPos;
                            ti.widthScale = 1;
                            ti.heightScale = 1;
                            ti.rotation = Random.value * Mathf.PI;
                            ti.prototypeIndex = t;
                            ptis[i] = ti;
                            goto Placed;
                        }
                    }
                }
            Placed:
                continue;
            }
            Debug.LogFormat("Created {0} tree instances of type {1}", nTrees, to.treePrefab.name);
            tis.AddRange(ptis);
        }
        td.SetTreeInstances(tis.ToArray(), true);
        Debug.LogFormat("Placed {0} trees", td.treeInstanceCount);
    }
}
