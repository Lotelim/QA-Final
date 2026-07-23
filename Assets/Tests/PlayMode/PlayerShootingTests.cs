using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests proving each weapon power level fires the expected number of projectiles
/// (1/2/3/4 -> 1/2/3/5 shots respectively, per the fan-out pattern in PlayerShooting.MakeAShot).
/// </summary>
public class PlayerShootingTests
{
    readonly List<GameObject> spawned = new List<GameObject>();
    string currentProjectileName;

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        PlayerShooting.instance = null;

        // MakeAShot() instantiates clones that aren't tracked individually; sweep them up so
        // leftover shots don't pollute later tests (in this fixture or any other, e.g. BonusTests
        // also spins up a real PlayerShooting and can leave its own stray shot behind).
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
        // Unique per test (not just per fixture) so this can never be confused with a clone
        // left behind by some other test file that also fires a shot, e.g. BonusTests.
        currentProjectileName = "TestProjectile_Power" + weaponPower;

        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestShooter");
        var shooting = go.AddComponent<PlayerShooting>();
        // Fires on the very first Update() (nextFire starts at 0, Time.time is already positive),
        // then sets a ~10000s cooldown - so however many extra frames this test yields through,
        // only that first shot ever fires. A "fast" fireRate would instead refire every frame
        // (0.001s cooldown vs. ~0.016s+ real frame time), overshooting the expected count.
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
        yield return null; // a few frames of margin; the ~10000s cooldown keeps this to a single shot

        Assert.AreEqual(expectedShots, CountProjectileClones());
    }

    // This Test Framework version doesn't support [UnityTest] combined with [TestCase], so each
    // weapon power gets its own method sharing the same coroutine assertion helper.
    [UnityTest]
    public IEnumerator MakeAShot_AtWeaponPower1_FiresOneProjectile() => AssertShotCountAtWeaponPower(1, 1);

    [UnityTest]
    public IEnumerator MakeAShot_AtWeaponPower2_FiresTwoProjectiles() => AssertShotCountAtWeaponPower(2, 2);

    [UnityTest]
    public IEnumerator MakeAShot_AtWeaponPower3_FiresThreeProjectiles() => AssertShotCountAtWeaponPower(3, 3);

    [UnityTest]
    public IEnumerator MakeAShot_AtWeaponPower4_FiresFiveProjectiles() => AssertShotCountAtWeaponPower(4, 5);
}
