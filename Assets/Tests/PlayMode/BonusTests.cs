using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class BonusTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        PlayerShooting.instance = null;

        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.gameObject.name == "TestProjectile(Clone)")
                Object.Destroy(t.gameObject);

        yield return null;
    }

    GameObject CreateGunWithVFX(string name)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, name);
        go.AddComponent<ParticleSystem>();
        return go;
    }

    GameObject CreatePlayerWithShooting(int weaponPower)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestPlayer");
        go.tag = "Player";
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        var shooting = go.AddComponent<PlayerShooting>();
        shooting.weaponPower = weaponPower;
        shooting.projectileObject = TestSceneHelpers.CreatePlaceholder(spawned, "TestProjectile");
        shooting.guns = new Guns
        {
            centralGun = CreateGunWithVFX("CentralGun"),
            leftGun = CreateGunWithVFX("LeftGun"),
            rightGun = CreateGunWithVFX("RightGun"),
        };
        return go;
    }

    GameObject CreateBonus(Vector3 position)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestBonus");
        go.transform.position = position;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        go.AddComponent<Bonus>();
        return go;
    }

    [UnityTest]
    public IEnumerator PlayerPickup_IncreasesWeaponPower_AndDestroysTheBonus()
    {
        CreatePlayerWithShooting(weaponPower: 1);
        GameObject bonus = CreateBonus(Vector3.zero);

        yield return null;
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.AreEqual(2, PlayerShooting.instance.weaponPower);
        Assert.IsTrue(bonus == null);
    }

    [UnityTest]
    public IEnumerator PlayerPickup_AtMaxWeaponPower_DoesNotExceedMax()
    {
        CreatePlayerWithShooting(weaponPower: 4); 
        GameObject bonus = CreateBonus(Vector3.zero);

        yield return null;
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.AreEqual(4, PlayerShooting.instance.weaponPower);
        Assert.IsTrue(bonus == null, "the bonus should still be consumed even when weapon power is already maxed");
    }
}
