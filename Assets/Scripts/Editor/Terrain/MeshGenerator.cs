using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


public class MeshGenerator : EditorWindow
{
    Vector2 scrollPos;

    public Terrain terrain;
    public Material terrainMat;
    [Range(16,513)]
    public int dim;

    [MenuItem("Window/Terrain/Mesh Generator")]
    static void Init()
    {
        MeshGenerator window = (MeshGenerator)GetWindow(typeof(MeshGenerator));
        window.Show();
        window.titleContent = new GUIContent("Terrain Mesh Generator");
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
        terrain = Selection.activeGameObject.GetComponent<Terrain>();
        // Generate Mesh from heightmap
        // ---------------------------------------------------------------
        GUILayout.Label("Mesh Generation from Terrain Heightmap", EditorStyles.boldLabel);
        GUILayout.Label(terrain.name);

        ScriptableObject meshg = this;
        SerializedObject meshgso = new SerializedObject(meshg);


        SerializedProperty propDim = meshgso.FindProperty("dim");
        EditorGUILayout.PropertyField(propDim, new GUIContent("Heightmap dimensions:"), true);
        if (GUILayout.Button("Generate Mesh"))
        {
            GenerateMesh();
        }

        //SerializedProperty propMat = meshgso.FindProperty("terrainMat");
        //EditorGUILayout.PropertyField(propMat);
        if (GUILayout.Button("Generate Material"))
        {
            GenerateMaterial();
        }
        // ---------------------------------------------------------------
        GUILayout.EndScrollView();
        meshgso.ApplyModifiedProperties();
    }

    private void GenerateMesh()
    {
        TerrainData td = terrain.terrainData;
        Vector3[] vertices = new Vector3[dim * dim];
        Vector2[] uvs = new Vector2[dim * dim];
        int[] indices = new int[6 * (dim - 1) * (dim - 1)];

        // fill vertices + UVs
        for (int idx = 0, j = 0; j < dim; j++)
        {
            for (int i = 0; i < dim; i++, idx++)
            {
                uvs[idx] = new Vector2(i, j) / (dim - 1);
                Vector2 position = uvs[idx] * new Vector2(td.size.x, td.size.z);
                vertices[idx] = new Vector3(position.x, td.GetInterpolatedHeight(uvs[idx].x, uvs[idx].y), position.y);
            }
        }

        // fill indices/triangles
        // tidx is the index of the top left vertex in the quad of the indices array
        // vidx is the index of the top left vertex in the quad of the vertices array
        for (int tidx = 0, vidx = 0, j = 0; j < dim - 1; j++, vidx++)
        {
            for (int i = 0; i < dim - 1; i++, tidx += 6, vidx++)
            {
                // bottom left triangle CW
                indices[tidx + 0] = vidx;
                indices[tidx + 1] = vidx + dim;
                indices[tidx + 2] = vidx + dim + 1;
                // top right triangle CW
                indices[tidx + 3] = vidx + dim + 1;
                indices[tidx + 4] = vidx + 1;
                indices[tidx + 5] = vidx;
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = indices;
        mesh.name = "terrainMesh";
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        terrain.gameObject.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
        AssetDatabase.CreateAsset(mesh, "Assets/Prefabs/Terrain/TerrainMesh.asset");
    }

    void GenerateMaterial()
    {
        TerrainData td = terrain.terrainData;
        terrainMat = new Material(Shader.Find("Custom/TerrainFullShader"));
        terrainMat.SetTexture(Shader.PropertyToID("_Alphamap"), td.GetAlphamapTexture(0));
        terrainMat.SetInt("terrainLayers", td.terrainLayers.Length);
        for (int i = 0; i < td.terrainLayers.Length; i++)
        {
            terrainMat.SetTexture(Shader.PropertyToID("_diffuse" + i), td.terrainLayers[i].diffuseTexture);
        }
        terrain.gameObject.GetComponentInChildren<MeshRenderer>().material = terrainMat;
        AssetDatabase.CreateAsset(terrainMat, "Assets/Materials/Terrain/Terrain.mat");
    }
}
