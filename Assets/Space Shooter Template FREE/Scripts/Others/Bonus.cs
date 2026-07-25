using UnityEngine;

public class Bonus : MonoBehaviour 
{
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.tag == "Player") 
        {
            if (PlayerShooting.instance.weaponPower < PlayerShooting.instance.maxweaponPower)
                PlayerShooting.instance.weaponPower++;
            Destroy(gameObject);
        }
    }
}
