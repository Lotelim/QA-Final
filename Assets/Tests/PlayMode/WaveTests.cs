using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WaveTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        Player.instance = null;

        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.gameObject.name == "TestWaveEnemy(Clone)")
                Object.Destroy(t.gameObject);

        yield return null;
    }

    GameObject CreateEnemyPrefabPlaceholder()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestWaveEnemy");
        go.SetActive(false); 
        go.AddComponent<FollowThePath>();
        var enemy = go.AddComponent<Enemy>();
        enemy.hitEffect = TestSceneHelpers.CreatePlaceholder(spawned, "HitFX");
        enemy.destructionVFX = TestSceneHelpers.CreatePlaceholder(spawned, "DestructionFX");
        return go;
    }

    Transform[] CreateStraightPath(params Vector3[] points)
    {
        var transforms = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            GameObject pointGO = TestSceneHelpers.CreatePlaceholder(spawned, "PathPoint" + i);
            pointGO.transform.position = points[i];
            transforms[i] = pointGO.transform;
        }
        return transforms;
    }

    Wave CreateWave(int count, float timeBetween, GameObject enemyPrefab)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestWave");
        var wave = go.AddComponent<Wave>();
        wave.enemy = enemyPrefab;
        wave.count = count;
        wave.speed = 10f;
        wave.timeBetween = timeBetween;
        wave.pathPoints = CreateStraightPath(new Vector3(0, 5, 0), new Vector3(0, 0, 0), new Vector3(0, -5, 0), new Vector3(0, -10, 0));
        wave.shooting = new Shooting { shotChance = 0, shotTimeMin = 0, shotTimeMax = 0 };
        return wave;
    }

    GameObject CreatePlayer()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestPlayer");
        go.AddComponent<Player>().destructionFX = TestSceneHelpers.CreatePlaceholder(spawned, "PlayerFX");
        return go;
    }

    int CountActiveClones(string cloneName)
    {
        int count = 0;
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.gameObject.name == cloneName && t.gameObject.activeSelf)
                count++;
        return count;
    }

    [UnityTest]
    public IEnumerator CreateEnemyWave_WithPlayerPresent_SpawnsConfiguredCountOverTime()
    {
        CreatePlayer();
        GameObject enemyPrefab = CreateEnemyPrefabPlaceholder();
        CreateWave(count: 3, timeBetween: 0.05f, enemyPrefab: enemyPrefab);

        yield return new WaitForSeconds(0.3f);

        Assert.AreEqual(3, CountActiveClones("TestWaveEnemy(Clone)"));
    }

    [UnityTest]
    public IEnumerator CreateEnemyWave_StopsSpawning_OnceThePlayerIsGone()
    {
        GameObject player = CreatePlayer();
        GameObject enemyPrefab = CreateEnemyPrefabPlaceholder();
        CreateWave(count: 5, timeBetween: 0.1f, enemyPrefab: enemyPrefab);

        yield return new WaitForSeconds(0.15f); 
        int countBeforePlayerLeaves = CountActiveClones("TestWaveEnemy(Clone)");
        Assert.GreaterOrEqual(countBeforePlayerLeaves, 1);

        Object.Destroy(player);
        yield return null;

        yield return new WaitForSeconds(0.5f); 

        Assert.AreEqual(countBeforePlayerLeaves, CountActiveClones("TestWaveEnemy(Clone)"),
            "no further enemies should spawn once Player.instance is gone");
    }
}
