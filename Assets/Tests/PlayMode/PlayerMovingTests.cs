using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerMovingTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        PlayerMoving.instance = null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator Start_ComputesBordersFromViewportMinusOffsets()
    {
        Camera cam = TestSceneHelpers.CreateMainCamera(spawned);
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestPlayerMoving");
        var moving = go.AddComponent<PlayerMoving>();
        // Borders is a plain [Serializable] class field with no initializer; the Editor/prefab
        // deserialization path that normally populates it never runs for a runtime AddComponent.
        moving.borders = new Borders();

        yield return null; // Start() runs

        float expectedMinX = cam.ViewportToWorldPoint(Vector2.zero).x + moving.borders.minXOffset;
        float expectedMaxX = cam.ViewportToWorldPoint(Vector2.right).x - moving.borders.maxXOffset;
        float expectedMinY = cam.ViewportToWorldPoint(Vector2.zero).y + moving.borders.minYOffset;
        float expectedMaxY = cam.ViewportToWorldPoint(Vector2.up).y - moving.borders.maxYOffset;

        Assert.That(moving.borders.minX, Is.EqualTo(expectedMinX).Within(0.001f));
        Assert.That(moving.borders.maxX, Is.EqualTo(expectedMaxX).Within(0.001f));
        Assert.That(moving.borders.minY, Is.EqualTo(expectedMinY).Within(0.001f));
        Assert.That(moving.borders.maxY, Is.EqualTo(expectedMaxY).Within(0.001f));
    }

    [UnityTest]
    public IEnumerator Update_ClampsPositionInsideBorders_WhenPushedOutOfBounds()
    {
        TestSceneHelpers.CreateMainCamera(spawned);
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestPlayerMoving");
        var moving = go.AddComponent<PlayerMoving>();
        // Borders is a plain [Serializable] class field with no initializer; the Editor/prefab
        // deserialization path that normally populates it never runs for a runtime AddComponent.
        moving.borders = new Borders();

        yield return null; // Start() computes borders

        go.transform.position = new Vector3(moving.borders.maxX + 50f, moving.borders.maxY + 50f, 0);
        yield return null; // Update() should clamp it back inside

        Assert.That(go.transform.position.x, Is.LessThanOrEqualTo(moving.borders.maxX));
        Assert.That(go.transform.position.y, Is.LessThanOrEqualTo(moving.borders.maxY));
    }
}
