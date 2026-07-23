using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-off content-authoring tool: builds Level_1 (a straight adaptation of Demo_Scene, wired
/// with level-completion tracking) and Level_2 (harder timings, a shielded-enemy wave, and a
/// boss), plus the prefab variants they need. Uses real Editor APIs (AssetDatabase,
/// PrefabUtility, EditorSceneManager) rather than hand-edited YAML so Unity itself keeps every
/// GUID/fileID reference valid. Safe to re-run - it rebuilds its generated assets from scratch
/// each time rather than mutating them in place.
/// </summary>
public static class LevelContentBuilder
{
    const string ScenesFolder = "Assets/Space Shooter Template FREE/Scenes";
    const string PrefabsFolder = "Assets/Space Shooter Template FREE/Prefabs";
    const string SpritesFolder = "Assets/Space Shooter Template FREE/Sprites";

    const string DemoScenePath = ScenesFolder + "/Demo_Scene.unity";
    const string Level1Path = ScenesFolder + "/Level_1.unity";
    const string Level2Path = ScenesFolder + "/Level_2.unity";

    const string BaseEnemyPath = PrefabsFolder + "/Enemies/Enemy_straight_projectile.prefab";
    const string ShieldedEnemyPath = PrefabsFolder + "/Enemies/Enemy_shielded.prefab";
    const string BaseWavePath = PrefabsFolder + "/EnemyWaves/Wave_1.prefab";
    const string ShieldedWavePath = PrefabsFolder + "/EnemyWaves/Wave_Shielded.prefab";
    const string BossPath = PrefabsFolder + "/Enemies/Boss.prefab";
    const string EnemySpritePath = SpritesFolder + "/Enemies/Enemy_01.png";

    [MenuItem("Tools/QA Final/Build Level 1 and Level 2 Content")]
    public static void BuildLevels()
    {
        GameObject shieldedEnemy = BuildShieldedEnemyPrefab();
        GameObject shieldedWave = BuildShieldedWavePrefab(shieldedEnemy);
        GameObject boss = BuildBossPrefab();

        BuildLevel1();
        BuildLevel2(shieldedWave, boss);

        AddScenesToBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[LevelContentBuilder] Level_1 and Level_2 content build complete.");
    }

    static void DeleteIfExists(string path)
    {
        if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            AssetDatabase.DeleteAsset(path);
    }

    // ---- Prefabs ----

    static GameObject BuildShieldedEnemyPrefab()
    {
        DeleteIfExists(ShieldedEnemyPath);
        AssetDatabase.CopyAsset(BaseEnemyPath, ShieldedEnemyPath);

        GameObject contents = PrefabUtility.LoadPrefabContents(ShieldedEnemyPath);
        var shield = contents.AddComponent<Shield>();
        shield.shieldHealth = 3;
        PrefabUtility.SaveAsPrefabAsset(contents, ShieldedEnemyPath);
        PrefabUtility.UnloadPrefabContents(contents);

        return AssetDatabase.LoadAssetAtPath<GameObject>(ShieldedEnemyPath);
    }

    static GameObject BuildShieldedWavePrefab(GameObject shieldedEnemyPrefab)
    {
        DeleteIfExists(ShieldedWavePath);
        AssetDatabase.CopyAsset(BaseWavePath, ShieldedWavePath);

        GameObject contents = PrefabUtility.LoadPrefabContents(ShieldedWavePath);
        Wave wave = contents.GetComponent<Wave>();
        wave.enemy = shieldedEnemyPrefab;
        wave.count = 4;
        wave.timeBetween = 0.6f;
        PrefabUtility.SaveAsPrefabAsset(contents, ShieldedWavePath);
        PrefabUtility.UnloadPrefabContents(contents);

        return AssetDatabase.LoadAssetAtPath<GameObject>(ShieldedWavePath);
    }

    static GameObject BuildBossPrefab()
    {
        DeleteIfExists(BossPath);

        GameObject baseEnemyAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BaseEnemyPath);
        Enemy baseEnemy = baseEnemyAsset.GetComponent<Enemy>();
        var baseCollider = baseEnemyAsset.GetComponent<CircleCollider2D>();
        var baseSpriteRenderer = baseEnemyAsset.GetComponent<SpriteRenderer>();

        var root = new GameObject("Boss");
        try
        {
            root.tag = "Enemy";

            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var spriteRenderer = root.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(EnemySpritePath);
            spriteRenderer.color = new Color(1f, 0.35f, 0.35f); // tint red so it reads as distinct from regular enemies
            spriteRenderer.sortingLayerID = baseSpriteRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = baseSpriteRenderer.sortingOrder;

            root.transform.localScale = new Vector3(3.5f, 3.5f, 1f); // "huge"

            var collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = baseCollider.radius;

            var enemy = root.AddComponent<Enemy>();
            enemy.health = 500; // huge HP compared to a regular enemy's 2
            enemy.hitEffect = baseEnemy.hitEffect;
            enemy.destructionVFX = baseEnemy.destructionVFX;

            root.AddComponent<BossMovement>();
            root.AddComponent<Boss>();

            PrefabUtility.SaveAsPrefabAsset(root, BossPath);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        return AssetDatabase.LoadAssetAtPath<GameObject>(BossPath);
    }

    // ---- Scenes ----

    static void BuildLevel1()
    {
        DeleteIfExists(Level1Path);
        AssetDatabase.CopyAsset(DemoScenePath, Level1Path);

        var scene = EditorSceneManager.OpenScene(Level1Path, OpenSceneMode.Single);
        GameObject gameController = FindRoot(scene, "Game_Controller");
        var levelController = gameController.GetComponentInChildren<LevelController>();

        int expectedDefeats = ComputeExpectedDefeats(levelController);
        ConfigureLevelFlow(gameController, expectedDefeats, nextSceneName: "Level_2");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void BuildLevel2(GameObject shieldedWavePrefab, GameObject bossPrefab)
    {
        DeleteIfExists(Level2Path);
        AssetDatabase.CopyAsset(DemoScenePath, Level2Path);

        var scene = EditorSceneManager.OpenScene(Level2Path, OpenSceneMode.Single);
        GameObject gameController = FindRoot(scene, "Game_Controller");
        var levelController = gameController.GetComponentInChildren<LevelController>();

        // Harder than Level 1: waves arrive sooner, power-ups are rarer, planets busier.
        foreach (var wave in levelController.enemyWaves)
            wave.timeToStart *= 0.6f;
        levelController.timeForNewPowerup *= 1.5f;
        levelController.timeBetweenPlanets *= 0.7f;

        float lastWaveTime = levelController.enemyWaves.Length > 0
            ? levelController.enemyWaves.Max(w => w.timeToStart)
            : 0f;

        var waves = new List<EnemyWaves>(levelController.enemyWaves);
        waves.Add(new EnemyWaves { timeToStart = lastWaveTime + 4f, wave = shieldedWavePrefab });
        levelController.enemyWaves = waves.ToArray();

        levelController.boss = bossPrefab;
        levelController.bossSpawnDelay = lastWaveTime + 12f;

        int expectedDefeats = ComputeExpectedDefeats(levelController) + 1; // +1 for the boss
        ConfigureLevelFlow(gameController, expectedDefeats, nextSceneName: "");

        EditorUtility.SetDirty(levelController);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void ConfigureLevelFlow(GameObject gameController, int expectedDefeats, string nextSceneName)
    {
        var tracker = gameController.GetComponent<LevelCompletionTracker>();
        if (tracker == null)
            tracker = gameController.AddComponent<LevelCompletionTracker>();
        tracker.expectedDefeats = expectedDefeats;

        var flow = gameController.GetComponent<LevelFlow>();
        if (flow == null)
            flow = gameController.AddComponent<LevelFlow>();
        flow.nextSceneName = nextSceneName;
        flow.delayBeforeLoad = 2f;
    }

    static int ComputeExpectedDefeats(LevelController levelController)
    {
        int total = 0;
        foreach (var ew in levelController.enemyWaves)
            total += ew.wave.GetComponent<Wave>().count;
        return total;
    }

    static GameObject FindRoot(UnityEngine.SceneManagement.Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == name)
                return root;
        throw new System.Exception($"Could not find root GameObject '{name}' in scene '{scene.name}'");
    }

    static void AddScenesToBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        AddIfMissing(scenes, Level1Path);
        AddIfMissing(scenes, Level2Path);
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static void AddIfMissing(List<EditorBuildSettingsScene> scenes, string path)
    {
        if (!scenes.Exists(s => s.path == path))
            scenes.Add(new EditorBuildSettingsScene(path, true));
    }
}
