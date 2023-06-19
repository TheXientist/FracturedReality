using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConLaser : MonoBehaviour
{

    public float maxLength = 16.0f;
    public GameObject hitEffect;
    public Renderer meshRenderer1;
    private bool meshRend1Set;
    public Renderer meshRenderer2;
    private bool meshRend2Set;
    public ParticleSystem[] hitPsArray;
    public int segmentCount = 32;
    public float globalProgressSpeed = 1f;
    public AnimationCurve shaderProgressCurve;
    public AnimationCurve lineWidthCurve;
    public Light pl;
    public float moveHitToSource;

    public bool isAttacking=false;

    private Renderer renderer;
    private LineRenderer lr;
    private IDamageable currentDamageReceiver;
    private GameObject currentBlockingObject;
    private Vector3[] resultVectors;
    private float dist;
    private float globalProgress;
    private Vector3 hitPosition;
    private Vector3 currentPosition;
    private int hittableLayerMask;

    public static bool m_raycastHit = false;
    public static RaycastHit hit;

    void Start()
    {
        globalProgress = 1f;
        lr = GetComponent<LineRenderer>();
        renderer = GetComponent<Renderer>();
        lr.positionCount = segmentCount;
        resultVectors = new Vector3[segmentCount + 1];
        for (int i = 0; i < segmentCount + 1; i++)
        {
            resultVectors[i] = transform.forward;
        }

        meshRend1Set = meshRenderer1 != null;
        meshRend2Set = meshRenderer2 != null;
        hittableLayerMask = LayerMask.GetMask("Spaceship", "Environment");
    }
    
    private void FixedUpdate()
    {
        //Curvy Start

        for (int i = segmentCount - 1; i > 0; i--)
        {
            resultVectors[i] = resultVectors[i - 1];
        }
        resultVectors[0] = transform.forward;
        resultVectors[segmentCount] = resultVectors[segmentCount - 1];
        float blockLength = maxLength / segmentCount;


        currentPosition = new Vector3(0, 0, 0);

        for (int i = 0; i < segmentCount; i++)
        {
            currentPosition = transform.position;
            for (int j = 0; j < i; j++)
            {
                currentPosition += resultVectors[j] * blockLength;
            }
            lr.SetPosition(i, currentPosition);
        }

        //Curvy End



        //Collision Start

            for (int i = 0; i < segmentCount; i++)
            {

                currentPosition = transform.position;
                for (int j = 0; j < i; j++)
                {
                    currentPosition += resultVectors[j] * blockLength;
                }


                m_raycastHit = Physics.Raycast(currentPosition, resultVectors[i], out hit, blockLength, hittableLayerMask);

                if (m_raycastHit)
                {
                    hitPosition = currentPosition + resultVectors[i] * hit.distance;
                    hitPosition = Vector3.MoveTowards(hitPosition, transform.position, moveHitToSource);
                    if (hitEffect)
                    {
                        hitEffect.transform.position = hitPosition;
                    }

                    dist = Vector3.Distance(hitPosition, transform.position);
                    
                    // Only get new IDamageable if hit changed
                    if (!hit.transform.gameObject.Equals(currentBlockingObject))
                    {
                        currentBlockingObject = hit.transform.gameObject;
                        currentDamageReceiver = currentBlockingObject.GetComponent<IDamageable>();
                        
                        Obstacle temp = currentBlockingObject.GetComponent<Obstacle>();

                        if(temp != null)
                        {
                         StartCoroutine(temp.FadeColor());
                        }
                    }
                    
                    break;
                }
            }

            if (!m_raycastHit)
            {
                currentDamageReceiver = null;
                currentBlockingObject = null;
            }
            //Collision End


        //Emit Particles on Collision Start

        if (hitEffect)
        {
            if (globalProgress < 0.75f)
            {
                foreach (ParticleSystem ps in hitPsArray)
                {
                    pl.enabled = true;

                    var em = ps.emission;
                    em.enabled = true;
                    //ps.enableEmission = true;
                }
            }
            else
            {
                foreach (ParticleSystem ps in hitPsArray)
                {
                    pl.enabled = false;

                    var em = ps.emission;
                    em.enabled = false;
                    //ps.enableEmission = false;
                }
            }
        }

        //Emit Particles on Collision End

        renderer.material.SetFloat("_Distance", dist);
        renderer.material.SetVector("_Position", transform.position);

        if (isAttacking)
        {
            globalProgress = 0f;
        }

        if (globalProgress <= 1f)
        {
            globalProgress += Time.deltaTime * globalProgressSpeed;
        }

        if (hitEffect)
        {
            pl.intensity = shaderProgressCurve.Evaluate(globalProgress)*1.5f;
        }        

        float progress = shaderProgressCurve.Evaluate(globalProgress);
        renderer.material.SetFloat("_Progress", progress);

        if (meshRend1Set && meshRend2Set)
        {
            meshRenderer1.material.SetFloat("_Progress", progress);
            meshRenderer2.material.SetFloat("_Progress", progress);
        }       

        float width = lineWidthCurve.Evaluate(globalProgress);
        lr.widthMultiplier = width;

        /*if (Input.GetMouseButtonDown(0) && hitEffect)
        {
            hitPsArray[1].Emit(100);
        }*/

        // Damage
        // TODO: add tickrate (?)
        if (m_raycastHit)
            currentDamageReceiver.TakeDamage(5f);
    }
}
