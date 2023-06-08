using FishNet.Managing;
using Steamworks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    private const string HOST_KEY = "HostAddress";
    private const string LOBBY_KEY = "name";
    private static BootstrapManager instance;
#if UNITY_EDITOR
    [SerializeField] private SceneAsset menuScene;
#endif
    [SerializeField] private string menuSceneName;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks _fishySteamworks;

    private void Awake()
    {
        instance = this;
#if UNITY_EDITOR
        menuSceneName = menuScene.name;
#endif
    }

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    private string hostName = HOST_KEY;
    private string lobbyName = LOBBY_KEY;

    public static ulong CurrentLobbyID;

    private void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void GoToMenu()
    {
        Debug.Log($"Steamworks intialized, connected to Local-User. Loading MainMenu Scene:\"{menuSceneName}\"...");
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Additive);
    }

    public static void CreateLobby(ELobbyType lobbyType, int maxPlayers)
    {
        SteamMatchmaking.CreateLobby(lobbyType, maxPlayers);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("SetingUp Lobby-Data on lobby-creation with callback-result: " + callback.m_eResult.ToString());

        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.Log("Lobby creation was unsuccessful!");
            return;
        }

        // set as host
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        hostName = SteamUser.GetSteamID().ToString();
        lobbyName = SteamFriends.GetPersonaName().ToString() + "'s lobby";

        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), HOST_KEY, hostName);
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), LOBBY_KEY, lobbyName);
        
        _fishySteamworks.SetClientAddress(hostName);
        _fishySteamworks.StartConnection(server:true); 
        // starting connection by giving the host-client (Client&Server) connection, server-attrb = true
        Debug.Log("Lobby creation was successful!");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // get as client
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        lobbyName = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), LOBBY_KEY);
        hostName = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), HOST_KEY);

        MainMenuManager.LobbyEntered(lobbyName, _networkManager.IsServer);

        _fishySteamworks.SetClientAddress(hostName);
        // starting connection by giving the client (ClientOnly) connection, server-attrb = false
        _fishySteamworks.StartConnection(false);
    }

    public static void JoinByID(CSteamID steamID)
    {
        Debug.Log("Attempting to join lobby with ID: " + steamID.m_SteamID);
        if (SteamMatchmaking.RequestLobbyData(steamID))
            SteamMatchmaking.JoinLobby(steamID);
        else
            Debug.Log("Failed to join lobby with ID: " + steamID.m_SteamID);
    }

    public static void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        CurrentLobbyID = 0;

        instance._fishySteamworks.StopConnection(false);
        if (instance._networkManager.IsServer)
            instance._fishySteamworks.StopConnection(true);
    }
}