using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    [Tooltip("Damage which a projectile deals to another object. Integer")]
    public int damage;

    [Tooltip("Whether the projectile belongs to the ‘Enemy’ or to the ‘Player’")]
    public bool enemyBullet;

    [Tooltip("Whether the projectile is destroyed in the collision, or not")]
    public bool destroyedByCollision;

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (enemyBullet && collision.tag == "Player" && Player.instance != null) 
        {
            Player.instance.GetDamage(damage);
            if (destroyedByCollision)
                Destruction();
        }
        else if (!enemyBullet && collision.tag == "Enemy")
        {
            collision.GetComponent<Enemy>().GetDamage(damage);
            if (destroyedByCollision)
                Destruction();
        }
    }

    void Destruction() 
    {
        Destroy(gameObject);
    }
}


