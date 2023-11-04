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
        //IMPLEMENT ADAPTIVE DAMAGE
        
        if(other.CompareTag("Boss"))
        {
            other.GetComponent<IDamageable>().TakeDamage(other.GetComponent<BossAI>().BossCurrentHealth / Mathf.Max(1f, LevelManager.instance.RemainingTime));
            StartCoroutine(PlayCollisionEffect());
        }

        if(other.CompareTag("Obstacle"))
        {
            other.GetComponent<IDamageable>().TakeDamage(m_DamageValue);
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
