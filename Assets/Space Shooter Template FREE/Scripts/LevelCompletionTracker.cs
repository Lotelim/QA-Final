using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Tracks level-clear progress. Every Enemy spawned during the level (including a Boss,
/// via the same Enemy.OnDestroyed event) registers itself here; once the number of enemies
/// defeated reaches expectedDefeats, OnLevelComplete fires exactly once.
///
/// Counting against a fixed expected total (instead of "no enemies currently alive") avoids
/// falsely completing during the natural gaps between waves, while later waves are still
/// waiting to spawn.
/// </summary>
public class LevelCompletionTracker : MonoBehaviour
{
    public static LevelCompletionTracker instance;

    [Tooltip("Total number of enemies (including any boss) that must be defeated to clear this level")]
    public int expectedDefeats;

    public UnityEvent OnLevelComplete = new UnityEvent();

    readonly HashSet<Enemy> registered = new HashSet<Enemy>();
    int defeatedCount;
    bool completed;

    public int DefeatedCount => defeatedCount;
    public bool IsComplete => completed;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    /// <summary>Registers an enemy whose defeat counts toward clearing this level. Safe to call more than once for the same enemy.</summary>
    public void Register(Enemy enemy)
    {
        if (enemy == null || completed || !registered.Add(enemy))
            return;

        enemy.OnDestroyed += HandleEnemyDefeated;
    }

    void HandleEnemyDefeated()
    {
        if (completed)
            return;

        defeatedCount++;
        if (defeatedCount >= expectedDefeats)
        {
            completed = true;
            OnLevelComplete?.Invoke();
        }
    }
}
