using System;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode tests for the Catmull-Rom spline math shared by Wave (gizmo preview) and
/// FollowThePath (runtime movement). Written before SplineUtility existed, to pin down
/// the exact behavior that used to be duplicated in both call sites.
/// </summary>
public class SplineUtilityTests
{
    static readonly Vector3[] StraightLine =
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(2, 0, 0),
        new Vector3(3, 0, 0),
    };

    static readonly Vector3[] ClosedSquareLoop =
    {
        new Vector3(0, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(1, 1, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, 0, 0), // closes the loop: last point == first point
    };

    static void AssertApprox(Vector3 expected, Vector3 actual, float tolerance = 0.0001f)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(tolerance), "x mismatch");
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(tolerance), "y mismatch");
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(tolerance), "z mismatch");
    }

    [Test]
    public void PadForCatmullRom_WithFewerThanTwoPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() => SplineUtility.PadForCatmullRom(new[] { Vector3.zero }));
    }

    [Test]
    public void PadForCatmullRom_WithNullPath_Throws()
    {
        Assert.Throws<ArgumentException>(() => SplineUtility.PadForCatmullRom(null));
    }

    [Test]
    public void Interpolate_AtT0_ReturnsFirstControlPoint()
    {
        Vector3[] padded = SplineUtility.PadForCatmullRom(StraightLine);
        AssertApprox(StraightLine[0], SplineUtility.Interpolate(padded, 0f));
    }

    [Test]
    public void Interpolate_AtT1_ReturnsLastControlPoint()
    {
        Vector3[] padded = SplineUtility.PadForCatmullRom(StraightLine);
        AssertApprox(StraightLine[StraightLine.Length - 1], SplineUtility.Interpolate(padded, 1f));
    }

    [TestCase(0.25f)]
    [TestCase(0.5f)]
    [TestCase(0.75f)]
    public void Interpolate_OnCollinearPath_StaysOnTheLine(float t)
    {
        Vector3[] padded = SplineUtility.PadForCatmullRom(StraightLine);
        Vector3 result = SplineUtility.Interpolate(padded, t);

        Assert.That(result.y, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(result.z, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(result.x, Is.InRange(0f, 3f));
    }

    [Test]
    public void Interpolate_OnClosedLoop_IsContinuousAcrossTheSeam()
    {
        // For a path whose last point equals its first, the curve should not visibly
        // "jump" when wrapping from t=1 back to t=0 (Wave/FollowThePath both loop on this).
        Vector3[] padded = SplineUtility.PadForCatmullRom(ClosedSquareLoop);
        Vector3 atStart = SplineUtility.Interpolate(padded, 0f);
        Vector3 atEnd = SplineUtility.Interpolate(padded, 1f);

        AssertApprox(atStart, atEnd, 0.01f);
    }

    [TestCase(0f)]
    [TestCase(0.3f)]
    [TestCase(0.6f)]
    [TestCase(1f)]
    public void GetPointOnPath_MatchesPadThenInterpolate(float t)
    {
        Vector3 viaConvenience = SplineUtility.GetPointOnPath(StraightLine, t);
        Vector3 viaTwoSteps = SplineUtility.Interpolate(SplineUtility.PadForCatmullRom(StraightLine), t);

        AssertApprox(viaTwoSteps, viaConvenience);
    }
}
