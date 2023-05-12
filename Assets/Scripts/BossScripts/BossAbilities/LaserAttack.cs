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


    public override IEnumerator Execute(GameObject bossObject, GameObject playerObject)
    {
        yield return SpawnLaser(bossObject, playerObject);

        yield return null;
    }

    public IEnumerator SpawnLaser(GameObject bossObject, GameObject playerObject)
    {
        AudioSource sound = Instantiate(m_warningSound);


        sound.Play();

        yield return new WaitForSeconds(laserDealy);

        Destroy(sound.gameObject);

       GameObject tempLaser = Instantiate(m_laserPrefab, bossObject.transform.position, bossObject.transform.rotation, bossObject.transform);
       

       m_targetLaser = tempLaser.GetComponent<ObjectTargetLaser>();


        m_targetLaser.laserTarget = playerObject;

       m_targetLaser.laser.isAttacking = true;

       yield return new WaitForSeconds(laserDuration);

       tempLaser.GetComponent<ObjectTargetLaser>().laser.isAttacking = false;

        Destroy(tempLaser);

    }
}
