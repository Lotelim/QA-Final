using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


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
