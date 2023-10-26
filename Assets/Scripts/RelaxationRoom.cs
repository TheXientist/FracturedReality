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
    private DissolveObject m_relaxBox;

    [SerializeField]
    private AudioClip m_relaxMusic;

    private MusicFader m_musicFader;

    private GameObject playerObject;
    private SpaceshipController spaceshipController;
    private Player playerController;
    private Rigidbody playerRB;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        m_musicFader = FindAnyObjectByType<MusicFader>();
        spaceshipController = playerObject.GetComponent<SpaceshipController>();
        playerController = playerObject.GetComponent<Player>();
        playerRB = playerObject.GetComponent<Rigidbody>();

        for (int i = 0; i < playerObject.transform.childCount - 1; i++)
        {
            GameObject temp = playerObject.transform.GetChild(i).gameObject;
            if (temp.activeSelf && !temp.name.Equals("Main Camera"))
            {
                m_bossFightObjects.Add(temp);
            }
        }

    }

    public IEnumerator ActivateRelaxRoom()
    {
        StartCoroutine ( m_musicFader.PlayMusicCoroutine(m_relaxMusic, true, true));

        spaceshipController.enabled = false;
        playerController.enabled = false;

        playerRB.velocity = Vector3.zero;

        transform.position = playerObject.transform.position;
        transform.rotation = playerObject.transform.rotation;

        m_relaxBox.gameObject.SetActive(true);

        yield return StartCoroutine(m_relaxBox.ShowObject());

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
        playerController.enabled = true;

        foreach (GameObject obj in m_relaxationRoomObjects)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in m_bossFightObjects)
        {
            obj.SetActive(true);
        }
        
        yield return StartCoroutine(m_relaxBox.HideObject());
        m_relaxBox.gameObject.SetActive(false);
    }
}
