using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ShieldIntegrationTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    GameObject CreateEnemy(int health, int? shieldHealth = null)
    {
        var go = new GameObject("TestEnemy");
        spawned.Add(go);

        if (shieldHealth.HasValue)
        {
            var shield = go.AddComponent<Shield>();
            shield.shieldHealth = shieldHealth.Value;
        }

        var enemy = go.AddComponent<Enemy>();
        enemy.health = health;
        enemy.hitEffect = CreatePlaceholder("HitFX");
        enemy.destructionVFX = CreatePlaceholder("DestructionFX");

        return go;
    }

    GameObject CreatePlaceholder(string name)
    {
        var placeholder = new GameObject(name);
        spawned.Add(placeholder);
        return placeholder;
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
    public IEnumerator GetDamage_WithActiveShield_ProtectsHealthUntilShieldDepleted()
    {
        GameObject enemyObject = CreateEnemy(health: 10, shieldHealth: 5);
        yield return null; 

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        Shield shield = enemyObject.GetComponent<Shield>();

        enemy.GetDamage(3);
        Assert.AreEqual(10, enemy.health);
        Assert.AreEqual(2, shield.shieldHealth);

        enemy.GetDamage(4); 
        Assert.AreEqual(8, enemy.health);
        Assert.IsFalse(shield.IsActive);
    }

    [UnityTest]
    public IEnumerator GetDamage_WithoutShield_DamagesHealthDirectlyAsBefore()
    {
        GameObject enemyObject = CreateEnemy(health: 10);
        yield return null;

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemy.GetDamage(4);

        Assert.AreEqual(6, enemy.health);
    }

    [UnityTest]
    public IEnumerator GetDamage_LethalDamageThroughDepletedShield_DestroysEnemy()
    {
        GameObject enemyObject = CreateEnemy(health: 5, shieldHealth: 2);
        yield return null;

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemy.GetDamage(10); 

        yield return null; 

        Assert.IsTrue(enemyObject == null);
    }
}
