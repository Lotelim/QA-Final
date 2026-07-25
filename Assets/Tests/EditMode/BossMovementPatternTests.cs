using NUnit.Framework;
using UnityEngine;


public class BossMovementPatternTests
{
    [TestCase(0.00f, BossDirection.Left)]
    [TestCase(0.19f, BossDirection.Left)]
    [TestCase(0.20f, BossDirection.Right)]
    [TestCase(0.39f, BossDirection.Right)]
    [TestCase(0.40f, BossDirection.Up)]
    [TestCase(0.59f, BossDirection.Up)]
    [TestCase(0.60f, BossDirection.Down)]
    [TestCase(0.79f, BossDirection.Down)]
    [TestCase(0.80f, BossDirection.Idle)]
    [TestCase(0.999f, BossDirection.Idle)]
    public void PickNextDirection_MapsRollToExpectedDirection(float roll, BossDirection expected)
    {
        Assert.AreEqual(expected, BossMovementPattern.PickNextDirection(roll));
    }

    [Test]
    public void PickNextDirection_AtRollOfExactlyOne_ClampsToLastDirectionInsteadOfOverflowing()
    {
        Assert.AreEqual(BossDirection.Idle, BossMovementPattern.PickNextDirection(1f));
    }

    [TestCase(BossDirection.Left, -1, 0)]
    [TestCase(BossDirection.Right, 1, 0)]
    [TestCase(BossDirection.Up, 0, 1)]
    [TestCase(BossDirection.Down, 0, -1)]
    [TestCase(BossDirection.Idle, 0, 0)]
    public void ToVector_MapsDirectionToExpectedUnitVector(BossDirection direction, int x, int y)
    {
        Assert.AreEqual(new Vector2(x, y), BossMovementPattern.ToVector(direction));
    }
}
