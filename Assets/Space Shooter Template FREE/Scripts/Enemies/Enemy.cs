using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    #region FIELDS
    [Tooltip("Health points in integer")]
    public int health;

    [Tooltip("Enemy's projectile prefab")]
    public GameObject Projectile;

    [Tooltip("VFX prefab generating after destruction")]
    public GameObject destructionVFX;
    public GameObject hitEffect;
    
    [HideInInspector] public int shotChance; 
    [HideInInspector] public float shotTimeMin, shotTimeMax; 
    #endregion

    [HideInInspector] public int maxHealth;

    public event System.Action<int, int> OnHealthChanged;

    public event System.Action OnDestroyed;

    Shield shield;

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


    void ActivateShooting() 
    {
        if (Random.value < (float)shotChance / 100)                             
        {                         
            Instantiate(Projectile,  gameObject.transform.position, Quaternion.identity);             
        }
    }

    public void GetDamage(int damage)
    {
        if (shield != null)
            damage = shield.AbsorbDamage(damage); 

        if (damage <= 0)
            return; 

        health -= damage;           
        OnHealthChanged?.Invoke(health, maxHealth);
        if (health <= 0)
            Destruction();
        else
            Instantiate(hitEffect,transform.position,Quaternion.identity,transform);
    }

   
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

    void Destruction()
    {
        CancelInvoke(); 
        Instantiate(destructionVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}
