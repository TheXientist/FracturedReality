using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SidewayHoming", menuName = "BossAbilities/SidewayHoming")]
public class SidewayHomingAttack : AbilityScriptableObject
{
    public float firingAngle = 70;

    public float bulletModuleSpeed = 80;

    [SerializeField]
    private GameObject m_bulletPrefab;

    public float homingPercentage = 0.9f;


    public override IEnumerator Execute(GameObject bossObject, GameObject playerObject)
    {
        ShootHomingAtPosition(playerObject.transform.position, bossObject.transform.position, m_bulletPrefab, true);
        ShootHomingAtPosition(playerObject.transform.position, bossObject.transform.position, m_bulletPrefab, false);
        firingAngle *= -1;
        ShootHomingAtPosition(playerObject.transform.position, bossObject.transform.position, m_bulletPrefab, false);
        ShootHomingAtPosition(playerObject.transform.position, bossObject.transform.position, m_bulletPrefab, true);

        yield return null;
    }

    public void ShootHomingAtPosition(Vector3 targetPosition, Vector3 firingPosition, GameObject bulletModulePrefab, bool inXAngle)
    {
        if(inXAngle)
        {
            targetPosition += new Vector3(firingAngle, 0, 0);
        }
        else
        {
            targetPosition += new Vector3(0, firingAngle, 0);
        }

        Vector3 direction = (targetPosition - firingPosition).normalized;

        GameObject bulletModule = Instantiate(bulletModulePrefab, firingPosition + (targetPosition - firingPosition).normalized , Quaternion.LookRotation(direction, Vector3.up));

        HomingBullet bulletHomingScript = bulletModule.GetComponent<HomingBullet>();
        bulletHomingScript.direction = direction;
        bulletHomingScript.speed = bulletModuleSpeed;
        bulletHomingScript.homingPercentage = homingPercentage;
    }

}
