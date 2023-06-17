using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(fileName = "New RandomPattern", menuName = "BossAbilities/RandomPattern")]
public class BulletRandomPattern : AbilityScriptableObject
{
    [SerializeField]
    private GameObject m_bulletPrefab;

    [SerializeField]
    private float m_spawnThreshold = 3f;

    [SerializeField]
    private int m_bulletCount = 5;

    [SerializeField]
    private float m_bulletModuleSpeed = 1f;

    private Vector3 m_startPoint;
    private float m_radius = 1f;

    [SerializeField]
    private float m_circleSteps = 5;

    [SerializeField]
    private int m_circleCount = 10;


    public override IEnumerator Execute(GameObject bossObject, GameObject playerObject)
    {
        InstantiatePattern(bossObject, m_bulletCount, playerObject);
        yield return null;
    }

    public void InstantiatePattern(GameObject bossObject, int bulletCount, GameObject playerObject)
    {
        float tempUpDirection = 0;

        for (int j = 0; j <= m_circleCount; j++)
        {
            if(tempUpDirection == 0)
            {
                CreateBulletCircle(bulletCount, tempUpDirection);
            }
            else
            {
                CreateBulletCircle(bulletCount, tempUpDirection);
                CreateBulletCircle(bulletCount, tempUpDirection * -1);
            }

            tempUpDirection -= m_circleSteps;
        }

        ShootAtPosition(playerObject.transform.position, bossObject.transform.position, m_bulletPrefab, 0,m_bulletModuleSpeed, 0);
    }


    public void CreateBulletCircle(int bulletCount, float upAngle)
    {
        float angleStep = 360f / bulletCount;
        float tempAngle = 0f;
        float randomOffset = Random.Range(0, 25);

        for (int i = 0; i < bulletCount; i++)
        {
            float projectileDirXPosition = m_startPoint.x + Mathf.Sin((tempAngle * Mathf.PI / 180) * m_radius);
            float projectileDirYPosition = m_startPoint.y + Mathf.Cos((tempAngle * Mathf.PI / 180) * m_radius);


            Vector3 projectileVector = new Vector3(projectileDirXPosition, projectileDirYPosition, 0);
            Vector3 projectileMoveDirection = (projectileVector - m_startPoint).normalized * m_bulletModuleSpeed;

            GameObject tempBullet = Instantiate(m_bulletPrefab, m_startPoint, Quaternion.identity);

            Rigidbody rBody = tempBullet.GetComponent<Rigidbody>();

            Vector3 tempSpeed = new Vector3(projectileMoveDirection.x, upAngle, projectileMoveDirection.y).normalized;
            rBody.velocity = tempSpeed * m_bulletModuleSpeed;

            tempAngle += angleStep + randomOffset;
        }
    }   
}
