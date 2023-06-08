using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager Instance;
    
    [Header("Out Lobby")]
    [SerializeField] private GameObject menuScreen;
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button joinLobbyBtn;
    [SerializeField] private TMP_InputField lobbyInput;

    [Header("In Lobby")]
    [SerializeField] private GameObject lobbyScreen;
    [SerializeField] private TextMeshProUGUI lobbyTitle, lobbyIDText;
    [SerializeField] private Button startGameBtn;
    [SerializeField] private Button leaveLobbyBtn;

    [Header("Game")]
    [SerializeField] private Transform PlanetTransform;
    private PlanetSelectionUI[] planetSelections;
    private string gameScene;

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

        SelectGamePlanet(planetSelections[0]);
    }

    public void CreateLobby()
    {
        //BootstrapManager.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
        BootstrapManager.CreateLobby(ELobbyType.k_ELobbyTypePublic, 10);
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

    void CloseAllScreens()
    {
        menuScreen.SetActive(false);
        lobbyScreen.SetActive(false);
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

    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        Instance.lobbyTitle.text = lobbyName;
        Instance.startGameBtn.gameObject.SetActive(isHost);
        Instance.lobbyIDText.text = BootstrapManager.CurrentLobbyID.ToString();
        Instance.OpenLobby();
    }

    private void DeselectAllPlanet()
    {
        foreach (PlanetSelectionUI ps in planetSelections)
            ps.DeselectBtn();
    }

    public static void SelectGamePlanet(PlanetSelectionUI planetSelection)
    {
        Instance.DeselectAllPlanet();
        planetSelection.SelectBtn();
        Instance.gameScene = planetSelection.PlanetSceneName;
    }
}
