using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode tests for the pure damage-absorption math of the Shield component,
/// written before Shield.cs existed to pin down the exact contract:
/// a shield absorbs as much incoming damage as it has points for, and lets
/// only the overflow through to whatever is behind it.
/// </summary>
public class ShieldTests
{
    static Shield CreateShield(int shieldHealth)
    {
        var go = new GameObject("TestShield");
        var shield = go.AddComponent<Shield>();
        shield.shieldHealth = shieldHealth;
        return shield;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (Shield shield in Object.FindObjectsByType<Shield>(FindObjectsSortMode.None))
            Object.DestroyImmediate(shield.gameObject);
    }

    [Test]
    public void AbsorbDamage_WhenShieldHasMoreThanEnoughPoints_AbsorbsAllDamageAndReturnsZero()
    {
        Shield shield = CreateShield(10);

        int leftover = shield.AbsorbDamage(4);

        Assert.AreEqual(0, leftover);
        Assert.AreEqual(6, shield.shieldHealth);
        Assert.IsTrue(shield.IsActive);
    }

    [Test]
    public void AbsorbDamage_WhenDamageExactlyDepletesShield_ReturnsZeroAndDeactivates()
    {
        Shield shield = CreateShield(5);

        int leftover = shield.AbsorbDamage(5);

        Assert.AreEqual(0, leftover);
        Assert.AreEqual(0, shield.shieldHealth);
        Assert.IsFalse(shield.IsActive);
    }

    [Test]
    public void AbsorbDamage_WhenDamageExceedsShield_ReturnsOverflowAndDeactivates()
    {
        Shield shield = CreateShield(3);

        int leftover = shield.AbsorbDamage(10);

        Assert.AreEqual(7, leftover);
        Assert.AreEqual(0, shield.shieldHealth);
        Assert.IsFalse(shield.IsActive);
    }

    [Test]
    public void AbsorbDamage_WhenShieldAlreadyDepleted_PassesFullDamageThrough()
    {
        Shield shield = CreateShield(0);

        int leftover = shield.AbsorbDamage(8);

        Assert.AreEqual(8, leftover);
    }

    [TestCase(0)]
    [TestCase(-3)]
    public void AbsorbDamage_WithZeroOrNegativeDamage_LeavesShieldUnchanged(int damage)
    {
        Shield shield = CreateShield(10);

        int leftover = shield.AbsorbDamage(damage);

        Assert.AreEqual(damage, leftover);
        Assert.AreEqual(10, shield.shieldHealth);
    }
}
