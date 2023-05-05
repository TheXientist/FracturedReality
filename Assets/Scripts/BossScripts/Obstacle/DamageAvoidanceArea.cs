using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAvoidanceArea : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") 
        {
            //set player immune to fatal damage
        }
        //set player immune to fatal damage
    }
}
