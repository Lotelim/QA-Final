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

    public event System.Action<int> OnWaveStarted;
    public event System.Action<Enemy> OnBossSpawned;

    Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        for (int i = 0; i<enemyWaves.Length; i++)
        {
            StartCoroutine(CreateEnemyWave(enemyWaves[i].timeToStart, enemyWaves[i].wave, i + 1));
        }
        StartCoroutine(PowerupBonusCreation());
        StartCoroutine(PlanetsCreation());
        if (boss != null)
            StartCoroutine(CreateBoss());
    }

    IEnumerator CreateBoss()
    {
        yield return new WaitForSeconds(bossSpawnDelay);
        if (Player.instance != null)
        {
            GameObject bossInstance = Instantiate(boss);
            OnBossSpawned?.Invoke(bossInstance.GetComponent<Enemy>());
        }
    }

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

    IEnumerator PowerupBonusCreation()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeForNewPowerup);
            if (PlayerMoving.instance == null)
                continue; 
            Instantiate(
                powerUp,
                new Vector2(
                    Random.Range(PlayerMoving.instance.borders.minX, PlayerMoving.instance.borders.maxX),
                    mainCamera.ViewportToWorldPoint(Vector2.up).y + powerUp.GetComponent<Renderer>().bounds.size.y / 2),
                Quaternion.identity
                );
        }
    }

    IEnumerator PlanetsCreation()
    {
        for (int i = 0; i < planets.Length; i++)
        {
            planetsList.Add(planets[i]);
        }
        yield return new WaitForSeconds(10);
        while (true)
        {         
            int randomIndex = Random.Range(0, planetsList.Count);
            GameObject newPlanet = Instantiate(planetsList[randomIndex]);
            planetsList.RemoveAt(randomIndex);
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
