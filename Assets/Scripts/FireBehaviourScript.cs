

//// td.GetTreeInstances

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FireBehaviourScript : MonoBehaviour
//{
//    public TerrainData terrainData;
//    private TreeInstance[] trees;
//    private bool[] isTreeOnFire;

//    // Start is called before the first frame update
//    void Start() 
//    {
//        trees = terrainData.treeInstances;
//        isTreeOnFire = new bool[trees.Length];

//        // Choose a random tree to start the fire
//        int randomTree = Random.Range(0, trees.Length);
//        StartFire(randomTree);
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        // Check for spreading fire
//        for (int i = 0; i < trees.Length; i++)
//        {
//            if (isTreeOnFire[i])
//            {
//                // SpreadFire(i);
//            }
//        }
//    }

//    void StartFire(int treeIndex)
//    {
//        isTreeOnFire[treeIndex] = true;

//        trees[treeIndex].color = Color.red; // You can change the tree color to represent it's on fire
//        StartCoroutine(SpreadFireAfterDelay(treeIndex, 10f)); // Spread fire after 10 seconds
//    }

//    /*
//    void SpreadFire(int sourceTreeIndex)
//    {
//        Vector3 sourcePosition = terrainData.GetTreeInstancePosition(sourceTreeIndex);

//        for (int i = 0; i < trees.Length; i++)
//        {
//            if (i != sourceTreeIndex && !isTreeOnFire[i])
//            {
//                Vector3 targetPosition = terrainData.GetTreeInstancePosition(i);
//                float distance = Vector3.Distance(sourcePosition, targetPosition);

//                // Check if the tree is within the specified radius
//                if (distance <= 5f)
//                {
//                    // Probability of catching fire can be adjusted
//                    if (Random.Range(0f, 1f) < 0.1f)
//                    {
//                        StartFire(i);
//                    }
//                }
//            }
//        }
//    }*/

//    IEnumerator SpreadFireAfterDelay(int treeIndex, float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        SpreadFire(treeIndex);
//    }
//}

///**/