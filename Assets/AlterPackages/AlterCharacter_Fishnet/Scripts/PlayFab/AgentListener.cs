using System.Collections;
using UnityEngine;
using PlayFab;
using System;
using PlayFab.Networking;
using System.Collections.Generic;
using PlayFab.MultiplayerAgent.Model;
using System.Linq;
using System.IO;

public class AgentListener : MonoBehaviour 
{
    private List<ConnectedPlayer> _connectedPlayers;
    public bool Debugging = true;
    public bool shouldRetryConnection = true;
    public float retryConnectionTimer = 2f;
    [SerializeField] private string GsdkConfigFileName = null;

    // Use this for initialization
    void Start () {
        _connectedPlayers = new List<ConnectedPlayer>();
        Debug.Log("Given Config Path: " + Path.Combine(Application.dataPath, GsdkConfigFileName));
        PlayFabMultiplayerAgentAPI.Start(Path.Combine(Application.dataPath, GsdkConfigFileName));
        PlayFabMultiplayerAgentAPI.IsDebugging = Debugging;
        PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
        PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
        PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
        PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;

        UnityNetworkServer.Instance.OnPlayerAdded.AddListener(OnPlayerAdded);
        UnityNetworkServer.Instance.OnPlayerRemoved.AddListener(OnPlayerRemoved);

        SetupGSDK();
        
        StartCoroutine(ReadyForPlayers());
    }
    private void SetupGSDK()
    {
        // get the port that the server will listen to
        // We *have to* do it on process mode, since there might be more than one game server instances on the same VM and we want to avoid port collision
        // On container mode, we can omit the below code and set the port directly, since each game server instance will run on its own network namespace. However, below code will work as well
        // we have to do that on process
        var connInfo = PlayFabMultiplayerAgentAPI.GetGameServerConnectionInfo();
        if (connInfo == null)
        {
            Debug.LogError("Connection Failed as ConnectionInfo is not found!");
            if (shouldRetryConnection)
            {
                Debug.Log("---------------" + "< Retrying Connection in " + retryConnectionTimer + "s >" + "---------------");
                Invoke(nameof(SetupGSDK), retryConnectionTimer); // retry connection every few secs
            }
            return;
        }
        else
        {
            Debug.Log("PlayFabMultiplayerAgentAPI connection successful and received connectionInfo as: " + connInfo);
        }
        // make sure the ListeningPortKey is the same as the one configured in your Build settings (either on LocalMultiplayerAgent or on MPS)
        const string ListeningPortKey = "gameport";
        var portInfo = connInfo.GamePortsConfiguration.Where(x => x.Name == ListeningPortKey);
        if (portInfo.Count() > 0)
        {
            Debug.Log(string.Format("port with name {0} was found in GSDK Config Settings.", ListeningPortKey));
            UnityNetworkServer.Instance.NetworkHUD.Port = portInfo.Single().ServerListeningPort;
        }
        else
        {
            string msg = string.Format("Cannot find port with name {0} in GSDK Config Settings. If you are running locally, make sure the LocalMultiplayerAgent is running and that the MultiplayerSettings.json file includes correct name as a GamePort Name. If you are running this sample in the cloud, make sure you have assigned the correct name to the port", ListeningPortKey);
            Debug.LogError(msg);
            throw new Exception(msg);
        }
    }

    private void OnEnable() 
        // here i have put in a gameobject having networkobject comp,
        // it disables and enables whenever not connected to server, easier to restart kindoff
    {
        if (shouldRetryConnection)
        {
            Debug.Log("Retrying Connection Agent Listener!");
            Start();
        }
    }

    IEnumerator ReadyForPlayers()
    {
        yield return new WaitForSeconds(.5f);
        PlayFabMultiplayerAgentAPI.ReadyForPlayers();
    }
    
    private void OnServerActive()
    {
        Debug.Log("Server Started From Agent Activation");
        UnityNetworkServer.Instance.StartListen(UnityNetworkServer.Type.Server);
        if(PlayFabMultiplayerAgentAPI.IsDebugging)
        {
            foreach (KeyValuePair<string,string> kvp in PlayFabMultiplayerAgentAPI.GetConfigSettings())
            {
                Debug.Log(kvp.Key + ": " + kvp.Value);
            }
        }

        // if using PlayFab matchmaking, you can get the matchmaking queue name with
        // string queueName = PlayFabMultiplayerAgentAPI.GetConfigSettings()["PF_MATCH_QUEUE_NAME"];
    }

    private void OnPlayerRemoved(string playfabId)
    {
        ConnectedPlayer player = _connectedPlayers.Find(x => x.PlayerId.Equals(playfabId, StringComparison.OrdinalIgnoreCase));
        _connectedPlayers.Remove(player);
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
    }

    private void OnPlayerAdded(string playfabId)
    {
        _connectedPlayers.Add(new ConnectedPlayer(playfabId));
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
    }

    private void OnAgentError(string error)
    {
        Debug.Log(error);
    }

    private void OnShutdown()
    {
        Debug.Log("Server is shutting down");
        foreach(var conn in UnityNetworkServer.Instance.Connections)
        {
            conn.Connection.Broadcast<ShutdownMessage>(new ShutdownMessage());
        }
        StartCoroutine(Shutdown());
    }

    IEnumerator Shutdown()
    {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }

    private void OnMaintenance(DateTime? NextScheduledMaintenanceUtc)
    {
        Debug.LogFormat("Maintenance scheduled for: {0}", NextScheduledMaintenanceUtc.Value.ToLongDateString());
        foreach (var conn in UnityNetworkServer.Instance.Connections)
        {
            conn.Connection.Broadcast<MaintenanceMessage>(new MaintenanceMessage() {
                ScheduledMaintenanceUTC = (DateTime)NextScheduledMaintenanceUtc
            });
        }
    }
}
