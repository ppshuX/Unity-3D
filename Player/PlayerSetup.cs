using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable;

    private Camera sceneCamera;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsLocalPlayer)
        {
            DisableComponents();
        }
        else
        {
            sceneCamera = Camera.main;
            if (sceneCamera != null)
            {
                sceneCamera.gameObject.SetActive(false);
            }
        }

        SetPlayerName();
    }

    private void SetPlayerName()
    {
        transform.name = "Player " + GetComponent<NetworkObject>().NetworkObjectId;
    }

    private void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    // Update is called once per frame
    private void OnDisable()
    {
        if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
    }
}