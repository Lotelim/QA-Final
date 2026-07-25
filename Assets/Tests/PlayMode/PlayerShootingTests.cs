using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerShootingTests
{
    readonly List<GameObject> spawned = new List<GameObject>();
    string currentProjectileName;

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        PlayerShooting.instance = null;
 
        if (currentProjectileName != null)
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if (t.gameObject.name == currentProjectileName + "(Clone)")
                    Object.Destroy(t.gameObject);

        yield return null;
    }

    GameObject CreateGunWithVFX(string name)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, name);
        go.AddComponent<ParticleSystem>();
        return go;
    }

    PlayerShooting CreateShooter(int weaponPower)
    {  
        currentProjectileName = "TestProjectile_Power" + weaponPower;

        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestShooter");
        var shooting = go.AddComponent<PlayerShooting>();
        shooting.fireRate = 0.0001f;
        shooting.projectileObject = TestSceneHelpers.CreatePlaceholder(spawned, currentProjectileName);
        shooting.guns = new Guns
        {
            centralGun = CreateGunWithVFX("CentralGun"),
            leftGun = CreateGunWithVFX("LeftGun"),
            rightGun = CreateGunWithVFX("RightGun"),
        };
        shooting.weaponPower = weaponPower;
        return shooting;
    }

    int CountProjectileClones()
    {
        string cloneName = currentProjectileName + "(Clone)";
        int count = 0;
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.gameObject.name == cloneName)
                count++;
        return count;
    }

    IEnumerator AssertShotCountAtWeaponPower(int weaponPower, int expectedShots)
    {
        CreateShooter(weaponPower);
        yield return null;
        yield return null;
        yield return null;

        Assert.AreEqual(expectedShots, CountProjectileClones());
    }

    [UnityTest]
    public IEnumerator MakeAShot_AtWeaponPower1_FiresOneProjectile() => AssertShotCountAtWeaponPower(1, 1);

    [UnityTest]
    public IEnumerator MakeAShot_AtWeaponPower2_FiresTwoProjectiles() => AssertShotCountAtWeaponPower(2, 2);

    [UnityTest]
    public IEnumerator MakeAShot_AtWeaponPower3_FiresThreeProjectiles() => AssertShotCountAtWeaponPower(3, 3);

    [UnityTest]
    public IEnumerator MakeAShot_AtWeaponPower4_FiresFiveProjectiles() => AssertShotCountAtWeaponPower(4, 5);
}
