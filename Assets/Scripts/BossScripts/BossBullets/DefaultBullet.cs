using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DefaultBullet : AbstractBullet, IBullet
{

    public void DestroySelf()
    {
        Destroy(gameObject);
    }



    private void OnTriggerEnter(Collider other)
    {
        //implement player trigger enter and call DestroySelf() afterwards
        if(other.CompareTag("Player"))
        {
            other.GetComponent<Player>().TakeDamage(m_DamageValue);
            DestroySelf();
        }
        else if(other.CompareTag("Obstacle"))
        {
            other.GetComponent<IDamageable>().TakeDamage(base.m_DamageValue);
            DestroySelf();
        }

    }


}
