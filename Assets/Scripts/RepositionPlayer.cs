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




}
