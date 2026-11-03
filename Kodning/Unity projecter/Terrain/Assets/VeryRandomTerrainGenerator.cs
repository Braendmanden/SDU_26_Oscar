using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class VeryRandomTerrainGenerator : ScriptableWizard
{

    [Range(0.1f,1)] public float maxHeight = 1;

    public int seed;
    public bool useRandomSeed = false;

    [MenuItem("Terrain/Generate Very Random Terrain")]
    public static void CreateWizard(MenuCommand command)
    {
        ScriptableWizard.DisplayWizard("Generate Very Random Terrain", typeof(VeryRandomTerrainGenerator));
    }

    private void OnEnable()
    {

    }

    void OnWizardCreate()
    {
        GameObject G = Selection.activeGameObject;
        if (G.GetComponent<Terrain>())
        {
            GenerateTerrain(G.GetComponent<Terrain>());
        }
    }

    //Our Generate Terrain function
    public void GenerateTerrain(Terrain t)
    {
        if (!useRandomSeed)
            Random.InitState(seed); 

        //Heights For Our Hills/Mountains
        float[,] hts = new float[t.terrainData.heightmapResolution, t.terrainData.heightmapResolution];
        for (int i = 0; i < t.terrainData.heightmapResolution; i++)
        {
            for (int k = 0; k < t.terrainData.heightmapResolution; k++)
            {
                hts[i, k] = Random.Range(0f,1f) * maxHeight;
            }
        }
        t.terrainData.SetHeights(0, 0, hts);
    }
}