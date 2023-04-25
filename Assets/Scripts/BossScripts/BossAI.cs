using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : BossClass
{
    // Start is called before the first frame update
    void Start()
    {       
        //start the main coroutine
        StartCoroutine("StartFight");        
    }

    //fight coroutine (Main)
    private IEnumerator StartFight()
    {
        yield return beginFight();
        
        yield return null;
    }


    void Update()
    {
        if(bossCurrentHealth <= 0)
        {
            //boss dead
            Destroy(gameObject);
        }
    }


}
