using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public GameObject destructionFX;

    public static Player instance;

    public event System.Action OnPlayerDied;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null; 
        OnPlayerDied?.Invoke();
    }

    public void GetDamage(int damage)
    {
        Destruction();
    }

    void Destruction()
    {
        Instantiate(destructionFX, transform.position, Quaternion.identity); 
        Destroy(gameObject);
    }
}
















