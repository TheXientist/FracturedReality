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

    private bool shootNextLeft;


    public override IEnumerator Execute(Transform spawn, Vector3 targetPosition)
    {
        ShootHomingAtPosition(targetPosition, spawn.position);

        yield return null;
    }

    public void ShootHomingAtPosition(Vector3 targetPos, Vector3 firingPosition)
    {
        firingAngle = Random.Range(0, 360);

        var player = SpaceshipController.Instance.GetComponent<Rigidbody>();
        Vector3 interceptPosition = Intercept(targetPos,
            player.velocity, firingPosition, bulletModuleSpeed);
        
        // Aim left or right of predicted interception point
        Vector3 targetPosition = interceptPosition + (shootNextLeft ? 1 : -1) * firingAngle * player.transform.right; 
        shootNextLeft = !shootNextLeft;

        Vector3 direction = (targetPosition - firingPosition).normalized;

        GameObject bulletModule = Instantiate(m_bulletPrefab, firingPosition + (targetPosition - firingPosition).normalized , Quaternion.LookRotation(direction, Vector3.up));

        HomingBullet bulletHomingScript = bulletModule.GetComponent<HomingBullet>();
        bulletHomingScript.direction = direction;
        bulletHomingScript.speed = bulletModuleSpeed;
        bulletHomingScript.homingPercentage = homingPercentage;
    }

}
