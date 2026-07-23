using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RepeatingBackgroundTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Update_WhenBelowVerticalSize_RepositionsUpByTwiceVerticalSize()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestBackground");
        var bg = go.AddComponent<RepeatingBackground>();
        bg.verticalSize = 5f;
        go.transform.position = new Vector3(1, -5.1f, 0); // just below -verticalSize

        yield return null; // Update() runs

        Assert.That(go.transform.position.y, Is.EqualTo(-5.1f + 10f).Within(0.001f));
        Assert.That(go.transform.position.x, Is.EqualTo(1f).Within(0.001f), "reposition should only affect Y");
    }

    [UnityTest]
    public IEnumerator Update_WhenAboveNegativeVerticalSize_DoesNotReposition()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestBackground");
        var bg = go.AddComponent<RepeatingBackground>();
        bg.verticalSize = 5f;
        go.transform.position = new Vector3(0, 0f, 0);

        yield return null;

        Assert.That(go.transform.position.y, Is.EqualTo(0f).Within(0.001f));
    }
}
