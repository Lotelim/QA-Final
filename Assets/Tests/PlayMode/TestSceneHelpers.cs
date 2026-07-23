using System.Collections.Generic;
using UnityEngine;

/// <summary>Shared setup/teardown helpers for PlayMode tests that need a configured main camera or disposable placeholder GameObjects.</summary>
public static class TestSceneHelpers
{
    public static Camera CreateMainCamera(List<GameObject> spawned)
    {
        var camGO = new GameObject("MainCamera");
        spawned.Add(camGO);
        var cam = camGO.AddComponent<Camera>();
        camGO.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGO.transform.position = new Vector3(0, 0, -10);
        return cam;
    }

    public static GameObject CreatePlaceholder(List<GameObject> spawned, string name = "FX")
    {
        var go = new GameObject(name);
        spawned.Add(go);
        return go;
    }

    public static void DestroyAll(List<GameObject> spawned)
    {
        foreach (GameObject go in spawned)
            if (go != null)
                Object.Destroy(go);
        spawned.Clear();
    }
}
