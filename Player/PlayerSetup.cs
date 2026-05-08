using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable;

    [Tooltip("进游戏后要关掉的场景/菜单相机。勿依赖 Camera.main：FPS 标成 MainCamera 时 Main 已是子相机，会被误关。")]
    [SerializeField]
    private Camera menuWorldCamera;

    private Camera sceneCamera;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsLocalPlayer)
        {
            SetLayerMaskForAllChildren(transform, LayerMask.NameToLayer("Remote Player"));
            DisableComponents();
        }
        else
        {
            PlayerUI.Singleton.setPlayer(GetComponent<Player>());
            SetLayerMaskForAllChildren(transform, LayerMask.NameToLayer("Player"));
            if (menuWorldCamera != null)
            {
                sceneCamera = menuWorldCamera;
            }
            else
            {
                GameObject sceneCamGo = GameObject.Find("SceneCamera");
                sceneCamera = sceneCamGo != null ? sceneCamGo.GetComponent<Camera>() : null;
            }

            if (sceneCamera != null)
            {
                sceneCamera.gameObject.SetActive(false);
            }
        }

        string name = "Player " + GetComponent<NetworkObject>().NetworkObjectId.ToString();
        Player player = GetComponent<Player>();
        player.Setup();

        GameManager.Singleton.RegisterPlayer(name, player);
    }

    private void SetLayerMaskForAllChildren(Transform transform, LayerMask layerMask)
    {
        transform.gameObject.layer = layerMask;
        for (int i = 0; i < transform.childCount; i ++ )
        {
            SetLayerMaskForAllChildren(transform.GetChild(i), layerMask);
        }
    }

    private void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }

        GameManager.Singleton.UnRegisterPlayer(transform.name);
    }
}
