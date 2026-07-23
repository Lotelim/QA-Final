using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads the next level's scene once LevelCompletionTracker reports the current level clear.
/// Kept separate from LevelCompletionTracker so the completion-detection logic itself can be
/// unit tested without touching SceneManager.
/// </summary>
[RequireComponent(typeof(LevelCompletionTracker))]
public class LevelFlow : MonoBehaviour
{
    [Tooltip("Scene to load when the level is cleared. Leave empty if this is the last level.")]
    public string nextSceneName;

    [Tooltip("Seconds to wait after level-clear before loading the next scene")]
    public float delayBeforeLoad = 2f;

    private void Awake()
    {
        GetComponent<LevelCompletionTracker>().OnLevelComplete.AddListener(HandleLevelComplete);
    }

    void HandleLevelComplete()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            Invoke(nameof(LoadNextScene), delayBeforeLoad);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
