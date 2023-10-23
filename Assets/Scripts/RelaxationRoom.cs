using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelaxationRoom : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> m_bossFightObjects;

    [SerializeField]
    private List<GameObject> m_relaxationRoomObjects;

    [SerializeField]
    private DissolveSphere m_relaxSphere;

    [SerializeField]
    private AudioClip m_relaxMusic;

    private MusicFader m_musicFader;

    private GameObject playerObject;
    private SpaceshipController spaceshipController;
    private Rigidbody playerRB;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        m_musicFader = FindAnyObjectByType<MusicFader>();
        spaceshipController = playerObject.GetComponent<SpaceshipController>();
        playerRB = playerObject.GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        //Temp: Switch between Relax-Room and Boss-Fight
        if(Input.GetKeyDown(KeyCode.K))
        {
          StartCoroutine(  ActivateBossFightRoom());
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
           StartCoroutine( ActivateRelaxRoom());
        }

    }

    public IEnumerator ActivateRelaxRoom()
    {
        StartCoroutine ( m_musicFader.PlayMusicCoroutine(m_relaxMusic, true, true));

        spaceshipController.enabled = false;

        playerRB.velocity = Vector3.zero;

        m_relaxSphere.transform.position = playerObject.transform.position;

        m_relaxSphere.gameObject.SetActive(true);

        yield return StartCoroutine(m_relaxSphere.ShowSphere());

        foreach (GameObject obj in m_bossFightObjects)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in m_relaxationRoomObjects)
        {
            obj.SetActive(true);
        }

        
    }

    public IEnumerator ActivateBossFightRoom()
    {
        spaceshipController.enabled = true;

        foreach (GameObject obj in m_bossFightObjects)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in m_relaxationRoomObjects)
        {
            obj.SetActive(false);
        }

        yield return StartCoroutine(m_relaxSphere.HideSphere());

        m_relaxSphere.gameObject.SetActive(false);
    }
}
