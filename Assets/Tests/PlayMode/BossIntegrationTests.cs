using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for the Boss ship: BossMovement should roam the play area in various
/// directions without leaving screen bounds, and a Boss (which extends Enemy directly, with
/// huge health) should survive normal hits but still raise OnDestroyed once truly defeated.
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
    public IEnumerator BossMovement_RampsUpVelocityGradually_InsteadOfSnappingToFullSpeed()
    {
        // Regression test for "enemies move too weird": BossMovement used to jump straight to
        // the new direction's full-speed vector the instant CurrentDirection changed, which
        // looked robotic next to the smoothly path-following regular enemies. It should now
        // ease its velocity toward the target instead.
        CreateMainCamera();
        GameObject bossGO = CreatePlaceholder("TestBoss3");
        var movement = bossGO.AddComponent<BossMovement>();
        movement.speed = 10f;
        movement.minDirectionHoldTime = 10f;
        movement.maxDirectionHoldTime = 10f;
        movement.turnAcceleration = 1f; // slow ramp so the first tick is clearly still accelerating

        yield return null; // Start(): picks a direction, velocity starts at zero
        yield return null; // one Update() tick of ramping

        Assert.Less(movement.CurrentSpeed, movement.speed,
            "velocity should still be ramping up, not already at full speed, right after picking a new direction");
    }

    [UnityTest]
    public IEnumerator Boss_HasHugeHealth_SurvivesANormalHit()
    {
        // Boss extends Enemy directly (no separate Enemy component) - adding both used to make
        // GetComponent<Enemy>() ambiguous between two independent health pools, which was the
        // actual cause of the boss health bar not tracking damage in real play.
        GameObject bossGO = CreatePlaceholder("TestBossEntity");
        var boss = bossGO.AddComponent<Boss>();
        boss.health = 500; // huge HP compared to a regular enemy
        boss.hitEffect = CreatePlaceholder("HitFX");
        boss.destructionVFX = CreatePlaceholder("DestructionFX");

        yield return null;

        boss.GetDamage(50);

        Assert.AreEqual(450, boss.health);
        Assert.IsFalse(bossGO == null);
    }

    [UnityTest]
    public IEnumerator Boss_WhenFinallyDefeated_RaisesOnDestroyedAndIsRemoved()
    {
        GameObject bossGO = CreatePlaceholder("TestBossEntity");
        var boss = bossGO.AddComponent<Boss>();
        boss.health = 500;
        boss.hitEffect = CreatePlaceholder("HitFX");
        boss.destructionVFX = CreatePlaceholder("DestructionFX");

        yield return null;

        bool defeated = false;
        boss.OnDestroyed += () => defeated = true;

        boss.GetDamage(500);
        yield return null; // let Destroy() take effect

        Assert.IsTrue(defeated);
        Assert.IsTrue(bossGO == null);
    }

    [UnityTest]
    public IEnumerator Boss_GetComponentEnemy_ResolvesToTheSameInstanceAsGetComponentBoss()
    {
        // Regression test for the actual reported bug: with Boss : Enemy, a GameObject must
        // have exactly one Enemy-family component. If a separate Enemy component were ever
        // added alongside Boss again, GetComponent<Enemy>() (used by the damage-dealing
        // collision code) and GetComponent<Boss>() could resolve to two different instances,
        // desyncing whichever one the HUD subscribed to from whichever one actually takes damage.
        GameObject bossGO = CreatePlaceholder("TestBossEntity2");
        var boss = bossGO.AddComponent<Boss>();
        boss.health = 500;
        boss.hitEffect = CreatePlaceholder("HitFX");
        boss.destructionVFX = CreatePlaceholder("DestructionFX");

        yield return null;

        Assert.AreSame(boss, bossGO.GetComponent<Enemy>());
        Assert.AreSame(boss, bossGO.GetComponent<Boss>());
    }
}
