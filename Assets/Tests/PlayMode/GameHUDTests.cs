using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

public class GameHUDTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        Player.instance = null;
        LevelCompletionTracker.instance = null;

        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.gameObject.name == "TestWavePrefabForHud(Clone)" || t.gameObject.name == "TestBossPrefabForHud(Clone)")
                Object.Destroy(t.gameObject);

        yield return null;
    }

    GameHUD CreateHud()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestHUD");
        var hud = go.AddComponent<GameHUD>();

        GameObject waveTextGO = TestSceneHelpers.CreatePlaceholder(spawned, "WaveText");
        hud.waveText = waveTextGO.AddComponent<Text>();

        hud.bossHealthBarRoot = TestSceneHelpers.CreatePlaceholder(spawned, "BossBarRoot");
        GameObject fillGO = TestSceneHelpers.CreatePlaceholder(spawned, "BossBarFill");
        hud.bossHealthFill = fillGO.AddComponent<Slider>();

        hud.winScreenRoot = TestSceneHelpers.CreatePlaceholder(spawned, "WinScreen");
        hud.loseScreenRoot = TestSceneHelpers.CreatePlaceholder(spawned, "LoseScreen");
        return hud;
    }

    GameObject CreatePlayer()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestPlayer");
        go.AddComponent<Player>().destructionFX = TestSceneHelpers.CreatePlaceholder(spawned, "PlayerFX");
        return go;
    }

    LevelController CreateLevelController()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestLevelControllerForHud");
        var controller = go.AddComponent<LevelController>();
        controller.enemyWaves = new EnemyWaves[0];
        controller.timeForNewPowerup = 9999f;
        controller.planets = new GameObject[0];
        controller.timeBetweenPlanets = 9999f;
        return controller;
    }

    [UnityTest]
    public IEnumerator Start_HidesBossBarAndWinLoseScreens_ByDefault()
    {
        GameHUD hud = CreateHud();
        yield return null;

        Assert.IsFalse(hud.bossHealthBarRoot.activeSelf);
        Assert.IsFalse(hud.winScreenRoot.activeSelf);
        Assert.IsFalse(hud.loseScreenRoot.activeSelf);
    }

    [UnityTest]
    public IEnumerator WaveStarted_UpdatesWaveText_EvenForAZeroDelayFirstWave()
    {
        CreatePlayer();
        GameObject wavePrefab = TestSceneHelpers.CreatePlaceholder(spawned, "TestWavePrefabForHud");
        LevelController controller = CreateLevelController();
        controller.enemyWaves = new[] { new EnemyWaves { timeToStart = 0f, wave = wavePrefab } };

        GameHUD hud = CreateHud();

        yield return null; 

        Assert.AreEqual("Wave 1", hud.waveText.text);
    }

    [UnityTest]
    public IEnumerator BossSpawned_ShowsHealthBar_TracksDamage_AndHidesAgainWhenDefeated()
    {
        CreatePlayer();
        GameObject bossPrefab = TestSceneHelpers.CreatePlaceholder(spawned, "TestBossPrefabForHud");
        var bossEnemy = bossPrefab.AddComponent<Boss>(); 
        bossEnemy.health = 100;
        bossEnemy.hitEffect = TestSceneHelpers.CreatePlaceholder(spawned, "HitFX");
        bossEnemy.destructionVFX = TestSceneHelpers.CreatePlaceholder(spawned, "DestructionFX");

        LevelController controller = CreateLevelController();
        controller.boss = bossPrefab;
        controller.bossSpawnDelay = 0.05f;

        GameHUD hud = CreateHud();

        yield return new WaitForSeconds(0.15f); 

        Assert.IsTrue(hud.bossHealthBarRoot.activeSelf, "health bar should show once the boss spawns");
        Assert.AreEqual(1f, hud.bossHealthFill.value, 0.001f, "should start full");

        Enemy spawnedBoss = null;
        foreach (Enemy e in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            if (e.gameObject.name == "TestBossPrefabForHud(Clone)")
                spawnedBoss = e;
        Assert.IsNotNull(spawnedBoss);

        spawnedBoss.GetDamage(50);
        Assert.AreEqual(0.5f, hud.bossHealthFill.value, 0.001f, "fill should track remaining health");

        spawnedBoss.GetDamage(50);
        yield return null; 

        Assert.IsFalse(hud.bossHealthBarRoot.activeSelf, "health bar should hide once the boss is defeated");
    }

    [UnityTest]
    public IEnumerator PlayerDied_ShowsLoseScreen()
    {
        GameObject playerGO = CreatePlayer();
        GameHUD hud = CreateHud();
        yield return null; 

        playerGO.GetComponent<Player>().GetDamage(1);
        yield return null;

        Assert.IsTrue(hud.loseScreenRoot.activeSelf);
    }

    [UnityTest]
    public IEnumerator LevelComplete_OnTheLastLevel_ShowsWinScreen()
    {
        GameObject trackerGO = TestSceneHelpers.CreatePlaceholder(spawned, "Tracker");
        var tracker = trackerGO.AddComponent<LevelCompletionTracker>();
        tracker.expectedDefeats = 1;
        var flow = trackerGO.AddComponent<LevelFlow>();
        flow.nextSceneName = ""; 

        GameHUD hud = CreateHud();
        yield return null;

        tracker.OnLevelComplete.Invoke(); 

        Assert.IsTrue(hud.winScreenRoot.activeSelf);
    }

    [UnityTest]
    public IEnumerator LevelComplete_WhenNotTheLastLevel_DoesNotShowWinScreen()
    {
        GameObject trackerGO = TestSceneHelpers.CreatePlaceholder(spawned, "Tracker");
        var tracker = trackerGO.AddComponent<LevelCompletionTracker>();
        tracker.expectedDefeats = 1;
        var flow = trackerGO.AddComponent<LevelFlow>();
        flow.nextSceneName = "Level_2"; 

        GameHUD hud = CreateHud();
        yield return null;

        tracker.OnLevelComplete.Invoke();

        Assert.IsFalse(hud.winScreenRoot.activeSelf);
    }
}
