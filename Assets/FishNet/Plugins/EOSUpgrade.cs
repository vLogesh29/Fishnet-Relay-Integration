#if EOS
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Platform;
using FishNet.Transporting.FishyEOSPlugin;
using FishNet.Plugins.FishyEOS.Util;


using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EOSUpgrade : MonoBehaviour
{
    private static PlatformInterface PlatformInterface => EOS.GetPlatformInterface();
    private static LobbyInterface _cachedLobbyInterface;

    // Added for so user can be kicked from EOS lobby as well as server

    /// <summary>
    /// Gets the EOS Connect Peer Id of a connection Id. 
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public static string FishyEOS_GetRemoteConnectionAddress(int connectionId)
    {
        return /*_server.*/ServerPeer_GetRemoteConnectionAddress(connectionId);
    }

    /// <summary>
    /// Gets the EOS Local Product User Id of the remote connection.
    /// </summary>
    internal static string ServerPeer_GetRemoteConnectionAddress(int connectionId)
    {
        //if (connectionId == /*FishyEOS.*/CLIENT_HOST_ID)
            //return _localUserId.ToString();

        var fishy = InstanceFinder.NetworkManager.GetComponent<FishyEOS>();
        return fishy.GetConnectionAddress(connectionId);
    }

    public static LobbyInterface GetCachedLobbyInterface()
    {
        if (!IsSafeToUseCache()) return null;
        if (_cachedLobbyInterface != null) return _cachedLobbyInterface;
        _cachedLobbyInterface = PlatformInterface?.GetLobbyInterface();
        return _cachedLobbyInterface;
    }
    private static bool IsSafeToUseCache()
    {
        // Cache references maybe unsafe if PlatformInterface is null
        return PlatformInterface != null;
    }
}
#endif