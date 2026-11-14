using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Arregla l'avis "The tree X must use the Nature/Soft Occlusion shader".
// Unity el mostra quan un arbre del Terrain es un model sense LODGroup.
// Aquesta eina crea, per a cada model d'arbre, un prefab amb LODGroup
// (a Assets/Prefabs/TerrainTrees) i el posa al Terrain en lloc del model.
// Afecta tots els TerrainData del projecte. Menu: Tools > Warped > Fix Terrain Trees
public static class TerrainTreeFixer
{
    private const string OutputFolder = "Assets/Prefabs/TerrainTrees";

    [MenuItem("Tools/Warped/Fix Terrain Trees")]
    public static void FixAllTerrainTrees()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
            AssetDatabase.Refresh();
        }

        Dictionary<GameObject, GameObject> converted = new Dictionary<GameObject, GameObject>();
        int changedPrototypes = 0;
        int changedTerrains = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:TerrainData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
            if (data == null) continue;

            TreePrototype[] prototypes = data.treePrototypes;
            bool changed = false;

            for (int i = 0; i < prototypes.Length; i++)
            {
                GameObject prefab = prototypes[i].prefab;
                if (prefab == null) continue;
                if (prefab.GetComponent<LODGroup>() != null) continue;

                if (!converted.TryGetValue(prefab, out GameObject lodPrefab))
                {
                    lodPrefab = CreateLodPrefab(prefab);
                    converted[prefab] = lodPrefab;
                }
                if (lodPrefab == null) continue;

                prototypes[i].prefab = lodPrefab;
                changed = true;
                changedPrototypes++;
            }

            if (changed)
            {
                data.treePrototypes = prototypes;
                data.RefreshPrototypes();
                EditorUtility.SetDirty(data);
                changedTerrains++;
            }
        }

        AssetDatabase.SaveAssets();

        foreach (Terrain t in Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
            t.Flush();

        Debug.Log($"[TerrainTreeFixer] {changedPrototypes} arbres actualitzats a {changedTerrains} terrenys. Prefabs a {OutputFolder}.");
    }

    private static GameObject CreateLodPrefab(GameObject model)
    {
        string prefabPath = $"{OutputFolder}/{model.name}_Tree.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null && existing.GetComponent<LODGroup>() != null) return existing;

        GameObject root = new GameObject(model.name);
        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
        if (visual == null) visual = Object.Instantiate(model);
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        LODGroup lodGroup = root.AddComponent<LODGroup>();
        lodGroup.SetLODs(new[] { new LOD(0.01f, renderers) });
        lodGroup.RecalculateBounds();

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return saved;
    }
}
