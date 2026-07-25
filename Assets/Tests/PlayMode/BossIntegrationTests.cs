using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
        movement.speed = 50f; 
        movement.minDirectionHoldTime = 10f; 
        movement.maxDirectionHoldTime = 10f;

        yield return null; 

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
        CreateMainCamera();
        GameObject bossGO = CreatePlaceholder("TestBoss3");
        var movement = bossGO.AddComponent<BossMovement>();
        movement.speed = 10f;
        movement.minDirectionHoldTime = 10f;
        movement.maxDirectionHoldTime = 10f;
        movement.turnAcceleration = 1f; 

        yield return null; 
        yield return null; 

        Assert.Less(movement.CurrentSpeed, movement.speed,
            "velocity should still be ramping up, not already at full speed, right after picking a new direction");
    }

    [UnityTest]
    public IEnumerator Boss_HasHugeHealth_SurvivesANormalHit()
    {  
        GameObject bossGO = CreatePlaceholder("TestBossEntity");
        var boss = bossGO.AddComponent<Boss>();
        boss.health = 500; 
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
        yield return null; 

        Assert.IsTrue(defeated);
        Assert.IsTrue(bossGO == null);
    }

    [UnityTest]
    public IEnumerator Boss_GetComponentEnemy_ResolvesToTheSameInstanceAsGetComponentBoss()
    {
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
