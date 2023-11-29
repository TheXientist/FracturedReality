using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SingleShotOnPlayerPos", menuName = "BossAbilities/SingleShotOnPlayerPos")]
public class SingleShotOnPlayerPosition : AbilityScriptableObject
{
    /// <summary>
    /// This prefab will be used as a bullet.
    /// </summary>
    [SerializeField]
    private GameObject m_bulletPrefab;
    

    /// <summary>
    /// Distance from the boss, where the object is spawned.
    /// </summary>
    [SerializeField]
    private float m_spawnThreshold = 3f;

    /// <summary>
    /// The bullet speed. Multiplier in percent ( to double the speed, double the value)
    /// </summary>
    [SerializeField]
    private float m_bulletModuleSpeed = 1f;

    [SerializeField]
    private float m_bulletModuleRotationSpeed = 0f;

    /// <summary>
    /// Will fire a single spawned bullet from the bossPosition to the playerPosition.
    /// </summary>
    /// <param name="bossPosition"></param>Current position of the boss.
    /// <param name="playerPosition"></param>Current position of the player.
    /// <returns></returns>
    public override IEnumerator Execute(Transform spawn, Vector3 targetPosition)
    {
        Vector3 m_targetOffset = new Vector3(Random.Range(-30,30), Random.Range(-30, 30), Random.Range(-30, 30));
        //ShootAtPredictedPlayerPosition(targetPosition, SpaceshipController.Instance.GetComponent<Rigidbody>().velocity, spawn.position, m_bulletModuleSpeed, m_bulletPrefab, m_bulletModuleRotationSpeed);
        ShootAtPosition(targetPosition + m_targetOffset, spawn.position, m_bulletPrefab, m_spawnThreshold, m_bulletModuleSpeed, m_bulletModuleRotationSpeed);

        yield return null;
    }

}
