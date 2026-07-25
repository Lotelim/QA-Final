using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PoolingControllerTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        PoolingController.instance = null;
        yield return null;
    }

    PoolingController CreatePooler(GameObject prefab, int count)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestPooler");
        var pooler = go.AddComponent<PoolingController>();
        pooler.poolingObjectsClass = new[] { new PoolingObjects { pooledPrefab = prefab, count = count } };
        return pooler;
    }

    [UnityTest]
    public IEnumerator Start_PrewarmsConfiguredCountAsInactiveClones()
    {
        GameObject prefab = TestSceneHelpers.CreatePlaceholder(spawned, "PooledThing");
        PoolingController pooler = CreatePooler(prefab, count: 3);
        yield return null; 

        Assert.AreEqual(3, pooler.transform.childCount);
        for (int i = 0; i < pooler.transform.childCount; i++)
            Assert.IsFalse(pooler.transform.GetChild(i).gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator GetPoolingObject_ReusesAnExistingInactiveInstance()
    {
        GameObject prefab = TestSceneHelpers.CreatePlaceholder(spawned, "PooledThing");
        PoolingController pooler = CreatePooler(prefab, count: 1);
        yield return null;

        int countBefore = pooler.transform.childCount;
        GameObject fetched = pooler.GetPoolingObject(prefab);
        int countAfter = pooler.transform.childCount;

        Assert.AreEqual(countBefore, countAfter, "should reuse the prewarmed instance rather than creating a new one");
        Assert.IsFalse(fetched.activeSelf, "GetPoolingObject itself does not activate the object; callers do");
    }

    [UnityTest]
    public IEnumerator GetPoolingObject_WhenNoInactiveInstanceIsAvailable_CreatesANewOne()
    {
        GameObject prefab = TestSceneHelpers.CreatePlaceholder(spawned, "PooledThing");
        PoolingController pooler = CreatePooler(prefab, count: 1);
        yield return null;

        GameObject first = pooler.GetPoolingObject(prefab);
        first.SetActive(true); 

        int countBefore = pooler.transform.childCount;
        GameObject second = pooler.GetPoolingObject(prefab);
        int countAfter = pooler.transform.childCount;

        Assert.AreEqual(countBefore + 1, countAfter);
        Assert.AreNotSame(first, second);
    }
}
