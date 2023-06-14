using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTargetLaser : MonoBehaviour
{
    public GameObject laserTarget;
    public float speed = 1.0f;
    public ConLaser laser;

    // This can be Update(), as it's definitely not the bottleneck
    void Update()
    {
        Quaternion toRotation = Quaternion.LookRotation(laserTarget.transform.position - transform.position);
        transform.rotation = toRotation;//Quaternion.Lerp(transform.rotation, toRotation, speed * Time.deltaTime);
    }
}
