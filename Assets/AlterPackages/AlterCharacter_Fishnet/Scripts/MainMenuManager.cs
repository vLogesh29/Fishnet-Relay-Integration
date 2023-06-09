using System;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; } = null;

    [Header("Out Lobby")]
    [SerializeField] private GameObject menuScreen;
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button joinLobbyBtn;
    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private TextMeshProUGUI maxPlayersTxt;
    [SerializeField] private Button incrementMaxPlayers;
    [SerializeField] private Button decrementMaxPlayers;
    [SerializeField] private ELobbyDistanceFilter lobbyFilter = ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide;
    [SerializeField] private int maxDisplayLobbies = 100;
    [SerializeField] private Transform lobbyListContainer;
    [SerializeField] private Button refreshLobbiesBtn;
    [SerializeField] private LobbySelectionUI lobbySelectionUITemplate;
    private List<LobbySelectionUI> listOfLobbies = new();

    [Header("In Lobby")]
    [SerializeField] private GameObject lobbyScreen;
    [SerializeField] private TextMeshProUGUI lobbyTitle, lobbyIDText;
    [SerializeField] private Button startGameBtn;
    [SerializeField] private Button leaveLobbyBtn;
    [SerializeField] private Transform playerListContainer;
    //[SerializeField] private PlayerViewUI playerViewUITemplate;
    private List<PlayerViewUI> listOfPlayers = new();

    [Header("Game")]
    [SerializeField] private Transform PlanetTransform;
    private PlanetSelectionUI[] planetSelections;
    private string gameScene;

    #region Private Methods
    private void Awake()
    {
        Instance = this;
        planetSelections = PlanetTransform.GetComponentsInChildren<PlanetSelectionUI>();
    }

    private void Start()
    {
        OpenMainMenu();

        createLobbyBtn.onClick.AddListener(() => CreateLobby());
        joinLobbyBtn.onClick.AddListener(() => JoinLobby());

        startGameBtn.onClick.AddListener(() => StartGame());
        leaveLobbyBtn.onClick.AddListener(() => LeaveLobby());

        incrementMaxPlayers.onClick.AddListener(() =>
        {
            maxPlayersTxt.text = (int.Parse(maxPlayersTxt.text) + 1).ToString();
        });
        decrementMaxPlayers.onClick.AddListener(() =>
        {
            maxPlayersTxt.text = (int.Parse(maxPlayersTxt.text) - 1).ToString();
        });

        refreshLobbiesBtn.onClick.AddListener(() => RefreshLobbyList());
        RefreshLobbyList();
        lobbySelectionUITemplate.gameObject.SetActive(false);

        SelectGamePlanet(planetSelections[0]);
    }

    private void CloseAllScreens()
    {
        menuScreen.SetActive(false);
        lobbyScreen.SetActive(false);
    }
    #endregion

    #region Public Methods
    public void RefreshLobbyList() =>
        BootstrapManager.GetLobbyList(maxFetchLobbies:maxDisplayLobbies, lobbyDistanceFilter:lobbyFilter);

    public void DisplayLobbies(HashSet<ulong> lobbyIDs, LobbyDataUpdate_t updatedData)
    {
        if (lobbySelectionUITemplate == null)
        {
            //Debug.LogError("Lobby Template reference null!");
            return;
        }

        if (lobbyIDs.Contains(updatedData.m_ulSteamIDLobby))
        {
            LobbySelectionUI lobbyInstance = Instantiate(lobbySelectionUITemplate, lobbyListContainer);
            lobbyInstance.gameObject.SetActive(true);
            lobbyInstance.SetLobbyInfo((CSteamID)updatedData.m_ulSteamIDLobby);
            listOfLobbies.Add(lobbyInstance);
        }
    }
    public void DestroyLobbies()
    {
        if (listOfLobbies.Count == 0)
            return;
        foreach(LobbySelectionUI lobby in listOfLobbies)
            Destroy(lobby.gameObject);
        listOfLobbies.Clear();
    }

    public void CreateLobby()
    {
        //BootstrapManager.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
        BootstrapManager.CreateLobby(ELobbyType.k_ELobbyTypePublic, int.Parse(maxPlayersTxt.text), lobbyInput.text);
    }

    public void OpenMainMenu()
    {
        CloseAllScreens();
        menuScreen.SetActive(true);
    }

    public void OpenLobby()
    {
        CloseAllScreens();
        lobbyScreen.SetActive(true);
    }

    public void JoinLobby()
    {
        CSteamID steamID = new CSteamID(Convert.ToUInt64(lobbyInput.text));
        // as string cant be used as steamID, we can converting it into a unsigned-long int
        BootstrapManager.JoinByID(steamID);
    }

    public void LeaveLobby()
    {
        BootstrapManager.LeaveLobby();
        OpenMainMenu();
    }

    public void StartGame()
    {
        Debug.Log("Staring Game synchroniously throughout all clients in the lobby!");

        //string[] scenesToClose = new string[] { "MenuSceneSteam" };
        //BootstrapNetworkManager.ChangeNetworkScene("SteamGameScene", scenesToClose);

        //string[] scenesToDontDestroyOnLoad = new string[] { "Bootstrap" };
        BootstrapNetworkManager.Instance.ChangeNetworkScene(gameScene);
    }

    public void LobbyEntered(string lobbyName, bool isHost)
    {
        lobbyTitle.text = lobbyName;
        startGameBtn.gameObject.SetActive(isHost);
        lobbyIDText.text = BootstrapManager.currentLobbyID.ToString();
        OpenLobby();
    }
    #endregion

    #region Static Methods
    private static void DeselectAllPlanet()
    {
        foreach (PlanetSelectionUI ps in Instance.planetSelections)
            ps.DeselectBtn();
    }

    public static void SelectGamePlanet(PlanetSelectionUI planetSelection)
    {
        DeselectAllPlanet();
        planetSelection.SelectBtn();
        Instance.gameScene = planetSelection.PlanetSceneName;
    }

    public static void OnPlayerJoined(PlayerViewUI playerView)
    {
        ulong playerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)BootstrapManager.currentLobbyID, Instance.listOfPlayers.Count);
        Instance.listOfPlayers.Add(playerView);
        playerView.SetPlayerInfo((CSteamID)playerSteamID);
        playerView.transform.SetParent(Instance.playerListContainer);
    }
    public static void OnPlayerLeft(PlayerViewUI playerView)
    {
        Instance.listOfPlayers.Remove(playerView);
    }
    #endregion
}
