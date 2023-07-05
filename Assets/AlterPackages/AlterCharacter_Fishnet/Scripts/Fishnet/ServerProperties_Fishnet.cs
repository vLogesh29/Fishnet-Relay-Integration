using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerProperties_Fishnet : NetworkBehaviour
{
    public static ServerProperties_Fishnet Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    // Define custom room properties
    [field: SyncVar(OnChange = nameof(OnRoomNameChanged))] 
    public string RoomName { get; [Server] private set; } = "ServerSingleton";
    
    [field: SyncVar(OnChange = nameof(OnRoomIdChanged))] 
    public int RoomId { get; [Server] private set; } = 0;

    [SyncObject] private readonly SyncDictionary<string, object> serverProperties = new();
    public IReadOnlyDictionary<string, object> ServerProperties => serverProperties;

    private void OnRoomNameChanged(string oldRoomName, string newRoomName, bool asServer)
    {
        Debug.Log($">>> server-property(asServer={asServer}): RoomName changed from {oldRoomName} to {newRoomName}.");
    }
    private void OnRoomIdChanged(int oldRoomId, int newRoomId, bool asServer)
    {
        Debug.Log($">>> server-property(asServer={asServer}): RoomID changed from {oldRoomId} to {newRoomId}.");
    }

    [Server]
    private void ModifyServerProperties(SyncDictionaryOperation op, string key, object value = null)
    {
        if (op == SyncDictionaryOperation.Add || op == SyncDictionaryOperation.Set)
        {
            if (serverProperties.ContainsKey(key))
                serverProperties[key] = value;
            else
                serverProperties.Add(key, value);
            return;
        }

        switch (op)
        {
            //Removes key.
            case SyncDictionaryOperation.Remove:
                serverProperties.Remove(key);
                break;

            //Clears the dictionary.
            case SyncDictionaryOperation.Clear:
                serverProperties.Clear();
                break;

            //Like SyncList, indicates all operations are complete.
            case SyncDictionaryOperation.Complete:
                break;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log(gameObject.name + ">>> Try Set ServerProperties: OnStartServer"); 
        Debug.Log($">>> Server-Side Properties: \nisSpawned:{IsSpawned}, " +
            //$"\nisOwner:{IsOwner}, " + // usage of isOwner not allowed in OnStartServer
            $"\nisServer/Only:{IsServer}/{IsServerOnly}, " +
            $"\nisClient/Only:{IsClient}/{IsClientOnly}, " +
            $"\nisHost:{IsHost};");

        SetRoomPropertiesServerInitialize("Test-Server", -9999); // server without client, cant call serverRPC
    }
    /*
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log(gameObject.name + ">>> Try set ServerProperties: OnStartClient");
        Debug.Log($">>> Client-Side Properties: \nisSpawned:{IsSpawned}, " +
            $"\nisOwner:{IsOwner}, " +
            $"\nisServer/Only:{IsServer}/{IsServerOnly}, " +
            $"\nisClient/Only:{IsClient}/{IsClientOnly}, " +
            $"\nisHost:{IsHost};");

        //SetRoomPropertiesServerRPC(Owner,"Test-Client", Owner.ClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRoomPropertiesServerRPC(NetworkConnection conn, string name, int id)
    {
        Debug.Log(">>> RoomPropertiesSet ServerRPC called by client-ID:" + conn.ClientId);

        // Set the custom room properties
        roomName = name;
        roomId = id;

        // Notify all clients about the updated room properties
        RpcUpdateRoomProperties(roomName, roomId);
    }
    */

    [Server]
    public void SetRoomPropertiesServerInitialize(string name, int id)
    {
        Debug.Log(">>> RoomPropertiesSet Initialize by Server called...");

        // Set the custom room properties
        RoomName = name;
        RoomId = id;

        // Notify all clients about the updated room properties
        RpcUpdateRoomProperties(RoomName, RoomId);
    }

    // to update room properties on late-join clients
    [ObserversRpc(BufferLast = true)]
    public void RpcUpdateRoomProperties(string name, int id)
    {
        Debug.Log(">>> RoomPropertiesCallback ObserverRPC called by server all clients");
        // Update the room properties on all clients

        // client try to set [Sever] attributed modifier, should throw warning. Only host can modify this as it will be server as well
        RoomName = "Host-"+name;
        RoomId = 1111+id;
        // instead of using sync var, server only initialized once, thus using bufferLast, it allow late joining clients also to fetch
        // the last rpc call made by server while initialization.
    }

    private void MatchMake()
    {
        ModifyServerProperties(SyncDictionaryOperation.Add, "World", SceneController_Fishnet.Instance.GetWorldName());
        RoomName = Guid.NewGuid().ToString();
        //RoomOptions ro = GetRoomOptions();

        //PhotonNetwork.JoinRandomOrCreateRoom(expectedCustomRoomProperties, 0, MatchmakingMode.RandomMatching, TypedLobby.Default, null, roomName, ro);
    }
}
