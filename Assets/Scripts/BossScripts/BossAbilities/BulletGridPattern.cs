using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BulletGridPattern", menuName = "BossAbilities/BulletGridPattern")]
public class BulletGridPattern : AbilityScriptableObject
{
    [SerializeField]
    private GameObject m_bulletPrefab;

    public int GridSize = 5;

    public int bulletOffset = 2;

    public float bulletSpeed = 50;

    public int bulletSpawnPropability = 30;

    public override IEnumerator Execute(Transform spawn, Vector3 targetPosition)
    {
        ShootGridAtPosition(targetPosition, spawn.position, m_bulletPrefab, 0, bulletSpeed, 20);

        yield return null;
    }

    public void ShootGridAtPosition(Vector3 targetPosition, Vector3 firingPosition, GameObject bulletModulePrefab, float spawnThreshold, float bulletModuleSpeed, float bulletModuleRotationSpeed)
    {
        Vector3 direction = (targetPosition - firingPosition).normalized;

        GameObject bulletModule = Instantiate(bulletModulePrefab, firingPosition + (targetPosition - firingPosition).normalized * spawnThreshold, Quaternion.LookRotation(direction, Vector3.up));

        GameObject bulletParent = Instantiate(bulletModulePrefab, bulletModule.transform.position, bulletModule.transform.rotation, bulletModule.transform);

        for (int i = 0; i < GridSize; i++)
        {
            for(int j = 0; j < GridSize; j++)
            {
                int randomNumber = Random.Range(0, 100);
                
                if(randomNumber < bulletSpawnPropability)
                {
                    GameObject tempChild = Instantiate(bulletModulePrefab, bulletModule.transform.position, bulletModule.transform.rotation, bulletParent.transform);

                    tempChild.transform.localPosition += new Vector3(j * bulletOffset, i * bulletOffset, 0);
                }
           
            }
        }
        float offsetRatio = bulletOffset  * (GridSize-2);

        bulletParent.transform.localPosition += new Vector3( (-GridSize-offsetRatio) / 2, (-GridSize - offsetRatio) / 2, 0);

        AmmunationModule bulletModuleScript = bulletModule.GetComponent<AmmunationModule>();
        bulletModuleScript.direction = direction;
        bulletModuleScript.speed = bulletModuleSpeed;
        bulletModuleScript.rotationSpeed = bulletModuleRotationSpeed;
    }
}
