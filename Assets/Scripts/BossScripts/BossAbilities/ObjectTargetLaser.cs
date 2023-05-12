using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTargetLaser : MonoBehaviour
{
    public GameObject laserTarget;
    public float speed = 1.0f;
    public ConLaser laser;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {


        Quaternion toRotation = Quaternion.LookRotation(laserTarget.transform.position - transform.position);
        transform.rotation = toRotation;//Quaternion.Lerp(transform.rotation, toRotation, speed * Time.deltaTime);

    }
}
