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

    private FadeBlackScreen blackScreen;

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
        WriteTime.SetCurrentEvent("Relax Room");

        blackScreen ??= FindAnyObjectByType<FadeBlackScreen>();
        StartCoroutine ( m_musicFader.PlayMusicCoroutine(m_relaxMusic, true, true));

        spaceshipController.enabled = false;
        playerController.enabled = false;

        Material material = GameObject.FindWithTag("RenderPlane").GetComponent<MeshRenderer>().material;
        material.SetFloat("_AO", 4.5f);
        material.SetFloat("_IAmbient", 0.5f);
        material.SetFloat("_LightExposure", 2.5f);
        material.SetFloat("_LightIntensity", 6f);
        material.SetFloat("_SaturationGamma", 1f);
        material.SetInteger("_LightMode", 1);

        playerRB.velocity = Vector3.zero;

        transform.position = playerObject.transform.position;
        transform.rotation = playerObject.transform.rotation;

        m_relaxBox.gameObject.SetActive(true);

        yield return StartCoroutine(m_relaxBox.ShowObject());

        yield return StartCoroutine( blackScreen.FadeAlpha(1, 1));

        foreach (GameObject obj in m_bossFightObjects)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in m_relaxationRoomObjects)
        {
            obj.SetActive(true);
        }
        

        playerObject.transform.position = Vector3.zero;
        playerObject.transform.rotation = Quaternion.identity;

        yield return StartCoroutine(blackScreen.FadeAlpha(0, 1));

    }

    public IEnumerator ActivateBossFightRoom()
    {
        yield return StartCoroutine(blackScreen.FadeAlpha(1, 1));

        WriteTime.SetCurrentEvent("Leave Relax Room");

        // Set to start position
        playerObject.transform.position = new Vector3(140f, 20f, -120f);
        playerRB.velocity = Vector3.zero;
        // Face boss
        playerObject.transform.rotation = Quaternion.LookRotation(playerController.m_BossObject.transform.position - playerObject.transform.position, playerObject.transform.up);
        
        Material material = GameObject.FindWithTag("RenderPlane").GetComponent<MeshRenderer>().material;
        material.SetFloat("_AO", 0.8f);
        material.SetFloat("_IAmbient", 0.7f);
        material.SetFloat("_LightExposure", 5.7f);
        material.SetFloat("_LightIntensity", 6f);
        material.SetFloat("_SaturationGamma", 2f);
        material.SetInteger("_LightMode", 0);

        foreach (GameObject obj in m_relaxationRoomObjects)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in m_bossFightObjects)
        {
            obj.SetActive(true);
        }
        yield return StartCoroutine(blackScreen.FadeAlpha(0, 1));
        
        yield return StartCoroutine(m_relaxBox.HideObject());
        m_relaxBox.gameObject.SetActive(false);
        
        spaceshipController.enabled = true;
        playerController.enabled = true;
    }
}
