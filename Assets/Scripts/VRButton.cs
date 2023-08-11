using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class VRButton : MonoBehaviour
{
    public bool hover = false;

    public float pressedTimer = 0;


    private Image image;

    public float hoverTimer = 0f;

    public float hoverColoredTime = 1f;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        isPressed();
        Hover();

    }

    public void isHovering()
    {
        hoverTimer = hoverColoredTime;
    }

    private void Hover()
    {
        if (hoverTimer >= 0 && pressedTimer <=0)
        {
            image.color = Color.yellow;
            hoverTimer -= 0.3f;
        }
        else if (hoverTimer < 0f)
        {
            image.color = Color.white;

        }
    }

    public void Pressed()
    {
        print("blue");
        image.color = Color.blue;
    }

    public void isPressed()
    {
        if(pressedTimer > 0)
        {
            pressedTimer -= 0.1f;
        }

    }

    public void ResetPressedTimer()
    {
        pressedTimer = 2f;
    }



}
