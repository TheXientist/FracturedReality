using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowViewport : MonoBehaviour
{
    [SerializeField] private Transform viewportMarker;
    [SerializeField] private float distanceThreshold;

    [SerializeField] private float followSpeed;

    private bool moving;

    private Rigidbody playerRb;

    private void Start()
    {
        playerRb = FindObjectOfType<Player>().GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        transform.SetPositionAndRotation(viewportMarker.position, viewportMarker.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.activeSelf || moving || Vector3.Magnitude(playerRb.velocity) > 0.1f) return;
        
        if (Vector3.Distance(transform.position, viewportMarker.position) > distanceThreshold)
        {
            StartCoroutine(MoveToViewport());
        }
    }

    private IEnumerator MoveToViewport()
    {
        moving = true;
        
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        for (float i = 0f; i < 100f || Vector3.Distance(transform.position, viewportMarker.position) > distanceThreshold; i += followSpeed)
        {
            transform.position = Vector3.Lerp(startPos, viewportMarker.position, i / 100f);
            transform.rotation = Quaternion.Lerp(startRot, viewportMarker.rotation, i / 100f);
            yield return new WaitForFixedUpdate();
        }

        transform.hasChanged = true;
        moving = false;
    }
}
