using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(fileName = "New RandomHemispherePattern", menuName = "BossAbilities/RandomHemispherePattern")]
public class RandomHemispherePattern : AbilityScriptableObject
{
    [SerializeField]
    private GameObject m_bulletPrefab;

    [SerializeField]
    private int m_bulletCount = 5;

    [SerializeField]
    private float m_bulletModuleSpeed = 1f;

    [SerializeField] private float spawnRadius;


    public override IEnumerator Execute(Transform spawn, Vector3 targetPosition)
    {
        InstantiatePattern(spawn, m_bulletCount, targetPosition);
        yield return null;
    }

    public void InstantiatePattern(Transform spawn, int bulletCount, Vector3 targetPos)
    {
        Vector3 spawnPos = spawn.position;
        Quaternion rotationOffset = Quaternion.FromToRotation(Vector3.forward, (targetPos - spawnPos).normalized);

        for (int j = 0; j <= bulletCount; j++)
        {
            float azimuth = 2f * Mathf.PI * Random.value;
            float zenith = Mathf.Asin(Mathf.Sqrt(Random.value));

            Vector3 targetOnHemisphere = new Vector3(
                Mathf.Sin(zenith) * Mathf.Cos(azimuth),
                Mathf.Sin(zenith) * Mathf.Sin(azimuth),
                Mathf.Cos(zenith));

            Vector3 targetDirection = rotationOffset * targetOnHemisphere;
            
            ShootAtPosition(spawnPos + (spawnRadius+1) * targetDirection, spawnPos, m_bulletPrefab, 0, m_bulletModuleSpeed, 0);
        }

        ShootAtPosition(targetPos, spawnPos + spawnRadius * (targetPos - spawnPos).normalized, m_bulletPrefab, 0,m_bulletModuleSpeed, 0);
    }
}
