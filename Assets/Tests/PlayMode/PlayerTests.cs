using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTests
{
    readonly List<GameObject> spawned = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestSceneHelpers.DestroyAll(spawned);
        Player.instance = null;
        yield return null;
    }

    Player CreatePlayer()
    {
        GameObject go = TestSceneHelpers.CreatePlaceholder(spawned, "TestPlayer");
        var player = go.AddComponent<Player>();
        player.destructionFX = TestSceneHelpers.CreatePlaceholder(spawned, "DestructionFX");
        return player;
    }

    [UnityTest]
    public IEnumerator Awake_SetsStaticInstance()
    {
        Player player = CreatePlayer();
        yield return null;

        Assert.AreSame(player, Player.instance);
    }

    [UnityTest]
    public IEnumerator GetDamage_AnyAmount_DestroysThePlayer()
    {
        // Destroy() is deferred to end-of-frame, so each case needs a yield before checking
        // for null; looping in one coroutine (rather than [UnityTest]+[TestCase], which this
        // Test Framework version doesn't support together) covers 1/999/0 damage.
        foreach (int damage in new[] { 1, 999, 0 })
        {
            Player player = CreatePlayer();
            yield return null;

            player.GetDamage(damage);
            yield return null;

            Assert.IsTrue(player == null, $"damage={damage} should destroy the player");
        }
    }

    [UnityTest]
    public IEnumerator OnDestroy_ClearsStaticInstance_SoLaterCallersDoNotHitADeadObject()
    {
        Player player = CreatePlayer();
        yield return null;
        Assert.AreSame(player, Player.instance);

        player.GetDamage(1); // destroys the player
        yield return null; // Destroy() (and thus OnDestroy) is deferred to end-of-frame

        Assert.IsTrue(Player.instance == null, "Player.instance should be cleared once the player is destroyed");
    }

    [UnityTest]
    public IEnumerator OnDestroy_LetsANewPlayerTakeOver_AsHappensOnALevelTransition()
    {
        // Regression test for the actual failure mode: Level 1's Player is destroyed when its
        // scene unloads, then Level 2's Player Awakes. Without clearing the static instance on
        // destroy, Awake's "if (instance == null)" guard would stay false forever (instance still
        // pointing at the old, destroyed Player), and the new level's Player would never become
        // reachable via Player.instance for the rest of the game.
        Player levelOnePlayer = CreatePlayer();
        yield return null;
        Assert.AreSame(levelOnePlayer, Player.instance);

        levelOnePlayer.GetDamage(1); // simulates the old scene's Player going away
        yield return null; // let the old Player actually finish being destroyed first, as a real scene unload would

        Player levelTwoPlayer = CreatePlayer();
        yield return null;

        Assert.AreSame(levelTwoPlayer, Player.instance, "the new level's Player should become the active instance");
    }

    [UnityTest]
    public IEnumerator OnPlayerDied_FiresWhenThePlayerIsDestroyed()
    {
        Player player = CreatePlayer();
        yield return null;

        bool died = false;
        player.OnPlayerDied += () => died = true;

        player.GetDamage(1);
        yield return null; // Destroy() (and thus OnDestroy) is deferred to end-of-frame

        Assert.IsTrue(died);
    }
}
