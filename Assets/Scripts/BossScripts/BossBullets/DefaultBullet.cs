using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DefaultBullet : AbstractBullet, IBullet
{
    public ParticleSystem particleImpactEffect;
    private MeshRenderer m_bulletMesh;

    private void Start()
    {
        m_bulletMesh = GetComponent<MeshRenderer>();
    }

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
            StartCoroutine(PlayCollisionEffect());
            
        }
        else if(other.CompareTag("Obstacle"))
        {
            other.GetComponent<IDamageable>().TakeDamage(base.m_DamageValue);
            StartCoroutine(PlayCollisionEffect());

        }
    }

    private IEnumerator PlayCollisionEffect()
    {
        transform.SetParent(null, true);
        m_bulletMesh.enabled = false;
        particleImpactEffect.Play();
        yield return new WaitForSeconds(1);
        DestroySelf();
    }


}
