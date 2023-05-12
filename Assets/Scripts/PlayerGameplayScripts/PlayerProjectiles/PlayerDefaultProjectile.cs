using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDefaultProjectile : AbstractBullet
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Boss") || other.CompareTag("Obstacle"))
        {
            other.GetComponent<IDamageable>().TakeDamage(base.m_DamageValue);
            Destroy(gameObject);
        }
    }

}
