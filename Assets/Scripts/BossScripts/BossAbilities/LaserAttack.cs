using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New LaserAttack", menuName = "BossAbilities/LaserAttack")]
public class LaserAttack : AbilityScriptableObject
{
    /// <summary>
    /// This prefab will be used as a laser.
    /// </summary>
    [SerializeField]
    private GameObject m_laserPrefab;

    public float laserDuration = 4f;

    [SerializeField]
    private float m_spawnThreshold = 3f;

    private ObjectTargetLaser m_targetLaser;

    public float laserDealy = 5;

    [SerializeField]
    private AudioSource m_warningSound;

    private AudioSource m_currentSound;

    private GameObject m_currentLaserObject;

    public override IEnumerator Execute(GameObject bossObject, GameObject playerObject)
    {
        yield return SpawnLaser(bossObject, playerObject);

        yield return null;
    }

    public IEnumerator SpawnLaser(GameObject bossObject, GameObject playerObject)
    {
        m_currentSound = Instantiate(m_warningSound);

        m_currentSound.Play();

        yield return new WaitForSeconds(laserDealy);

        Destroy(m_currentSound.gameObject);
       
        m_currentLaserObject = Instantiate(m_laserPrefab, bossObject.transform.position, bossObject.transform.rotation);//, bossObject.transform);

        m_targetLaser = m_currentLaserObject.GetComponent<ObjectTargetLaser>();

        m_targetLaser.laserTarget = playerObject;

        m_targetLaser.laser.isAttacking = true;

        yield return new WaitForSeconds(laserDuration);

        m_targetLaser.laser.isAttacking = false;

        Destroy(m_currentLaserObject);       
    }

    public override void InterruptCurrentAbility()
    {
        if (m_currentSound != null)
        {
            Destroy(m_currentSound.gameObject);
        }

        if (m_currentLaserObject != null)
        {
            Destroy(m_currentLaserObject);
        }
    }
}
