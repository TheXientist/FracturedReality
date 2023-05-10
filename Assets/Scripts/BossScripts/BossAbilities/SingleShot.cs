using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SingleShot", menuName = "BossAbilities/SingleShot")]
public class SingleShot : AbilityScriptableObject
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
    /// Will fire a single spawned bullet from the bossPosition to the playerPosition.
    /// </summary>
    /// <param name="bossPosition"></param>Current position of the boss.
    /// <param name="playerPosition"></param>Current position of the player.
    /// <returns></returns>
    public override IEnumerator Execute(GameObject bossObject, GameObject playerObject)
    {
        ShootAtPosition(playerObject.transform.position, bossObject.transform.position, m_bulletPrefab, m_spawnThreshold);

        yield return null;
    }
}
