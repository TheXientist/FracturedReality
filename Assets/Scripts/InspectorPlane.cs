using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InspectorPlane : MonoBehaviour
{
    private void Start()
    {
        GameObject renderPlane = GameObject.Find("RenderPlane");
        transform.parent = SceneView.currentDrawingSceneView.camera.transform;
        transform.localPosition = renderPlane.transform.localPosition;
        transform.localRotation = renderPlane.transform.localRotation;
        transform.localScale = renderPlane.transform.localScale;
    }

}
