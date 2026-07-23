using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for level-clear detection: OnLevelComplete must fire exactly once,
/// only once every registered enemy up to expectedDefeats has actually been destroyed -
/// not merely whenever the currently-alive count happens to hit zero (which would
/// misfire during the natural gap between waves).
/// </summary>
public class LevelCompletionTrackerTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    GameObject CreatePlaceholder(string name = "FX")
    {
        var go = new GameObject(name);
        spawned.Add(go);
        return go;
    }

    Enemy CreateEnemy(int health = 1)
    {
        GameObject go = CreatePlaceholder("TestEnemy");
        var enemy = go.AddComponent<Enemy>();
        enemy.health = health;
        enemy.hitEffect = CreatePlaceholder();
        enemy.destructionVFX = CreatePlaceholder();
        return enemy;
    }

    LevelCompletionTracker CreateTracker(int expectedDefeats)
    {
        GameObject go = CreatePlaceholder("Tracker");
        var tracker = go.AddComponent<LevelCompletionTracker>();
        tracker.expectedDefeats = expectedDefeats;
        return tracker;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject go in spawned)
            if (go != null)
                Object.Destroy(go);
        spawned.Clear();
        LevelCompletionTracker.instance = null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnLevelComplete_FiresOnlyAfterAllRegisteredEnemiesAreDefeated()
    {
        LevelCompletionTracker tracker = CreateTracker(expectedDefeats: 2);
        Enemy enemyA = CreateEnemy();
        Enemy enemyB = CreateEnemy();
        tracker.Register(enemyA);
        tracker.Register(enemyB);
        yield return null;

        int fireCount = 0;
        tracker.OnLevelComplete.AddListener(() => fireCount++);

        enemyA.GetDamage(1);
        yield return null;
        Assert.AreEqual(0, fireCount, "should not complete after only 1 of 2 enemies is defeated");

        enemyB.GetDamage(1);
        yield return null;
        Assert.AreEqual(1, fireCount, "should complete exactly once after the last enemy is defeated");
    }

    [UnityTest]
    public IEnumerator OnLevelComplete_DoesNotFireAgain_IfMoreEnemiesDieAfterCompletion()
    {
        LevelCompletionTracker tracker = CreateTracker(expectedDefeats: 1);
        Enemy enemyA = CreateEnemy();
        Enemy enemyB = CreateEnemy(); // registered, but not needed to reach expectedDefeats
        tracker.Register(enemyA);
        tracker.Register(enemyB);
        yield return null;

        int fireCount = 0;
        tracker.OnLevelComplete.AddListener(() => fireCount++);

        enemyA.GetDamage(1);
        yield return null;
        Assert.AreEqual(1, fireCount);

        enemyB.GetDamage(1);
        yield return null;
        Assert.AreEqual(1, fireCount, "should not fire a second time");
    }

    [UnityTest]
    public IEnumerator Register_CalledTwiceForSameEnemy_DoesNotDoubleCountItsDeath()
    {
        LevelCompletionTracker tracker = CreateTracker(expectedDefeats: 2);
        Enemy enemyA = CreateEnemy();
        tracker.Register(enemyA);
        tracker.Register(enemyA); // duplicate registration
        yield return null;

        int fireCount = 0;
        tracker.OnLevelComplete.AddListener(() => fireCount++);

        enemyA.GetDamage(1);
        yield return null;

        Assert.AreEqual(0, fireCount, "one enemy's death should count once toward expectedDefeats=2, not twice");
    }

    [UnityTest]
    public IEnumerator Enemy_AutoRegistersWithActiveTracker_OnStart()
    {
        LevelCompletionTracker tracker = CreateTracker(expectedDefeats: 1);
        Enemy enemy = CreateEnemy();
        yield return null; // Enemy.Start() runs and should self-register since tracker.instance is set

        int fireCount = 0;
        tracker.OnLevelComplete.AddListener(() => fireCount++);

        enemy.GetDamage(1);
        yield return null;

        Assert.AreEqual(1, fireCount);
    }
}
