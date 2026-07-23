using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for Projectile's friend/foe damage routing via real 2D trigger collisions.
/// </summary>
public class ProjectileTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        Player.instance = null;
        yield return null;
    }

    GameObject CreateProjectile(bool enemyBullet, int damage, bool destroyedByCollision, Vector3 position)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestProjectile");
        go.transform.position = position;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        var projectile = go.AddComponent<Projectile>();
        projectile.enemyBullet = enemyBullet;
        projectile.damage = damage;
        projectile.destroyedByCollision = destroyedByCollision;
        return go;
    }

    GameObject CreatePlayerTarget(Vector3 position)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestPlayer");
        go.tag = "Player";
        go.transform.position = position;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        go.AddComponent<BoxCollider2D>().isTrigger = true;
        go.AddComponent<Player>().destructionFX = TestSceneHelpers.CreatePlaceholder(spawned, "PlayerDestructionFX");
        return go;
    }

    GameObject CreateEnemyTarget(Vector3 position, int health)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestEnemy");
        go.tag = "Enemy";
        go.transform.position = position;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        var enemy = go.AddComponent<Enemy>();
        enemy.health = health;
        enemy.hitEffect = TestSceneHelpers.CreatePlaceholder(spawned, "HitFX");
        enemy.destructionVFX = TestSceneHelpers.CreatePlaceholder(spawned, "DestructionFX");
        return go;
    }

    [UnityTest]
    public IEnumerator EnemyBullet_HittingPlayer_DamagesPlayerAndSelfDestructsIfConfigured()
    {
        CreatePlayerTarget(Vector3.zero);
        GameObject projectile = CreateProjectile(enemyBullet: true, damage: 1, destroyedByCollision: true, position: Vector3.zero);

        yield return null;
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsTrue(Player.instance == null, "any hit destroys the player (single-hit-death model)");
        Assert.IsTrue(projectile == null, "projectile should self-destroy on collision when destroyedByCollision is set");
    }

    [UnityTest]
    public IEnumerator PlayerBullet_HittingEnemy_DamagesEnemyAndSelfDestructsIfConfigured()
    {
        GameObject enemy = CreateEnemyTarget(Vector3.zero, health: 10);
        GameObject projectile = CreateProjectile(enemyBullet: false, damage: 4, destroyedByCollision: true, position: Vector3.zero);

        yield return null;
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.AreEqual(6, enemy.GetComponent<Enemy>().health);
        Assert.IsTrue(projectile == null);
    }

    [UnityTest]
    public IEnumerator Projectile_WithDestroyedByCollisionFalse_SurvivesTheHit()
    {
        CreateEnemyTarget(Vector3.zero, health: 100);
        GameObject projectile = CreateProjectile(enemyBullet: false, damage: 1, destroyedByCollision: false, position: Vector3.zero);

        yield return null;
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsFalse(projectile == null, "destroyedByCollision=false should let the projectile pass through");
    }

    [UnityTest]
    public IEnumerator EnemyBullet_HittingAPlayerTaggedObjectWithNoLivePlayerInstance_DoesNotErrorOut()
    {
        // Regression test for the Projectile.cs null-guard: without it, Player.instance being
        // left stale after the real Player was destroyed (e.g. mid level-transition) would throw
        // a MissingReferenceException from inside OnTriggerEnter2D. Unity Test Framework fails
        // a test automatically if anything logs an error/exception during it, so this test
        // passes precisely by NOT logging one.
        GameObject firstPlayer = CreatePlayerTarget(new Vector3(-100, -100, 0));
        yield return null;
        Object.Destroy(firstPlayer);
        yield return null; // Player.instance is now cleared by the OnDestroy fix

        GameObject bareTarget = TestSceneHelpers.CreatePlaceholder(spawned, "BarePlayerTag");
        bareTarget.tag = "Player";
        var rb = bareTarget.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        bareTarget.AddComponent<BoxCollider2D>().isTrigger = true;

        CreateProjectile(enemyBullet: true, damage: 1, destroyedByCollision: false, position: bareTarget.transform.position);

        yield return null;
        yield return new WaitForFixedUpdate();
        yield return null;
    }
}
