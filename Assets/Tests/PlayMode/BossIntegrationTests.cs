using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for the Boss ship: BossMovement should roam the play area in various
/// directions without leaving screen bounds, and a Boss (Enemy + Boss marker with huge
/// health) should survive normal hits but still raise OnDestroyed once truly defeated.
/// </summary>
public class BossIntegrationTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    GameObject CreatePlaceholder(string name)
    {
        var go = new GameObject(name);
        spawned.Add(go);
        return go;
    }

    Camera CreateMainCamera()
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

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject go in spawned)
            if (go != null)
                Object.Destroy(go);
        spawned.Clear();
        yield return null;
    }

    [UnityTest]
    public IEnumerator BossMovement_StaysWithinConfiguredScreenBounds()
    {
        Camera cam = CreateMainCamera();
        GameObject bossGO = CreatePlaceholder("TestBoss");
        var movement = bossGO.AddComponent<BossMovement>();
        movement.speed = 50f; // fast, so any chosen direction reaches a bound quickly
        movement.minDirectionHoldTime = 10f; // hold the first pick for the whole test - no re-pick noise
        movement.maxDirectionHoldTime = 10f;

        yield return null; // Start() computes bounds and picks a direction

        float expectedMinX = cam.ViewportToWorldPoint(Vector2.zero).x + movement.minXOffset;
        float expectedMaxX = cam.ViewportToWorldPoint(Vector2.right).x - movement.maxXOffset;
        float expectedMinY = cam.ViewportToWorldPoint(Vector2.zero).y + movement.minYOffset;
        float expectedMaxY = cam.ViewportToWorldPoint(Vector2.up).y - movement.maxYOffset;

        Assert.IsTrue(System.Enum.IsDefined(typeof(BossDirection), movement.CurrentDirection));

        for (int i = 0; i < 120; i++)
        {
            yield return null;
            Vector3 pos = bossGO.transform.position;
            Assert.That(pos.x, Is.InRange(expectedMinX - 0.01f, expectedMaxX + 0.01f));
            Assert.That(pos.y, Is.InRange(expectedMinY - 0.01f, expectedMaxY + 0.01f));
        }
    }

    [UnityTest]
    public IEnumerator Boss_HasHugeHealth_SurvivesANormalHit()
    {
        GameObject bossGO = CreatePlaceholder("TestBossEntity");
        var enemy = bossGO.AddComponent<Enemy>();
        enemy.health = 500; // huge HP compared to a regular enemy
        enemy.hitEffect = CreatePlaceholder("HitFX");
        enemy.destructionVFX = CreatePlaceholder("DestructionFX");
        bossGO.AddComponent<Boss>();

        yield return null;

        enemy.GetDamage(50);

        Assert.AreEqual(450, enemy.health);
        Assert.IsFalse(bossGO == null);
    }

    [UnityTest]
    public IEnumerator Boss_WhenFinallyDefeated_RaisesOnDestroyedAndIsRemoved()
    {
        GameObject bossGO = CreatePlaceholder("TestBossEntity");
        var enemy = bossGO.AddComponent<Enemy>();
        enemy.health = 500;
        enemy.hitEffect = CreatePlaceholder("HitFX");
        enemy.destructionVFX = CreatePlaceholder("DestructionFX");
        bossGO.AddComponent<Boss>();

        yield return null;

        bool defeated = false;
        enemy.OnDestroyed += () => defeated = true;

        enemy.GetDamage(500);
        yield return null; // let Destroy() take effect

        Assert.IsTrue(defeated);
        Assert.IsTrue(bossGO == null);
    }
}
