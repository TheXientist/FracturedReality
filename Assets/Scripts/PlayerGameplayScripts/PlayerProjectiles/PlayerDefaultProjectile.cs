using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDefaultProjectile : AbstractBullet
{
    public ParticleSystem particleImpactEffect;
    private MeshRenderer m_ProjectileMesh;
    // Start is called before the first frame update
    void Start()
    {
        m_ProjectileMesh = GetComponent<MeshRenderer>();
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Boss") || other.CompareTag("Obstacle"))
        {
            other.GetComponent<IDamageable>().TakeDamage(base.m_DamageValue);
            StartCoroutine(PlayCollisionEffect());
        }
    }

    private IEnumerator PlayCollisionEffect()
    {
        transform.SetParent(null, true);
        m_ProjectileMesh.enabled = false;
        particleImpactEffect.Play();
        yield return new WaitForSeconds(1);
        DestroySelf();
    }

}
