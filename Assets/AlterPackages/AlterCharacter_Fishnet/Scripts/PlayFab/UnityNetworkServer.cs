﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Connection;
using FishNet.Managing;
using FishNet;
using FishNet.Broadcast;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace PlayFab.Networking
{
    public class UnityNetworkServer : MonoBehaviour
    {
        public static UnityNetworkServer Instance { get; private set; }

        public PlayerEvent OnPlayerAdded = new PlayerEvent();
        public PlayerEvent OnPlayerRemoved = new PlayerEvent();

        public int MaxConnections = 100;
        public int Port = 7777; // overwritten by the code in AgentListener.cs

        public List<UnityNetworkConnection> Connections
        {
            get { return _connections; }
            private set { _connections = value; }
        }
        private List<UnityNetworkConnection> _connections = new List<UnityNetworkConnection>();

        public class PlayerEvent : UnityEvent<string> { }

        private NetworkManager m_networkManager;
        private NetworkManager NetworkManager
        {
            get
            {
                if (m_networkManager == null)
                    m_networkManager = InstanceFinder.NetworkManager;
                return m_networkManager;
            }
        }

        // Use this for initialization
        public void Awake()
        {
            Instance = this;

            NetworkManager.ServerManager.RegisterBroadcast<ReceiveAuthenticateMessage>(OnReceiveAuthenticate);
            NetworkManager.ServerManager.OnRemoteConnectionState += NetworkServer_OnRemoteConnectionStateChanged;
        }

        private void NetworkServer_OnRemoteConnectionStateChanged(NetworkConnection conn, FishNet.Transporting.RemoteConnectionStateArgs stateArgs)
        {
            if (stateArgs.ConnectionState == FishNet.Transporting.RemoteConnectionState.Started)
                OnServerConnect(conn);
            else if (stateArgs.ConnectionState == FishNet.Transporting.RemoteConnectionState.Stopped)
                OnServerDisconnect(conn);
        }

        public void StartListen()
        {
            //this.GetComponent<TelepathyTransport>().port = (ushort)Port;
            //NetworkServer.Listen(MaxConnections);

            //NetworkManager.TransportManager.Transport.SetPort((ushort)Port);
            NetworkManager.TransportManager.Transport.SetMaximumClients(MaxConnections);
            NetworkManager.ServerManager.StartConnection((ushort)Port);
        }

        public void OnApplicationQuit()
        {
            NetworkManager.ServerManager.StopConnection(false); // sendDisconnectMessage : True to send a disconnect message to all clients first.
        }

        private void OnReceiveAuthenticate(NetworkConnection nconn, ReceiveAuthenticateMessage message)
        {
            var conn = _connections.Find(c => c.ConnectionAddress == nconn.GetAddress());
            if (conn != null)
            {
                conn.PlayFabId = message.PlayFabId;
                conn.IsAuthenticated = true;
                OnPlayerAdded.Invoke(message.PlayFabId);
            }
        }

        public void OnServerConnect(NetworkConnection conn)
        {
            //base.OnServerConnect(conn);

            Debug.LogWarning("Client Connected");
            var uconn = _connections.Find(c => c.ConnectionAddress == conn.GetAddress());
            if (uconn == null)
            {
                _connections.Add(new UnityNetworkConnection()
                {
                    Connection = conn,
                    ConnectionAddress = conn.GetAddress(),
                    LobbyId = PlayFabMultiplayerAgentAPI.SessionConfig.SessionId
                });
            }
        }

        // dont know how to trigger this callback in fishnet, only OnStartServer or OnStopServer is possible to implement
        public void OnServerError(NetworkConnection conn, Exception ex)
        {
            //base.OnServerError(conn, ex);

            Debug.Log(string.Format("Unity Network Connection Status: exception - {0}", ex.Message));
        }

        public void OnServerDisconnect(NetworkConnection conn)
        {
            //base.OnServerDisconnect(conn);

            var uconn = _connections.Find(c => c.ConnectionAddress == conn.GetAddress());
            if (uconn != null)
            {
                if (!string.IsNullOrEmpty(uconn.PlayFabId))
                {
                    OnPlayerRemoved.Invoke(uconn.PlayFabId);
                }
                _connections.Remove(uconn);
            }
        }
    }

    [Serializable]
    public class UnityNetworkConnection
    {
        public bool IsAuthenticated;
        public string PlayFabId;
        public string LobbyId;
        public string ConnectionAddress;
        public NetworkConnection Connection;
    }

    public class CustomGameServerMessageTypes
    {
        public const short ReceiveAuthenticate = 900;
        public const short ShutdownMessage = 901;
        public const short MaintenanceMessage = 902;
    }

    public struct ReceiveAuthenticateMessage : IBroadcast
    {
        public string PlayFabId;
    }

    public struct ShutdownMessage : IBroadcast
    { }

    [Serializable]
    public struct MaintenanceMessage : IBroadcast
    {
        public DateTime ScheduledMaintenanceUTC;
    }
}
