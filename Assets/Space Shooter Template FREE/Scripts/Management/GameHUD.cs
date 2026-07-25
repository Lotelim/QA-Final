using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHUD : MonoBehaviour
{
    public Text waveText;
    public GameObject bossHealthBarRoot;
    public Slider bossHealthFill;
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
            bossHealthFill.value = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
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
