using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    public float movespeed = 2f;
    public float boostSpeed = 5f;
    Vector2 lastMousePos;

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivity = 1f;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationY = 0F;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void Update()
    {
        if (axes == RotationAxes.MouseXAndY)
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;

            rotationY += Input.GetAxis("Mouse Y") * sensitivity;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }
        else if (axes == RotationAxes.MouseX)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivity, 0);
        }
        else
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivity;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
        }

        float boosting = 1;
        if (Input.GetKey("left shift"))
        {
            boosting = boostSpeed;
        }

        if (Input.GetKey("w"))
        {
            gameObject.transform.position += gameObject.transform.forward * movespeed * boosting * Time.deltaTime;
        }
        else if (Input.GetKey("s"))
        {
            gameObject.transform.position -= gameObject.transform.forward * movespeed * boosting * Time.deltaTime;
        }

        if (Input.GetKey("a"))
        {
            gameObject.transform.position -= gameObject.transform.right * movespeed * boosting * Time.deltaTime;
        }
        else if (Input.GetKey("d"))
        {
            gameObject.transform.position += gameObject.transform.right * movespeed * boosting * Time.deltaTime;
        }

        if (Input.GetKey("space"))
        {
            gameObject.transform.position += gameObject.transform.up * movespeed * boosting * Time.deltaTime;
        }
        else if (Input.GetKey("left ctrl"))
        {
            gameObject.transform.position -= gameObject.transform.up * movespeed * boosting * Time.deltaTime;
        }
    }
}
