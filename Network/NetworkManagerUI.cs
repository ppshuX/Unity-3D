using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField]
    private Button hostBtn;
    [SerializeField]
    private Button serverBtn;
    [SerializeField]
    private Button clientBtn;

    [SerializeField]
    private Button room1;
    [SerializeField]
    private Button room2;

    // Start is called before the first frame update
    void Start()
    {
        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i ++ )
        {
            if (args[i] == "-port" && i + 1 < args.Length)
            {
                int port = int.Parse(args[i + 1]);
                var transport = GetComponent<UNetTransport>();
                if (transport != null)
                {
                    transport.ConnectPort = transport.ServerListenPort = port;
                }
            }
        }

        for (int i = 0; i < args.Length; i ++ )
        {
            if (args[i] == "-lauch-as-server")
            {
                if (NetworkManager.Singleton == null)
                {
                    Debug.LogError("NetworkManager.Singleton 为 null，无法以专服启动。");
                }
                else
                {
                    NetworkManager.Singleton.StartServer();
                }

                DestroyAllButtons();
            }
        }

        // 必须先注册 Host/Server/Client：若 room1/room2 在打包场景里未赋值，
        // 原课件顺序会在 room1.onClick 处抛错，导致这三个按钮从未绑定。
        if (hostBtn != null)
        {
            hostBtn.onClick.AddListener(() =>
            {
                if (!EnsureNetworkManager()) return;
                NetworkManager.Singleton.StartHost();
                DestroyAllButtons();
            });
        }

        if (serverBtn != null)
        {
            serverBtn.onClick.AddListener(() =>
            {
                if (!EnsureNetworkManager()) return;
                NetworkManager.Singleton.StartServer();
                DestroyAllButtons();
            });
        }

        if (clientBtn != null)
        {
            clientBtn.onClick.AddListener(() =>
            {
                if (!EnsureNetworkManager()) return;
                NetworkManager.Singleton.StartClient();
                DestroyAllButtons();
            });
        }

        if (room1 != null)
        {
            room1.onClick.AddListener(() =>
            {
                var transport = GetComponent<UNetTransport>();
                if (transport != null)
                {
                    transport.ConnectPort = transport.ServerListenPort = 7777;
                }

                if (!EnsureNetworkManager()) return;
                NetworkManager.Singleton.StartClient();
                DestroyAllButtons();
            });
        }

        if (room2 != null)
        {
            room2.onClick.AddListener(() =>
            {
                var transport = GetComponent<UNetTransport>();
                if (transport != null)
                {
                    transport.ConnectPort = transport.ServerListenPort = 7778;
                }

                if (!EnsureNetworkManager()) return;
                NetworkManager.Singleton.StartClient();
                DestroyAllButtons();
            });
        }
    }

    private static bool EnsureNetworkManager()
    {
        if (NetworkManager.Singleton != null) return true;

        Debug.LogError("NetworkManager.Singleton 为 null：请确认场景中有启用的 NetworkManager。");
        return false;
    }

    private void DestroyAllButtons()
    {
        if (hostBtn != null) Destroy(hostBtn.gameObject);
        if (serverBtn != null) Destroy(serverBtn.gameObject);
        if (clientBtn != null) Destroy(clientBtn.gameObject);
        if (room1 != null) Destroy(room1.gameObject);
        if (room2 != null) Destroy(room2.gameObject);
    }
}
