using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class VisualEffectTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DestroysGameObject_AfterConfiguredDestructionTime()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestVFX");
        var vfx = go.AddComponent<VisualEffect>();
        vfx.destructionTime = 0.05f;

        yield return null; // OnEnable starts the destruction coroutine
        Assert.IsFalse(go == null, "should still be alive immediately after enabling");

        yield return new WaitForSeconds(0.2f);

        Assert.IsTrue(go == null, "should be destroyed once destructionTime has elapsed");
    }
}
