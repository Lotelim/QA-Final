using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Smoke tests that load the actual generated Level_1/Level_2 scenes and verify the content
/// LevelContentBuilder wired up is really there: level-completion tracking, the Level_2 shielded
/// wave and boss, and correct scene-transition targets.
///
/// These load real scenes via SceneManager and are run in isolation (a dedicated -testFilter
/// pass, not part of the main regression suite) since loading a full gameplay scene starts real
/// Player/PlayerMoving/PoolingController singletons that would otherwise leak into other fixtures.
/// </summary>
public class LevelSceneLoadTests
{
    static int cleanupSceneCounter;

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Player.instance = null;
        PlayerMoving.instance = null;
        PlayerShooting.instance = null;
        PoolingController.instance = null;
        LevelCompletionTracker.instance = null;

        // Loading Level_1/Level_2 leaves their real Main Camera (and everything else) as the
        // active scene content for the rest of the run - e.g. it would make Camera.main resolve
        // to the wrong camera in every later PlayMode fixture. Swap to a fresh empty scene and
        // unload the level scene so later tests get a clean slate again.
        Scene loadedLevelScene = SceneManager.GetActiveScene();
        Scene cleanScene = SceneManager.CreateScene("LevelSceneLoadTests_Cleanup_" + cleanupSceneCounter++);
        SceneManager.SetActiveScene(cleanScene);
        if (loadedLevelScene.IsValid() && loadedLevelScene != cleanScene)
            yield return SceneManager.UnloadSceneAsync(loadedLevelScene);
    }

    [UnityTest]
    public IEnumerator Level1_Loads_WithCompletionTrackingWiredToLevel2()
    {
        yield return SceneManager.LoadSceneAsync("Level_1", LoadSceneMode.Single);
        yield return null;

        GameObject gameController = GameObject.Find("Game_Controller");
        Assert.IsNotNull(gameController, "Level_1 should contain a Game_Controller object");

        var levelController = gameController.GetComponentInChildren<LevelController>();
        Assert.IsNotNull(levelController);
        Assert.IsNull(levelController.boss, "Level_1 should not have a boss");

        var tracker = gameController.GetComponent<LevelCompletionTracker>();
        Assert.IsNotNull(tracker, "Level_1 should have level-completion tracking wired up");
        Assert.Greater(tracker.expectedDefeats, 0);

        var flow = gameController.GetComponent<LevelFlow>();
        Assert.IsNotNull(flow);
        Assert.AreEqual("Level_2", flow.nextSceneName);

        Assert.IsNotNull(GameObject.FindWithTag("Player"), "Level_1 should have a Player");
    }

    [UnityTest]
    public IEnumerator Level2_Loads_WithShieldedWaveAndHugeHpBoss()
    {
        yield return SceneManager.LoadSceneAsync("Level_2", LoadSceneMode.Single);
        yield return null;

        GameObject gameController = GameObject.Find("Game_Controller");
        Assert.IsNotNull(gameController);

        var levelController = gameController.GetComponentInChildren<LevelController>();
        Assert.IsNotNull(levelController.boss, "Level_2 should have a boss configured");
        Assert.IsTrue(levelController.enemyWaves.Length > 0);

        bool hasShieldedWave = false;
        foreach (var ew in levelController.enemyWaves)
        {
            Wave wave = ew.wave.GetComponent<Wave>();
            if (wave.enemy.GetComponent<Shield>() != null)
                hasShieldedWave = true;
        }
        Assert.IsTrue(hasShieldedWave, "Level_2 should include at least one wave of shielded enemies");

        Assert.IsNotNull(levelController.boss.GetComponent<Boss>());
        Assert.IsNotNull(levelController.boss.GetComponent<BossMovement>());
        Assert.Greater(levelController.boss.GetComponent<Enemy>().health, 100, "boss should have huge HP");

        var tracker = gameController.GetComponent<LevelCompletionTracker>();
        Assert.IsNotNull(tracker);
        Assert.Greater(tracker.expectedDefeats, 0);

        var flow = gameController.GetComponent<LevelFlow>();
        Assert.IsNotNull(flow);
        Assert.IsTrue(string.IsNullOrEmpty(flow.nextSceneName), "Level_2 is the last level for now");
    }
}
