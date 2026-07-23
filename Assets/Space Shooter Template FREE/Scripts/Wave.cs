using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script generates an enemy wave. It defines how many enemies will be emerging, their speed and emerging interval. 
/// It also defines their shooting mode. It defines their moving path.
/// </summary>
[System.Serializable]
public class Shooting
{
    [Range(0,100)]
    [Tooltip("probability with which the ship of this wave will make a shot")]
    public int shotChance;

    [Tooltip("min and max time from the beginning of the path when the enemy can make a shot")]
    public float shotTimeMin, shotTimeMax;
}

public class Wave : MonoBehaviour {

    #region FIELDS
    [Tooltip("Enemy's prefab")]
    public GameObject enemy;

    [Tooltip("a number of enemies in the wave")]
    public int count;

    [Tooltip("path passage speed")]
    public float speed;

    [Tooltip("time between emerging of the enemies in the wave")]
    public float timeBetween;

    [Tooltip("points of the path. delete or add elements to the list if you want to change the number of the points")]
    public Transform[] pathPoints;

    [Tooltip("whether 'Enemy' rotates in path passage direction")]
    public bool rotationByPath;

    [Tooltip("if loop is activated, after completing the path 'Enemy' will return to the starting point")]
    public bool Loop;

    [Tooltip("color of the path in the Editor")]
    public Color pathColor = Color.yellow;
    public Shooting shooting;

    [Tooltip("if testMode is marked the wave will be re-generated after 3 sec")]
    public bool testMode;
    #endregion

    private void Start()
    {
        StartCoroutine(CreateEnemyWave()); 
    }

    IEnumerator CreateEnemyWave() //depending on chosed parameters generating enemies and defining their parameters
    {
        for (int i = 0; i < count; i++)
        {
            if (Player.instance == null) //stop spawning once the player is gone, matching LevelController's wave-spawn guard
                yield break;

            GameObject newEnemy;
            newEnemy = Instantiate(enemy, enemy.transform.position, Quaternion.identity);
            FollowThePath followComponent = newEnemy.GetComponent<FollowThePath>();
            followComponent.path = pathPoints;
            followComponent.speed = speed;
            followComponent.rotationByPath = rotationByPath;
            followComponent.loop = Loop;
            followComponent.SetPath();
            Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
            enemyComponent.shotChance = shooting.shotChance;
            enemyComponent.shotTimeMin = shooting.shotTimeMin;
            enemyComponent.shotTimeMax = shooting.shotTimeMax;
            newEnemy.SetActive(true);
            yield return new WaitForSeconds(timeBetween);
        }
        if (testMode)       //if testMode is activated, waiting for 3 sec and re-generating the wave
        {
            yield return new WaitForSeconds(3);
            StartCoroutine(CreateEnemyWave());
        }
        else if (!Loop)
            Destroy(gameObject); 
    }

    void OnDrawGizmos()  
    {
        DrawPath(pathPoints);  
    }

    void DrawPath(Transform[] path) //drawing the path in the Editor
    {
        Vector3[] pathPositions = new Vector3[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            pathPositions[i] = path[i].position;
        }
        Vector3[] newPathPositions = SplineUtility.PadForCatmullRom(pathPositions);
        Vector3 previosPositions = SplineUtility.Interpolate(newPathPositions, 0);
        Gizmos.color = pathColor;
        int SmoothAmount = path.Length * 20;
        for (int i = 1; i <= SmoothAmount; i++)
        {
            float t = (float)i / SmoothAmount;
            Vector3 currentPositions = SplineUtility.Interpolate(newPathPositions, t);
            Gizmos.DrawLine(currentPositions, previosPositions);
            previosPositions = currentPositions;
        }
    }
}
