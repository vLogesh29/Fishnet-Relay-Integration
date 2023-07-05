using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using FishNet;

public class CustomPlayerSpawner : MonoBehaviour
{
    #region Public.
    /// <summary>
    /// Called on the server when a player is spawned.
    /// </summary>
    public event Action<NetworkObject> OnSpawned;
    #endregion

    #region Serialized.
    /// <summary>
    /// Prefab to spawn for the player.
    /// </summary>
    [Tooltip("Prefab to spawn for the player.")]
    [SerializeField]
    private NetworkObject _playerPrefab;
    /// <summary>
    /// True to add player to the active scene when no global scenes are specified through the SceneManager.
    /// </summary>
    [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
    [SerializeField]
    private bool _addToDefaultScene = true;
    /// <summary>
    /// Areas in which players may spawn.
    /// </summary>
    [Tooltip("Areas in which players may spawn.")]
    [FormerlySerializedAs("_spawns")]
    public Transform[] Spawns = new Transform[0];
    #endregion

    #region Private.
    /// <summary>
    /// NetworkManager on this object or within this objects parents.
    /// </summary>
    private NetworkManager _networkManager;
    /// <summary>
    /// Next spawns to use.
    /// </summary>
    private int _nextSpawn;
    #endregion

    private void Start()
    {
        InitializeOnce();
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            BootstrapNetworkManager.Instance.OnClientLoadedScenes -= SceneManager_OnClientLoadedScenes;
    }


    /// <summary>
    /// Initializes this script for use.
    /// </summary>
    private void InitializeOnce()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }
        BootstrapNetworkManager.Instance.OnClientLoadedScenes += SceneManager_OnClientLoadedScenes;
    }

    /// <summary>
    /// Called when a client loads initial scenes after connecting.
    /// </summary>
    private void SceneManager_OnClientLoadedScenes(NetworkConnection conn, bool asServer)
    {
        Debug.Log($"--- Custom Player Spawner Event Raised! Client-ID[{conn.ClientId}]::asServer[{asServer}] ---");
        if (!asServer)
        {
            Debug.LogWarning($"Cant Spawn Player as this method is not called from acting server !");
            return;
        }
        if (_playerPrefab == null)
        {
            Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }
        Debug.Log("==> spawn conditions pass!");

#if PREDICTION_V2
            ////Test code.
            ////Spawn for everyone but server.
            //if (_networkManager.ServerManager.Clients.Count == 1)
            //    return;

            ////Only spawn for server.
            //if (_networkManager.ServerManager.Clients.Count != 1)
            //    return;
#endif

        Vector3 position;
        Quaternion rotation;
        SetSpawn(_playerPrefab.transform, out position, out rotation);

        NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, _playerPrefab.SpawnableCollectionId, true);
        nob.transform.SetPositionAndRotation(position, rotation);
        _networkManager.ServerManager.Spawn(nob, conn);

        Debug.Log($"==> spawning player at pos:{position}, rot: {rotation}, NetObj:{nob}");

        //If there are no global scenes 
        if (_addToDefaultScene)
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

        OnSpawned?.Invoke(nob);
    }


    /// <summary>
    /// Sets a spawn position and rotation.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    private void SetSpawn(Transform prefab, out Vector3 pos, out Quaternion rot)
    {
        //No spawns specified.
        if (Spawns.Length == 0)
        {
            SetSpawnUsingPrefab(prefab, out pos, out rot);
            return;
        }

        Transform result = Spawns[_nextSpawn];
        if (result == null)
        {
            SetSpawnUsingPrefab(prefab, out pos, out rot);
        }
        else
        {
            pos = result.position;
            rot = result.rotation;
        }

        //Increase next spawn and reset if needed.
        _nextSpawn++;
        if (_nextSpawn >= Spawns.Length)
            _nextSpawn = 0;
    }

    /// <summary>
    /// Sets spawn using values from prefab.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    private void SetSpawnUsingPrefab(Transform prefab, out Vector3 pos, out Quaternion rot)
    {
        pos = prefab.position;
        rot = prefab.rotation;
    }

}