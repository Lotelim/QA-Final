using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the on-screen HUD: current wave number, a boss health bar (shown only while a boss
/// is alive), and win/lose overlays with a restart button. Wires itself to whatever
/// LevelController/Player/LevelCompletionTracker exist in the scene.
///
/// The LevelController subscription happens in Awake(), not Start(): Unity guarantees every
/// object's Awake() completes before any object's Start() begins, and LevelController only
/// ever fires its events from within its own Start() (a zero-delay first wave fires "wave
/// started" synchronously from inside LevelController.Start() itself) - so subscribing in
/// Awake() is the only ordering-independent way to guarantee this HUD never misses that first
/// event. Player.instance/LevelCompletionTracker.instance, by contrast, are set in their own
/// Awake() calls, so this HUD only needs Start() (which runs after every Awake()) to safely
/// read them, regardless of GameObject/component ordering in the scene.
/// (Script Execution Order was tried first and does NOT work here - it only orders the
/// Update-family callbacks, not Awake/Start.)
/// </summary>
public class GameHUD : MonoBehaviour
{
    public Text waveText;
    public GameObject bossHealthBarRoot;
    public Image bossHealthFill;
    public GameObject winScreenRoot;
    public GameObject loseScreenRoot;

    [Tooltip("Scene to load when the Restart button is pressed")]
    public string restartSceneName = "Level_1";

    private void Awake()
    {
        var levelController = FindFirstObjectByType<LevelController>();
        if (levelController != null)
        {
            levelController.OnWaveStarted += HandleWaveStarted;
            levelController.OnBossSpawned += HandleBossSpawned;
        }
    }

    private void Start()
    {
        if (bossHealthBarRoot != null)
            bossHealthBarRoot.SetActive(false);
        if (winScreenRoot != null)
            winScreenRoot.SetActive(false);
        if (loseScreenRoot != null)
            loseScreenRoot.SetActive(false);

        if (Player.instance != null)
            Player.instance.OnPlayerDied += HandlePlayerDied;

        if (LevelCompletionTracker.instance != null)
            LevelCompletionTracker.instance.OnLevelComplete.AddListener(HandleLevelComplete);
    }

    void HandleWaveStarted(int waveNumber)
    {
        if (waveText != null)
            waveText.text = "Wave " + waveNumber;
    }

    void HandleBossSpawned(Enemy bossEnemy)
    {
        if (bossHealthBarRoot != null)
            bossHealthBarRoot.SetActive(true);
        bossEnemy.OnHealthChanged += HandleBossHealthChanged;
        bossEnemy.OnDestroyed += HandleBossDefeated;
        HandleBossHealthChanged(bossEnemy.health, bossEnemy.maxHealth);
    }

    void HandleBossHealthChanged(int current, int max)
    {
        if (bossHealthFill != null)
            bossHealthFill.fillAmount = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
    }

    void HandleBossDefeated()
    {
        if (bossHealthBarRoot != null)
            bossHealthBarRoot.SetActive(false);
    }

    void HandlePlayerDied()
    {
        if (loseScreenRoot != null)
            loseScreenRoot.SetActive(true);
    }

    void HandleLevelComplete()
    {
        // Only the last level in the chain (no next scene configured) shows a win screen;
        // earlier levels just transition via LevelFlow instead.
        var flow = FindFirstObjectByType<LevelFlow>();
        bool isLastLevel = flow == null || string.IsNullOrEmpty(flow.nextSceneName);
        if (isLastLevel && winScreenRoot != null)
            winScreenRoot.SetActive(true);
    }

    public void RestartFromLevel1()
    {
        SceneManager.LoadScene(restartSceneName);
    }
}
