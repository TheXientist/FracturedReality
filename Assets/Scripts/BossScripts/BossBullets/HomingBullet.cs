using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class HomingBullet : AbstractBullet, IBullet
{
    public GameObject target;
    public float speed;

    public float distance = 0;

    [HideInInspector]
    public Vector3 direction;

    private bool m_homing = false;

    public float homingPercentage = 0;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
        distance = (target.transform.position.magnitude - transform.position.magnitude) * homingPercentage;
        Invoke("DestroySelf", 6);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        if (m_homing)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
        else
        {
            distance -= speed * Time.deltaTime;
            if (distance <= 0)
                m_homing = true;
        }
        
        Vector3 movement = direction * speed * Time.deltaTime;
        transform.position += movement;
    }



    private void OnTriggerEnter(Collider other)
    {
        //implement player trigger enter and call DestroySelf() afterwards
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().TakeDamage(m_DamageValue);
            DestroySelf();
        }
        else if (other.CompareTag("Obstacle"))
        {
            other.GetComponent<IDamageable>().TakeDamage(base.m_DamageValue);
            DestroySelf();
        }

    }

}
