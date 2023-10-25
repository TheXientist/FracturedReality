using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveObject : MonoBehaviour {

    Material mat;

    private float startTime;
    private float lerpDuration = 4.0f;

    private void Start() {
        mat = GetComponent<Renderer>().material;
    }

    private void Update() {
        //mat.SetFloat("_DissolveAmount", Mathf.Sin(Time.time) / 2 + 0.5f);
    }

    public IEnumerator HideObject()
    {
        float startValue = 0.0f;
        float endValue = 1.0f;
        startTime = Time.time;

        while (Time.time - startTime < lerpDuration)
        {
            float t = (Time.time - startTime) / lerpDuration;
            float lerpedValue = Mathf.Lerp(startValue, endValue, t);

            mat.SetFloat("_DissolveValue", lerpedValue);
            yield return null;
        }
    }

    public IEnumerator ShowObject()
    {
        float startValue = 1.0f;
        float endValue = 0.0f;
        startTime = Time.time;

        while (Time.time - startTime < lerpDuration)
        {
            float t = (Time.time - startTime) / lerpDuration;
            float lerpedValue = Mathf.Lerp(startValue, endValue, t);

            mat.SetFloat("_DissolveValue", lerpedValue);
            yield return null;
        }
    }


}