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
        if(other.tag == "Boss")
        {
            other.GetComponent<BossClass>().TakeDamage(base.m_DamageValue);
            Destroy(gameObject);
        }
    }

}
