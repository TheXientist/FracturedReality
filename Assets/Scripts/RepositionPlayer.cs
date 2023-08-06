using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepositionPlayer : MonoBehaviour
{
    private float m_movingDistance = 0.2f;

    [SerializeField] private Transform m_VRPlayerRig;

    private Vector3 m_startPosCameraRig;


    private void Start()
    {
        //m_target = FindObjectOfType<SpaceshipController>().transform;
        m_startPosCameraRig = m_VRPlayerRig.localPosition;
    }

    public void MovePlayerUp()
    {
        m_VRPlayerRig.Translate(Vector3.up * m_movingDistance);
        //isChanging = true;
        Time.timeScale = 1f;
        StartCoroutine(UpdateUI());

    }
    public void MovePlayerDown()
    {
        m_VRPlayerRig.Translate(Vector3.down * m_movingDistance);
        Time.timeScale = 1f;
        StartCoroutine(UpdateUI());

    }
    public void MovePlayerLeft()
    {
        m_VRPlayerRig.Translate(Vector3.left * m_movingDistance);
        Time.timeScale = 1f;
        StartCoroutine(UpdateUI());
    }
    public void MovePlayerRight()
    {
        m_VRPlayerRig.Translate(Vector3.right * m_movingDistance);
        Time.timeScale = 1f;
        StartCoroutine(UpdateUI());
    }

    public void MovePlayerForward()
    {
        m_VRPlayerRig.Translate(Vector3.forward * m_movingDistance);
        Time.timeScale = 1f;
        StartCoroutine(UpdateUI());
    }

    public void MovePlayerBackward()
    {
        m_VRPlayerRig.Translate(Vector3.back * m_movingDistance);
        Time.timeScale = 1f;
        StartCoroutine(UpdateUI());
    }

    public IEnumerator UpdateUI()
    {
        yield return new WaitForSeconds(0.01f);
        Time.timeScale = 0;
    }

    public void RotateRight()
    {
        m_VRPlayerRig.Rotate(Vector3.up);

        Time.timeScale = 1f;

        StartCoroutine(UpdateUI());
    }

    public void RotateLeft()
    {
        m_VRPlayerRig.Rotate(Vector3.down);

        Time.timeScale = 1f;

        StartCoroutine(UpdateUI());
    }


    public void ResetCameraPosition()
    {
        m_VRPlayerRig.localPosition = m_startPosCameraRig;
        m_VRPlayerRig.localRotation = Quaternion.identity;
        Time.timeScale = 1f;

        StartCoroutine(UpdateUI());
    }
/*
    [SerializeField] private Transform PauseMenu;
    [SerializeField] private Transform Camera;

    //currently not used
    public void SetUIInPlayerView()
    {
        PauseMenu.position = Camera.position;

        PauseMenu.rotation = Camera.rotation;

        PauseMenu.Translate(Vector3.forward * 2);

        

        Time.timeScale = 1f;

        StartCoroutine(UpdateUI());
    }

    [SerializeField] private Transform m_head;
    [SerializeField] private Transform m_origin;
    [SerializeField] private Transform m_target;




    //currently not used
    public void Recenter()
    {
        Vector3 offset = m_head.position - m_origin.position;
        offset.y = 0;
        m_origin.position = m_target.position - offset;

        Vector3 targetForward = m_target.forward;
        targetForward.y = 0;
        Vector3 cameraForward = m_head.forward;
        cameraForward.y = 0;

        float angleZ = Vector3.SignedAngle(cameraForward, targetForward, Vector3.up);

        m_origin.RotateAround(m_head.position, Vector3.up, angleZ);


        float angleY = Vector3.SignedAngle(m_head.right, m_target.right, Vector3.forward);

        m_origin.RotateAround(m_head.position, Vector3.forward, angleY);

        Time.timeScale = 1f;
        StartCoroutine(UpdateUI());
    }

*/

}
