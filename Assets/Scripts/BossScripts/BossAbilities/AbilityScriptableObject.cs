using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public abstract class AbilityScriptableObject : ScriptableObject
{
    /// <summary>
    /// The bullet speed. Multiplier in percent ( to double the speed, double the value)
    /// </summary>
    [SerializeField]
    private float m_bulletModuleSpeed = 1f;

    [SerializeField]
    private float m_bulletModuleRotationSpeed = 0f;

    /// <summary>
    /// Propability of the ability. Higher value = more likely to use.
    /// </summary>
    [SerializeField]
    public byte abilityPropability = 3;

    /// <summary>
    /// Time in seconds the boss waits before using the next attack.
    /// </summary>
    [SerializeField]
    public int abilityCooldown = 3;

    /// <summary>
    /// Call to use the ability. Should be overwritten in children to define ability.
    /// </summary>
    /// <param name="bossObject"></param>Boss gameobject.
    /// <param name="playerObject"></param>Player gameobject.
    /// <returns></returns>
    public virtual IEnumerator Execute(GameObject bossObject, GameObject playerObject)
    {      
        yield return null; 
    }

    /// <summary>
    /// Will fire the bulletprefab from the firingPosition to the taretPosition. No homing.
    /// </summary>
    /// <param name="targetPosition"></param>Position of the target.
    /// <param name="firingPosition"></param>Position where the bullet will start.
    /// <param name="bulletModulePrefab"></param>Bullet Prefab.
    public void ShootAtPosition(Vector3 targetPosition, Vector3 firingPosition, GameObject bulletModulePrefab, float spawnThreshold )
    {

        Vector3 direction = (targetPosition - firingPosition).normalized;

        GameObject bulletModule = Instantiate(bulletModulePrefab, firingPosition + (targetPosition - firingPosition).normalized * spawnThreshold, Quaternion.LookRotation(direction, Vector3.up));

        AmmunationModule bulletModuleScript = bulletModule.GetComponent<AmmunationModule>();
        bulletModuleScript.direction = direction;
        bulletModuleScript.speed = m_bulletModuleSpeed;
        bulletModuleScript.rotationSpeed = m_bulletModuleRotationSpeed;
    }

    /// <summary>
    /// Create a bullet at the spawnPosition. Used for abilities like "Explosion". Bullet is not fired, only spawned.
    /// </summary>
    /// <param name="spawnPosition"></param>Position where the bullet is spawned.
    /// <returns></returns>
    public GameObject SpawnObjectAtPosition(Vector3 spawnPosition, GameObject spawnPrefab)
    {
        GameObject spawnedObject = Instantiate(spawnPrefab, spawnPosition , Quaternion.identity);

        return spawnedObject;
    }






}
