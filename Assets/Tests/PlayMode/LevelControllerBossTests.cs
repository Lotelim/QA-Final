using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for LevelController's optional single boss spawn, added to support
/// Level 2. Mirrors the existing wave-spawn coroutine's Player.instance guard.
/// </summary>
public class LevelControllerBossTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        Player.instance = null;

        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.gameObject.name == "TestBossPrefab(Clone)")
                Object.Destroy(t.gameObject);

        yield return null;
    }

    LevelController CreateLevelController(GameObject bossPrefab, float bossSpawnDelay)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestLevelController");
        var controller = go.AddComponent<LevelController>();
        controller.enemyWaves = new EnemyWaves[0];
        controller.timeForNewPowerup = 9999f;
        controller.planets = new GameObject[0];
        controller.timeBetweenPlanets = 9999f;
        controller.planetsSpeed = 0f;
        controller.boss = bossPrefab;
        controller.bossSpawnDelay = bossSpawnDelay;
        return controller;
    }

    GameObject CreatePlayer()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestPlayer");
        go.AddComponent<Player>().destructionFX = TestSceneHelpers.CreatePlaceholder(spawned, "PlayerFX");
        return go;
    }

    int CountBossClones()
    {
        int count = 0;
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.gameObject.name == "TestBossPrefab(Clone)")
                count++;
        return count;
    }

    [UnityTest]
    public IEnumerator Boss_WithNoBossConfigured_NeverSpawnsAnything()
    {
        CreatePlayer();
        CreateLevelController(bossPrefab: null, bossSpawnDelay: 0f);

        yield return null;

        Assert.AreEqual(0, CountBossClones());
    }

    [UnityTest]
    public IEnumerator Boss_SpawnsAfterConfiguredDelay_WhenPlayerPresent()
    {
        CreatePlayer();
        GameObject bossPrefab = TestSceneHelpers.CreatePlaceholder(spawned, "TestBossPrefab");
        CreateLevelController(bossPrefab, bossSpawnDelay: 0.15f);

        yield return null;
        Assert.AreEqual(0, CountBossClones(), "should not spawn before its delay elapses");

        yield return new WaitForSeconds(0.25f);
        Assert.AreEqual(1, CountBossClones());
    }

    [UnityTest]
    public IEnumerator Boss_WhenPlayerIsAbsentAtSpawnTime_DoesNotSpawn()
    {
        // no player created
        GameObject bossPrefab = TestSceneHelpers.CreatePlaceholder(spawned, "TestBossPrefab");
        CreateLevelController(bossPrefab, bossSpawnDelay: 0.05f);

        yield return new WaitForSeconds(0.15f);

        Assert.AreEqual(0, CountBossClones());
    }
}
