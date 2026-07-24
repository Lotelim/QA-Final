using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Serializable classes
[System.Serializable]
public class EnemyWaves 
{
    [Tooltip("time for wave generation from the moment the game started")]
    public float timeToStart;

    [Tooltip("Enemy wave's prefab")]
    public GameObject wave;
}

#endregion

public class LevelController : MonoBehaviour {

    //Serializable classes implements
    public EnemyWaves[] enemyWaves; 

    public GameObject powerUp;
    public float timeForNewPowerup;
    public GameObject[] planets;
    public float timeBetweenPlanets;
    public float planetsSpeed;
    List<GameObject> planetsList = new List<GameObject>();

    [Tooltip("Optional boss to spawn once, after bossSpawnDelay. Leave empty for levels with no boss.")]
    public GameObject boss;
    [Tooltip("Delay in seconds, from level start, before the boss spawns")]
    public float bossSpawnDelay;

    /// <summary>Raised with the 1-based wave number each time a wave actually spawns (e.g. for a "Wave N" HUD).</summary>
    public event System.Action<int> OnWaveStarted;

    /// <summary>Raised with the spawned boss's Enemy component once it's actually instantiated (e.g. for a boss health bar).</summary>
    public event System.Action<Enemy> OnBossSpawned;

    Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        //for each element in 'enemyWaves' array creating coroutine which generates the wave
        for (int i = 0; i<enemyWaves.Length; i++)
        {
            StartCoroutine(CreateEnemyWave(enemyWaves[i].timeToStart, enemyWaves[i].wave, i + 1));
        }
        StartCoroutine(PowerupBonusCreation());
        StartCoroutine(PlanetsCreation());
        if (boss != null)
            StartCoroutine(CreateBoss());
    }

    //spawns the level's boss (if configured) once, after bossSpawnDelay
    IEnumerator CreateBoss()
    {
        yield return new WaitForSeconds(bossSpawnDelay);
        if (Player.instance != null)
        {
            GameObject bossInstance = Instantiate(boss);
            OnBossSpawned?.Invoke(bossInstance.GetComponent<Enemy>());
        }
    }

    //Create a new wave after a delay
    IEnumerator CreateEnemyWave(float delay, GameObject Wave, int waveNumber)
    {
        if (delay != 0)
            yield return new WaitForSeconds(delay);
        if (Player.instance != null)
        {
            Instantiate(Wave);
            OnWaveStarted?.Invoke(waveNumber);
        }
    }

    //endless coroutine generating 'levelUp' bonuses.
    IEnumerator PowerupBonusCreation()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeForNewPowerup);
            if (PlayerMoving.instance == null)
                continue; //player is gone; nothing to bound the spawn position against, skip this cycle
            Instantiate(
                powerUp,
                //Set the position for the new bonus: for X-axis - random position between the borders of 'Player's' movement; for Y-axis - right above the upper screen border
                new Vector2(
                    Random.Range(PlayerMoving.instance.borders.minX, PlayerMoving.instance.borders.maxX),
                    mainCamera.ViewportToWorldPoint(Vector2.up).y + powerUp.GetComponent<Renderer>().bounds.size.y / 2),
                Quaternion.identity
                );
        }
    }

    IEnumerator PlanetsCreation()
    {
        //Create a new list copying the arrey
        for (int i = 0; i < planets.Length; i++)
        {
            planetsList.Add(planets[i]);
        }
        yield return new WaitForSeconds(10);
        while (true)
        {
            ////choose random object from the list, generate and delete it
            int randomIndex = Random.Range(0, planetsList.Count);
            GameObject newPlanet = Instantiate(planetsList[randomIndex]);
            planetsList.RemoveAt(randomIndex);
            //if the list decreased to zero, reinstall it
            if (planetsList.Count == 0)
            {
                for (int i = 0; i < planets.Length; i++)
                {
                    planetsList.Add(planets[i]);
                }
            }
            newPlanet.GetComponent<DirectMoving>().speed = planetsSpeed;

            yield return new WaitForSeconds(timeBetweenPlanets);
        }
    }
}
