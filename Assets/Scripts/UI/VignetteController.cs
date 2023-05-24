using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class VignetteController : MonoBehaviour
{
    public Shader vignetteShader;
    private Material vignetteMaterial;
    public float radius, feather;

    private Animator anim;
    private int FLASH_ID;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        FLASH_ID = Animator.StringToHash("Flash");
    }

    private void OnEnable()
    {
        FindObjectOfType<Player>().OnDamaged += Flash;
    }

    private void OnDisable()
    {
        FindObjectOfType<Player>().OnDamaged -= Flash;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (vignetteMaterial == null)
        {
            vignetteMaterial = new Material(vignetteShader);
        }
        
        vignetteMaterial.SetFloat("_Radius", radius);
        vignetteMaterial.SetFloat("_Feather", feather);
        
        Graphics.Blit(src, null, vignetteMaterial);
    }

    private void Flash()
    {
        // Alternative to animator => coroutine / Dotween
        if (anim.GetCurrentAnimatorStateInfo(0).loop) // Idle is the looping state
            anim.SetTrigger(FLASH_ID);
    }
}
