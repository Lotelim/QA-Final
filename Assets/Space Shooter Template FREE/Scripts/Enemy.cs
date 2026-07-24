using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script defines 'Enemy's' health and behavior. 
/// </summary>
public class Enemy : MonoBehaviour {

    #region FIELDS
    [Tooltip("Health points in integer")]
    public int health;

    [Tooltip("Enemy's projectile prefab")]
    public GameObject Projectile;

    [Tooltip("VFX prefab generating after destruction")]
    public GameObject destructionVFX;
    public GameObject hitEffect;
    
    [HideInInspector] public int shotChance; //probability of 'Enemy's' shooting during tha path
    [HideInInspector] public float shotTimeMin, shotTimeMax; //max and min time for shooting from the beginning of the path
    #endregion

    /// <summary>Health captured at Awake, before any damage - lets a health bar (e.g. for a Boss) compute a fill ratio.</summary>
    [HideInInspector] public int maxHealth;

    /// <summary>Raised whenever health actually changes from damage: (current, max).</summary>
    public event System.Action<int, int> OnHealthChanged;

    /// <summary>
    /// Raised when this enemy's GameObject is actually destroyed - whether killed (GetDamage
    /// reaching 0 health) or despawned naturally (e.g. FollowThePath reaching the end of its
    /// route with no kill involved). Fires from OnDestroy() rather than only from the lethal-
    /// damage path, so level-completion tracking counts every enemy that ever leaves play, not
    /// just the ones the player actually shot down - most enemies in a real playthrough fly
    /// past and are never killed.
    /// </summary>
    public event System.Action OnDestroyed;

    Shield shield; //optional shield that absorbs damage before it reaches health; null if this enemy has none

    private void Awake()
    {
        shield = GetComponent<Shield>();
        maxHealth = health;
    }

    private void Start()
    {
        Invoke("ActivateShooting", Random.Range(shotTimeMin, shotTimeMax));
        if (LevelCompletionTracker.instance != null)
            LevelCompletionTracker.instance.Register(this);
    }

    //coroutine making a shot
    void ActivateShooting() 
    {
        if (Random.value < (float)shotChance / 100)                             //if random value less than shot probability, making a shot
        {                         
            Instantiate(Projectile,  gameObject.transform.position, Quaternion.identity);             
        }
    }

    //method of getting damage for the 'Enemy'
    public void GetDamage(int damage)
    {
        if (shield != null)
            damage = shield.AbsorbDamage(damage); //shield absorbs what it can; only the overflow reaches health

        if (damage <= 0)
            return; //fully absorbed by the shield, health untouched

        health -= damage;           //reducing health for damage value, if health is less than 0, starting destruction procedure
        OnHealthChanged?.Invoke(health, maxHealth);
        if (health <= 0)
            Destruction();
        else
            Instantiate(hitEffect,transform.position,Quaternion.identity,transform);
    }

    //if 'Enemy' collides 'Player', 'Player' gets the damage equal to projectile's damage value
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && Player.instance != null)
        {
            if (Projectile.GetComponent<Projectile>() != null)
                Player.instance.GetDamage(Projectile.GetComponent<Projectile>().damage);
            else
                Player.instance.GetDamage(1);
        }
    }

    //method of destroying the 'Enemy'
    void Destruction()
    {
        CancelInvoke(); //without this, a pending ActivateShooting Invoke can still fire after this object is destroyed
        Instantiate(destructionVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    //fires for ANY destruction path (killed via Destruction(), or despawned naturally e.g. by
    //FollowThePath reaching its route's end) - see OnDestroyed's summary for why this matters
    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}
