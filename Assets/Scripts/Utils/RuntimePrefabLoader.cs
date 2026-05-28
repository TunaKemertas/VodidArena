using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Runtime project helper for this bootstrapped prototype.
/// In the editor it can load art prefabs directly from Assets/Prefabs.
/// In builds, put copies under Assets/Resources/Prefabs to load them.
/// </summary>
public static class RuntimePrefabLoader
{
    public static GameObject LoadPrefab(string prefabName)
    {
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
        if (prefab != null) return prefab;

#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{prefabName}.prefab");
#else
        return null;
#endif
    }
}

