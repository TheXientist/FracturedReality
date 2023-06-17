using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public abstract class AbilityScriptableObject : ScriptableObject
{  
    /// <summary>
    /// Propability of the ability. Higher value = more likely to use.
    /// </summary>
    [SerializeField]
    public byte abilityPropability = 3;

    /// <summary>
    /// Time in seconds the boss waits before using this attack.
    /// </summary>
    [SerializeField]
    public int abilityWindup = 0;
    
    /// <summary>
    /// Time in seconds the boss waits before continuing to the next attack.
    /// </summary>
    [SerializeField]
    public int abilityCooldown = 3;

    [SerializeField] public string animationTriggerName;
    
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

    public virtual void InterruptCurrentAbility(){}

    /// <summary>
    /// Will fire the bulletprefab from the firingPosition to the taretPosition. No homing.
    /// </summary>
    /// <param name="targetPosition"></param>Position of the target.
    /// <param name="firingPosition"></param>Position where the bullet will start.
    /// <param name="bulletModulePrefab"></param>Bullet Prefab.
    public void ShootAtPosition(Vector3 targetPosition, Vector3 firingPosition, GameObject bulletModulePrefab, float spawnThreshold, float bulletModuleSpeed, float bulletModuleRotationSpeed)
    {
        Vector3 direction = (targetPosition - firingPosition).normalized;

        GameObject bulletModule = Instantiate(bulletModulePrefab, firingPosition + (targetPosition - firingPosition).normalized * spawnThreshold, Quaternion.LookRotation(direction, Vector3.up));

        AmmunationModule bulletModuleScript = bulletModule.GetComponent<AmmunationModule>();
        bulletModuleScript.direction = direction;
        bulletModuleScript.speed = bulletModuleSpeed;
        bulletModuleScript.rotationSpeed = bulletModuleRotationSpeed;
    }

    public void ShootAtPredictedPlayerPosition(Vector3 targetPosition, Vector3 targetSpeed, Vector3 attackerPosition, float bulletSpeed, GameObject bulletModulePrefab, float bulletModuleRotationSpeed)
    {
        Vector3 interceptionPoint = Intercept(targetPosition, targetSpeed, attackerPosition, bulletSpeed);
        Vector3 direction = (interceptionPoint - attackerPosition).normalized;

        if(direction != null)
        {
            GameObject bulletModule = Instantiate(bulletModulePrefab, attackerPosition, Quaternion.LookRotation(direction, Vector3.up));

            AmmunationModule bulletModuleScript = bulletModule.GetComponent<AmmunationModule>();
            bulletModuleScript.direction = direction;
            bulletModuleScript.speed = bulletSpeed;
            bulletModuleScript.rotationSpeed = bulletModuleRotationSpeed;
        }

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

    public static Vector3 Intercept(Vector3 targetPosition, Vector3 targetSpeed, Vector3 attackerPosition, float bulletSpeed)
    {
        Vector3 q = targetPosition - attackerPosition;

        //solving quadratic equation from t*t(Vx*Vx + Vy*Vy - S*S) + 2*t*(Vx*Qx)(Vy*Qy) + Qx*Qx + Qy*Qy = 0

        float a = Vector3.Dot(targetSpeed, targetSpeed) - (bulletSpeed * bulletSpeed); //Dot is basically (targetSpeed.x * targetSpeed.x) + (targetSpeed.y * targetSpeed.y)
        float b = 2 * Vector3.Dot(targetSpeed, q); //Dot is basically (targetSpeed.x * q.x) + (targetSpeed.y * q.y)
        float c = Vector3.Dot(q, q); //Dot is basically (q.x * q.x) + (q.y * q.y)

        //Discriminant
        float D = Mathf.Sqrt((b * b) - 4 * a * c);

        if (D < 0) return Vector3.zero; //negative discriminant means no possible intercept

        float t1 = (-b + D) / (2 * a);
        float t2 = (-b - D) / (2 * a);

        //Debug.Log("t1: " + t1 + " t2: " + t2);

        float time = Mathf.Max(t1, t2);

        if (time <= 0) return Vector3.zero; //negative time means collision lies in the past

        Vector3 ret = targetPosition + targetSpeed * time;
        return ret;
    }
}
