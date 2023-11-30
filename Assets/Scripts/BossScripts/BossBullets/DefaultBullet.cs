using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DefaultBullet : AbstractBullet, IBullet
{
    public ParticleSystem particleImpactEffect;
    private MeshRenderer m_bulletMesh;
    private AudioSource m_audioSource;

    private void Start()
    {
        m_bulletMesh = GetComponent<MeshRenderer>();
        m_audioSource = GetComponent<AudioSource>();
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
            m_audioSource.Play();
            other.GetComponent<Player>().TakeDamage(m_DamageValue);
            StartCoroutine(PlayCollisionEffect());
            
        }
        else if(other.CompareTag("Obstacle"))
        {
            other.GetComponent<IDamageable>().TakeDamage(base.m_DamageValue);
            StartCoroutine(PlayCollisionEffect());

        }
    }

    public IEnumerator PlayCollisionEffect()
    {
        GetComponent<Collider>().enabled = false;
        transform.SetParent(null, true);
        m_bulletMesh.enabled = false;
        particleImpactEffect.Play();
        yield return new WaitForSeconds(1);
        DestroySelf();
    }


}
