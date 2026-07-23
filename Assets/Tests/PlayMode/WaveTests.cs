using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for Wave's spawn coroutine, including a regression test for the
/// missing Player.instance guard (Wave used to keep spawning enemies after the player
/// was gone, unlike LevelController's equivalent coroutine).
/// </summary>
public class WaveTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        Player.instance = null;

        // Wave.CreateEnemyWave() instantiates enemy clones with no tracked reference; sweep them
        // up so one test's spawned enemies don't inflate the next test's count.
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.gameObject.name == "TestWaveEnemy(Clone)")
                Object.Destroy(t.gameObject);

        yield return null;
    }

    GameObject CreateEnemyPrefabPlaceholder()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestWaveEnemy");
        go.SetActive(false); // wave prefabs are inactive templates, activated once configured
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

        yield return new WaitForSeconds(0.15f); // let at least 1 enemy spawn
        int countBeforePlayerLeaves = CountActiveClones("TestWaveEnemy(Clone)");
        Assert.GreaterOrEqual(countBeforePlayerLeaves, 1);

        Object.Destroy(player);
        yield return null; // Player.instance is now cleared

        yield return new WaitForSeconds(0.5f); // long enough for the remaining enemies to have spawned without the guard

        Assert.AreEqual(countBeforePlayerLeaves, CountActiveClones("TestWaveEnemy(Clone)"),
            "no further enemies should spawn once Player.instance is gone");
    }
}
