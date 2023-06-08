using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProperties_Fishnet : NetworkBehaviour
{
    // note sync vars are change-able from client side, but i wont update on any other clients as well as server;
    // even changing in inspector wont affect, as i think in fishnet, the '=' assignment operater of "SyncVar" - attributed
    // is been overriden to make a special Late-Observers-Rpc from server end only (as syncing is done after custom RPC calls only) call
    // based on TimeManager's Tick to sync on the readOnly-attributed observers. 
    // thus for, we have ensure that sync var is changed only from server, can done by doing any assignment related task to a syncVar
    // under a [Server] attributed function, to avoid improper syncing, and without Logging.Off, we could also see a warning when a client tries to
    // call a [Server] attributed fn. Maybe in later version, updating syncVars from client should'nt allowed (editor scripting / debug exception/warning)
    /*
     //A typical server-side SyncVar.
        [SyncVar]
        public string Name;

        //A client-side SyncVar.
        [field: SyncVar]
        public string Name { get; [ServerRpc] set; }
     */
    
    // client sync-vars (default: only Owners have authority to change it)
    [field: SyncVar] public string CharacterID { get; [ServerRpc] private set; } = "Player####";
    [field: SyncVar] public string CharacterURL { get; [ServerRpc] private set; } = "alterAPI.com/Player####";

    [field: SyncVar(OnChange = nameof(OnUserIdChanged))] 
    public int UserID { get; [ServerRpc] private set; } = -1;

    // can't do [attributed] auto-property {set} for Collections, rather should implemented as
    // a seperate ServerRPC fn call in-order to make it Client-Sync-Vars
    [SyncObject] private readonly SyncDictionary<string, object> playerProperties = new();
    public IReadOnlyDictionary<string, object> PlayerProperties => playerProperties;

    private void Awake()
    {
        playerProperties.OnChange += PlayerProperties_OnChange;
    }

    private void PlayerProperties_OnChange(SyncDictionaryOperation op, string key, object value, bool asServer)
    {
        switch (op)
        {
            //Adds key with value.
            case SyncDictionaryOperation.Add:
                Debug.Log($"[asServer:{asServer}] >>> PlayerProperties added <key,value> pair: [{key}]={value}");
                break;
            //Removes key.
            case SyncDictionaryOperation.Remove:
                Debug.Log($"[asServer:{asServer}] >>> PlayerProperties removed <key,value> pair: [{key}]={value}");
                break;
            //Sets key to a new value.
            case SyncDictionaryOperation.Set:
                Debug.Log($"[asServer:{asServer}] >>> PlayerProperties set <key,value> pair: [{key}]={value}");
                break;
            //Clears the dictionary.
            case SyncDictionaryOperation.Clear:
                Debug.Log($"[asServer:{asServer}] >>> PlayerProperties cleared <key,value> pair: [{key}]={value}");
                break;
            //Like SyncList, indicates all operations are complete.
            case SyncDictionaryOperation.Complete:
                Debug.Log($"[asServer:{asServer}] >>> PlayerProperties sync completed");// <key,value> pair: [{key}]={value}");
                break;
        }
    }

    /// <summary>
    /// Use this fn from client to modify player-properties in order to be reflected throughout the network.
    /// </summary>
    /// <param name="op">Dictionary Operation that needs to be performed.</param>
    /// <param name="key">Key</param>
    /// <param name="value">Optional Value, depending upon the operation passed.</param>
    [ServerRpc/*, Server*/] // server -> only server can run this fn; serverRPC -> always clients to call this fn in server as well
    private void ModifyPlayerProperties(SyncDictionaryOperation op, string key, object value = null)
    {
        if (op == SyncDictionaryOperation.Add || op == SyncDictionaryOperation.Set)
        {
            if (playerProperties.ContainsKey(key))
                playerProperties[key] = value;
            else
                playerProperties.Add(key, value);
            return;
        }

        switch (op)
        {
            //Removes key.
            case SyncDictionaryOperation.Remove:
                playerProperties.Remove(key);
                break;

            //Clears the dictionary.
            case SyncDictionaryOperation.Clear:
                playerProperties.Clear();
                break;

            //Like SyncList, indicates all operations are complete.
            case SyncDictionaryOperation.Complete:
                break;
        }
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        name = name.Replace("Clone", $"ID:{OwnerId:D2}");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        AssignPlayerProperties(setRandom: true);

        Debug.Log(">>> Server Set Random Player Properties");
        //Debug.LogError("ErrorPause!");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        AssignPlayerProperties();

        Debug.Log(">>> Client Reset Player Properties");

        //AssignPlayerPropertiesServerRpc();
        //Debug.Log(">>> Client Reset Player Properties through ServerRPC");
    }
    private void UpdatePlayerCustomProperties(string characterID, string characterURL, int userId)
    {
        // in-order to replicate Photon's hash-table simply use, Set-operation always, for this case...
        ModifyPlayerProperties(SyncDictionaryOperation.Set, "character_id", characterID);
        ModifyPlayerProperties(SyncDictionaryOperation.Set, "avatarUrl", characterURL);
        ModifyPlayerProperties(SyncDictionaryOperation.Set, "user_id", userId);
    }

    //[ServerRpc] private void AssignPlayerPropertiesServerRpc() => AssignPlayerProperties();

    private void AssignPlayerProperties(bool setRandom = false)
    {
        //if (!IsOwner) return; // this will only update from client, it doesnt throw error, but will not update in other clients side...
        if (!setRandom)
            UserID = Owner.ClientId;
        else
            UserID = Random.Range(0, 10000);

        CharacterID = "Player" + UserID.ToString("D4");
        CharacterURL = "alterAPI.com/" + CharacterID;

        UpdatePlayerCustomProperties(CharacterID, CharacterURL, UserID);
    }
    private void OnUserIdChanged(int oldVal, int newVal, bool asSever)
    {
        Debug.Log($">>> [Server:{asSever}]: Player Alter-ID changed from {oldVal} to {newVal}.");
    }
}
