using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class ViewReset : MonoBehaviour
{
    XRInputSubsystem xrInput;

    // Start is called before the first frame update
    void Start()
    {
        var xrSettings = XRGeneralSettings.Instance;
        var xrManager = xrSettings.Manager;
        var xrLoader = xrManager.activeLoader;
        xrInput = xrLoader.GetLoadedSubsystem<XRInputSubsystem>();
        xrInput.TryRecenter();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            //UnityEngine.XR.InputTracking.Recenter();

            xrInput.TryRecenter();

        }
    }
}
