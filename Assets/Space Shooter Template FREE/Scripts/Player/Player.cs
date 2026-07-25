using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script defines which sprite the 'Player" uses and its health.
/// </summary>

public class Player : MonoBehaviour
{
    public GameObject destructionFX;

    public static Player instance;

    /// <summary>Raised when the Player's GameObject is actually destroyed (e.g. for a "GAME OVER" screen).</summary>
    public event System.Action OnPlayerDied;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null; //without this, other scripts calling Player.instance after death would hit a destroyed object
        OnPlayerDied?.Invoke();
    }

    //method for damage proceccing by 'Player'
    public void GetDamage(int damage)
    {
        Destruction();
    }

    //'Player's' destruction procedure
    void Destruction()
    {
        Instantiate(destructionFX, transform.position, Quaternion.identity); //generating destruction visual effect and destroying the 'Player' object
        Destroy(gameObject);
    }
}
















