using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    /// <summary>Lesson 9.2 房间列表 HTTP API（你可改域名/端口）。</summary>
    private const string ApiBase = "http://49.232.65.186:8000";

    [SerializeField]
    private Button refreshButton;
    [SerializeField]
    private Button buildButton;

    [SerializeField]
    private Canvas menuUI;
    [SerializeField]
    private GameObject roomButtonPrefab;

    private readonly List<Button> rooms = new List<Button>();

    private int buildRoomPort = -1;

    private void Start()
    {
        if (!ApplyCommandLineConfig())
            return;

        InitButtons();
        RefreshRoomList();
    }

    private void OnApplicationQuit()
    {
        if (buildRoomPort != -1)
        {
            RemoveRoom();
        }
    }

    /// <summary>返回 false 时表示已作为专服启动，不再初始化菜单。</summary>
    private bool ApplyCommandLineConfig()
    {
        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
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

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-lauch-as-server")
            {
                if (NetworkManager.Singleton != null)
                    NetworkManager.Singleton.StartServer();
                else
                    Debug.LogError("NetworkManager.Singleton 为 null，无法以专服启动。");

                DestroyAllButtons();
                return false;
            }
        }

        return true;
    }

    private void InitButtons()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshRoomList);
        }

        if (buildButton != null)
        {
            buildButton.onClick.AddListener(BuildRoom);
        }
    }

    private void RefreshRoomList()
    {
        StartCoroutine(RefreshRoomListRequest(ApiBase + "/fps/get_room_list/"));
    }

    private IEnumerator RefreshRoomListRequest(string uri)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(uri))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError)
                yield break;

            var resp = JsonUtility.FromJson<GetRoomListResponse>(uwr.downloadHandler.text);
            if (resp == null || resp.rooms == null)
                yield break;

            foreach (var room in rooms)
            {
                if (room != null)
                {
                    room.onClick.RemoveAllListeners();
                    Destroy(room.gameObject);
                }
            }

            rooms.Clear();

            int k = 0;
            foreach (var room in resp.rooms)
            {
                if (roomButtonPrefab == null || menuUI == null)
                    break;

                int roomPort = room.port;
                string roomName = room.name;

                GameObject buttonObj = Instantiate(roomButtonPrefab, menuUI.transform);
                buttonObj.transform.localPosition = new Vector3(-21, 92 - k * 60, 0);
                Button button = buttonObj.GetComponent<Button>();
                var label = button != null ? button.GetComponentInChildren<TextMeshProUGUI>() : null;
                if (label != null)
                    label.text = roomName;

                if (button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        var transport = GetComponent<UNetTransport>();
                        if (transport != null)
                        {
                            transport.ConnectPort = transport.ServerListenPort = roomPort;
                        }

                        if (NetworkManager.Singleton == null)
                        {
                            Debug.LogError("NetworkManager.Singleton 为 null。");
                            return;
                        }

                        NetworkManager.Singleton.StartClient();
                        DestroyAllButtons();
                    });
                    rooms.Add(button);
                }

                k++;
            }
        }
    }

    private void BuildRoom()
    {
        StartCoroutine(BuildRoomRequest(ApiBase + "/fps/build_room/"));
    }

    private IEnumerator BuildRoomRequest(string uri)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(uri))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError)
                yield break;

            var resp = JsonUtility.FromJson<BuildRoomResponse>(uwr.downloadHandler.text);
            if (resp == null || resp.error_message != "success")
                yield break;

            var transport = GetComponent<UNetTransport>();
            if (transport != null)
            {
                transport.ConnectPort = transport.ServerListenPort = resp.port;
            }

            buildRoomPort = resp.port;

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager.Singleton 为 null。");
                yield break;
            }

            NetworkManager.Singleton.StartClient();
            DestroyAllButtons();
        }
    }

    private void RemoveRoom()
    {
        StartCoroutine(RemoveRoomRequest(ApiBase + "/fps/remove_room/?port=" + buildRoomPort));
    }

    private IEnumerator RemoveRoomRequest(string uri)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(uri))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError)
                yield break;

            var resp = JsonUtility.FromJson<RemoveRoomResponse>(uwr.downloadHandler.text);
            if (resp != null && resp.error_message == "success")
            {
            }
        }
    }

    private void DestroyAllButtons()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            Destroy(refreshButton.gameObject);
        }

        if (buildButton != null)
        {
            buildButton.onClick.RemoveAllListeners();
            Destroy(buildButton.gameObject);
        }

        foreach (var room in rooms)
        {
            if (room != null)
            {
                room.onClick.RemoveAllListeners();
                Destroy(room.gameObject);
            }
        }

        rooms.Clear();
    }
}
