using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPreserver : MonoBehaviour
{

    TerrainData td1;
    TerrainData td2;


    void Awake()
    {

        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("No terrain on GameObject: " + gameObject);
            return;
        }

        td1 = terrain.terrainData;

        // This is the backup name/path of the cloned TerrainData.
        string tdBackupName = "Terrain/" + td1.name + " Backup";
        td2 = Resources.Load<TerrainData>(tdBackupName);
        if (td2 == null)
        {
            Debug.LogError("No TerrainData backup in a Resources folder, missing name is: " + tdBackupName);
            return;
        }

        // If blue screen, we still have to copy, for sure. It is a fast operation.
        resetTerrainDataChanges();
    }

    void OnApplicationQuit()
    {
        // To reset the terrain after quite the application.
        resetTerrainDataChanges();
    }

    void resetTerrainDataChanges()
    {
        // Terrain collider
        td1.SetHeights(0, 0, td2.GetHeights(0, 0, td1.heightmapResolution, td1.heightmapResolution));
        // Textures
        td1.SetAlphamaps(0, 0, td2.GetAlphamaps(0, 0, td1.alphamapWidth, td1.alphamapHeight));
        // Trees
        td1.treeInstances = td2.treeInstances;
    }
}

