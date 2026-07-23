using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for LevelController's wave-scheduling coroutine. The powerup/planet
/// coroutines are configured with very long intervals so they cannot fire (and cannot NRE
/// on missing PlayerMoving/planet setup) within these short-lived tests.
/// </summary>
public class LevelControllerTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        Player.instance = null;

        // LevelController.CreateEnemyWave() instantiates wave clones with no tracked reference;
        // sweep them up so one test's spawn doesn't inflate the next test's count.
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.gameObject.name == "TestWavePrefab(Clone)")
                Object.Destroy(t.gameObject);

        yield return null;
    }

    LevelController CreateLevelController(float waveDelay, GameObject wavePrefab)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestLevelController");
        var controller = go.AddComponent<LevelController>();
        controller.enemyWaves = new[] { new EnemyWaves { timeToStart = waveDelay, wave = wavePrefab } };
        controller.timeForNewPowerup = 9999f; // keeps PowerupBonusCreation from ever firing in-test
        controller.planets = new GameObject[0];
        controller.timeBetweenPlanets = 9999f;
        controller.planetsSpeed = 0f;
        return controller;
    }

    GameObject CreatePlayer()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestPlayer");
        go.AddComponent<Player>().destructionFX = TestSceneHelpers.CreatePlaceholder(spawned, "PlayerFX");
        return go;
    }

    int CountClones(string cloneName)
    {
        int count = 0;
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.gameObject.name == cloneName)
                count++;
        return count;
    }

    [UnityTest]
    public IEnumerator CreateEnemyWave_WithZeroDelay_SpawnsImmediately_WhenPlayerPresent()
    {
        CreatePlayer();
        GameObject wavePrefab = TestSceneHelpers.CreatePlaceholder(spawned, "TestWavePrefab");
        CreateLevelController(waveDelay: 0f, wavePrefab: wavePrefab);

        yield return null; // Start() runs; delay=0 wave should instantiate right away

        Assert.AreEqual(1, CountClones("TestWavePrefab(Clone)"));
    }

    [UnityTest]
    public IEnumerator CreateEnemyWave_WhenPlayerIsAbsent_DoesNotSpawn()
    {
        GameObject wavePrefab = TestSceneHelpers.CreatePlaceholder(spawned, "TestWavePrefab");
        CreateLevelController(waveDelay: 0f, wavePrefab: wavePrefab);

        yield return null;

        Assert.AreEqual(0, CountClones("TestWavePrefab(Clone)"));
    }

    [UnityTest]
    public IEnumerator CreateEnemyWave_WithDelay_WaitsBeforeSpawning()
    {
        CreatePlayer();
        GameObject wavePrefab = TestSceneHelpers.CreatePlaceholder(spawned, "TestWavePrefab");
        CreateLevelController(waveDelay: 0.2f, wavePrefab: wavePrefab);

        yield return null;
        Assert.AreEqual(0, CountClones("TestWavePrefab(Clone)"), "should not spawn before its delay elapses");

        yield return new WaitForSeconds(0.3f);
        Assert.AreEqual(1, CountClones("TestWavePrefab(Clone)"));
    }
}
