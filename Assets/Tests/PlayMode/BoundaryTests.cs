using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BoundaryTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        yield return null;
    }

    Boundary CreateBoundary()
    {
        TestSceneHelpers.CreateMainCamera(spawned);
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestBoundary");
        var collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        return go.AddComponent<Boundary>();
    }

    GameObject CreateTaggedTrigger(string tag, Vector3 position)
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "Tagged_" + tag);
        go.tag = tag;
        go.transform.position = position;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        return go;
    }

    [UnityTest]
    public IEnumerator ProjectileLeavingBounds_IsDestroyed()
    {
        CreateBoundary();
        GameObject projectile = CreateTaggedTrigger("Projectile", Vector3.zero);
        yield return null; 
        yield return new WaitForFixedUpdate(); 

        projectile.transform.position = new Vector3(0, 10000f, 0); 
        yield return new WaitForFixedUpdate(); 
        yield return null;

        Assert.IsTrue(projectile == null);
    }

    [UnityTest]
    public IEnumerator BonusLeavingBounds_IsDestroyed()
    {
        CreateBoundary();
        GameObject bonus = CreateTaggedTrigger("Bonus", Vector3.zero);
        yield return null;
        yield return new WaitForFixedUpdate(); 

        bonus.transform.position = new Vector3(0, 10000f, 0);
        yield return new WaitForFixedUpdate(); 
        yield return null;

        Assert.IsTrue(bonus == null);
    }

    [UnityTest]
    public IEnumerator EnemyLeavingBounds_IsNotTouchedByBoundary()
    {
        CreateBoundary();
        GameObject enemy = CreateTaggedTrigger("Enemy", Vector3.zero);
        yield return null;
        yield return new WaitForFixedUpdate();

        enemy.transform.position = new Vector3(0, 10000f, 0);
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsFalse(enemy == null, "Boundary only cleans up Projectile/Bonus tags, not Enemy");
    }
}
