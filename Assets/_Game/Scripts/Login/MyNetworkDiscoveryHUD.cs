﻿using Mirror;
using Mirror.Discovery;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[AddComponentMenu("Network/NetworkDiscoveryHUD")]
[HelpURL("https://mirror-networking.com/docs/Components/NetworkDiscovery.html")]
[RequireComponent(typeof(NetworkDiscovery))]
public class MyNetworkDiscoveryHUD : MonoBehaviour
{
    readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
    Vector2 scrollViewPos = Vector2.zero;

    public NetworkDiscovery networkDiscovery;

    private void Awake()
    {
        SceneManager.LoadScene("Gameplay", LoadSceneMode.Additive);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (networkDiscovery == null)
        {
            networkDiscovery = GetComponent<NetworkDiscovery>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
            UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
        }
    }
#endif

    void OnGUI()
    {
        if (NetworkManager.singleton == null)
            return;

        if (NetworkServer.active || NetworkClient.active)
            return;

        if (!NetworkClient.isConnected && !NetworkServer.active && !NetworkClient.active)
            DrawGUI();
    }

    void DrawGUI()
    {
        GUILayout.BeginHorizontal();

        var options = new GUILayoutOption[] { GUILayout.Height(80), GUILayout.Width(120) };

        if (GUILayout.Button("Find Servers", options))
        {
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
        }

        // LAN Host
        if (GUILayout.Button("Start Host", options))
        {
            discoveredServers.Clear();
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
            
        }

        // Dedicated server
        if (GUILayout.Button("Start Server", options))
        {
            discoveredServers.Clear();
            NetworkManager.singleton.StartServer();

            networkDiscovery.AdvertiseServer();
        }

        GUILayout.EndHorizontal();

        // show list of found server

        GUILayout.Label($"Discovered Servers [{discoveredServers.Count}]:");

        // servers
        scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);

        foreach (ServerResponse info in discoveredServers.Values)
        {
            if (GUILayout.Button(info.EndPoint.Address.ToString(), options))
            {
                Connect(info);
                
            }
        }

        GUILayout.EndScrollView();
    }

    void Connect(ServerResponse info)
    {
        NetworkManager.singleton.StartClient(info.uri);
    }

    public void OnDiscoveredServer(ServerResponse info)
    {
        // Note that you can check the versioning to decide if you can connect to the server or not using this method
        discoveredServers[info.serverId] = info;
    }
}
