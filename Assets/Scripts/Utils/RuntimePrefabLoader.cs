using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Loads art prefabs for runtime-spawned objects.
/// APK/standalone builds only bundle assets under a Resources folder or referenced by a scene.
/// </summary>
public static class RuntimePrefabLoader
{
    static readonly Dictionary<string, GameObject> Cache = new Dictionary<string, GameObject>();
    static readonly HashSet<string> LoggedMissing = new HashSet<string>();

    public static GameObject LoadPrefab(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName)) return null;
        if (Cache.TryGetValue(prefabName, out GameObject cached) && cached != null)
            return cached;

        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
        if (prefab == null)
            prefab = Resources.Load<GameObject>($"Weapons/{prefabName}");

#if UNITY_EDITOR
        if (prefab == null)
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{prefabName}.prefab");
#endif

        if (prefab != null)
        {
            Cache[prefabName] = prefab;
            return prefab;
        }

        if (LoggedMissing.Add(prefabName))
        {
            Debug.LogError(
                $"Missing prefab '{prefabName}'. Assign it on GameBootstrapper in the Game scene, " +
                $"or place {prefabName}.prefab under Assets/Resources/Prefabs/.");
        }

        return null;
    }
}
