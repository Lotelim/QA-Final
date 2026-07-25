using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LevelControllerTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        Player.instance = null;
    
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
        controller.timeForNewPowerup = 9999f; 
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

        yield return null; 

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

    [UnityTest]
    public IEnumerator PowerupBonusCreation_AfterThePlayerIsGone_DoesNotErrorOut()
    { 
        TestSceneHelpers.CreateMainCamera(spawned);
        GameObject powerUpPrefab = TestSceneHelpers.CreatePlaceholder(spawned, "TestPowerUp");
        powerUpPrefab.AddComponent<SpriteRenderer>(); 

        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestLevelControllerNoPlayer");
        var controller = go.AddComponent<LevelController>();
        controller.enemyWaves = new EnemyWaves[0];
        controller.powerUp = powerUpPrefab;
        controller.timeForNewPowerup = 0.05f;
        controller.planets = new GameObject[0];
        controller.timeBetweenPlanets = 9999f;

        yield return new WaitForSeconds(0.15f); 
    }

    [UnityTest]
    public IEnumerator OnWaveStarted_FiresWithOneBasedWaveNumber_AsEachWaveSpawns()
    {
        CreatePlayer();
        GameObject wavePrefabA = TestSceneHelpers.CreatePlaceholder(spawned, "TestWavePrefabA");
        GameObject wavePrefabB = TestSceneHelpers.CreatePlaceholder(spawned, "TestWavePrefabB");

        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestLevelControllerWaveEvents");
        var controller = go.AddComponent<LevelController>();
        controller.enemyWaves = new[]
        {
            new EnemyWaves { timeToStart = 0f, wave = wavePrefabA },
            new EnemyWaves { timeToStart = 0.1f, wave = wavePrefabB },
        };
        controller.timeForNewPowerup = 9999f;
        controller.planets = new GameObject[0];
        controller.timeBetweenPlanets = 9999f;

        var fired = new List<int>();
        controller.OnWaveStarted += n => fired.Add(n);

        yield return null; 
        yield return new WaitForSeconds(0.15f); 

        CollectionAssert.AreEqual(new[] { 1, 2 }, fired);
    }
}
