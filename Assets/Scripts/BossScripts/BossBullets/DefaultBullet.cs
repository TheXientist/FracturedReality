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
        if(other.tag == "Player")
        {
            Debug.Log(other.gameObject.name);
            other.GetComponent<Player>().TakeDamage(m_DamageValue);
            DestroySelf();
        }
        else if(other.tag == "Obstacle")
        {
            Debug.Log(other.gameObject.name);
            other.GetComponent<Obstacle>().TakeDamage(base.m_DamageValue);
            DestroySelf();
        }

    }


}
