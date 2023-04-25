using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DefaultBullet : MonoBehaviour, IBullet
{

    public void DestroySelf()
    {
        Destroy(gameObject);
    }



    private void OnTriggerEnter(Collider other)
    {
        //implement player trigger enter and call DestroySelf() afterwards
    }


}
