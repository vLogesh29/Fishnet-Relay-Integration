using FishNet.Managing;
using FishNet.Transporting;
using PlayFab.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class NetworkHud : MonoBehaviour
{
    public const string DEFAULT_SERVER_IP = "localhost";
    public const int DEFAULT_PORT_NO = 7777;

    #region Types.
    /// <summary>
    /// Ways the HUD will automatically start a connection.
    /// </summary>
    private enum AutoStartType
    {
        Disabled,
        Host,
        Server,
        Client
    }
    #endregion

    #region Serialized.
    /// <summary>
    /// What connections to automatically start on play.
    /// </summary>
    [Tooltip("What connections to automatically start on play.")]
    [SerializeField]
    private AutoStartType _autoStartType = AutoStartType.Disabled;
    /// <summary>
    /// Color when socket is stopped.
    /// </summary>
    [Tooltip("Color when socket is stopped.")]
    [SerializeField]
    private Color _stoppedColor = Color.red;
    /// <summary>
    /// Color when socket is changing.
    /// </summary>
    [Tooltip("Color when socket is changing.")]
    [SerializeField]
    private Color _changingColor = Color.yellow;
    /// <summary>
    /// Color when socket is started.
    /// </summary>
    [Tooltip("Color when socket is started.")]
    [SerializeField]
    private Color _startedColor = Color.green;
    [Header("Indicators")]
    /// <summary>
    /// Indicator Parent for server state.
    /// </summary>
    [Tooltip("Indicator parent for server state.")]
    [SerializeField]
    private Transform _serverIndicatorParent;
    /// <summary>
    /// Indicator Parent for client state.
    /// </summary>
    [Tooltip("Indicator parent for client state.")]
    [SerializeField]
    private Transform _clientIndicatorParent;
    /// <summary>
    /// Indicator for server state.
    /// </summary>
    [Tooltip("Indicator for server state.")]
    [SerializeField]
    private Image _serverIndicatorImg;
    /// <summary>
    /// Indicator Image for client state.
    /// </summary>
    [Tooltip("Indicator image for client state.")]
    [SerializeField]
    private Image _clientIndicatorImg;
    /// <summary>
    /// Indicator Image for server state.
    /// </summary>
    [Tooltip("Indicator image for server state.")]
    [SerializeField]
    private TMP_Text _serverIndicatorTxt;
    /// <summary>
    /// Indicator for client state.
    /// </summary>
    [Tooltip("Indicator for client state.")]
    [SerializeField]
    private TMP_Text _clientIndicatorTxt;
    /// <summary>
    /// Parent of all Input Fields for connection configuration.
    /// </summary>
    [Tooltip("Parent of all input fields for connection configuration.")]
    [SerializeField]
    private Transform _inputFieldParent;
    /// <summary>
    /// Text Field to enter server IP address.
    /// </summary>
    [Tooltip("Field to enter server IP address.")]
    [SerializeField] private TMP_InputField _serverIPField;
    /// <summary>
    /// Text Field to enter Port number.
    /// </summary>
    [Tooltip("Field to enter port no.")]
    [SerializeField] private TMP_InputField _portNoField;
    #endregion

    #region Private.
    /// <summary>
    /// Found NetworkManager.
    /// </summary>
    private NetworkManager _networkManager;
    /// <summary>
    /// Current state of client socket.
    /// </summary>
    private LocalConnectionState _clientState = LocalConnectionState.Stopped;
    /// <summary>
    /// Current state of server socket.
    /// </summary>
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;

    [Header("Debug:Configuration")]
    
    [SerializeField] private string _serverAddress = DEFAULT_SERVER_IP;
    [field:SerializeField] public IPAddressType AddressType { get; private set; } = IPAddressType.IPv4;

    /// <summary>
    /// Server Address [to which client connects to (or) to which server binds to]
    /// </summary>
    public string ServerAddress
    {
        get => _serverAddress;
        set
        {
            _serverAddress = value;

            if (_serverAddress.Split('.').Length == 4)
                AddressType = IPAddressType.IPv4;
            else if (_serverAddress.Contains(':'))
                AddressType = IPAddressType.IPv6;
            else
                _serverAddress = DEFAULT_SERVER_IP;

            _serverIPField.text = _serverAddress.ToString();
        }
    }

    [SerializeField] private int _port = DEFAULT_PORT_NO;
    /// <summary>
    /// Port Number used by this transport-layer connection by server and client
    /// </summary>
    public int Port
    {
        get => _port;
        set
        {
            _port = value;
            _portNoField.text = Port.ToString();
        }
    }
#if !ENABLE_INPUT_SYSTEM
    /// <summary>
    /// EventSystem for the project.
    /// </summary>
    private EventSystem _eventSystem;
#endif
    #endregion

    string GetNextStateText(LocalConnectionState state)
    {
        if (state == LocalConnectionState.Stopped)
            return "Start";
        else if (state == LocalConnectionState.Starting)
            return "Starting";
        else if (state == LocalConnectionState.Stopping)
            return "Stopping";
        else if (state == LocalConnectionState.Started)
            return "Stop";
        else
            return "Invalid";
    }

    void OnGUI()
    {
#if ENABLE_INPUT_SYSTEM

        GUILayout.BeginArea(new Rect(16, 16, 256, 9000));
        Vector2 defaultResolution = new Vector2(1920f, 1080f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / defaultResolution.x, Screen.height / defaultResolution.y, 1));

        GUIStyle style = GUI.skin.GetStyle("button");
        int originalFontSize = style.fontSize;

        Vector2 buttonSize = new Vector2(256f, 64f);
        style.fontSize = 28;
        //Server button.
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            if (GUILayout.Button($"{GetNextStateText(_serverState)} Server", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)))
                OnClick_Server();
            GUILayout.Space(10f);
        }

        //Client button.
        if (GUILayout.Button($"{GetNextStateText(_clientState)} Client", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)))
            OnClick_Client();

        style.fontSize = originalFontSize;

        GUILayout.EndArea();
#endif
    }

    private void Awake()
    {
        _serverIPField.text = ServerAddress;
        _portNoField.text = Port.ToString();

        _serverIPField.onEndEdit.AddListener((string val) =>
        {
            ServerAddress = val;
        });
        _portNoField.onEndEdit.AddListener((string val) =>
        {
            if (!int.TryParse(val, out _port))
                Port = DEFAULT_PORT_NO;
        });
    }

    private void Start()
    {
#if !ENABLE_INPUT_SYSTEM
        SetEventSystem();
        BaseInputModule inputModule = FindObjectOfType<BaseInputModule>();
        if (inputModule == null)
            gameObject.AddComponent<StandaloneInputModule>();
#else
        _serverIndicatorParent.gameObject.SetActive(false);
        _clientIndicatorParent.gameObject.SetActive(false);
#endif

        _networkManager = FindObjectOfType<NetworkManager>();
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager not found, HUD will not function.");
            return;
        }
        else
        {
            UpdateIndicatorState(LocalConnectionState.Stopped, ref _serverIndicatorTxt, ref _serverIndicatorImg);
            UpdateIndicatorState(LocalConnectionState.Stopped, ref _clientIndicatorTxt, ref _clientIndicatorImg);
            _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        }

        if (_autoStartType == AutoStartType.Host || _autoStartType == AutoStartType.Server)
            OnClick_Server();
        if (!Application.isBatchMode && (_autoStartType == AutoStartType.Host || _autoStartType == AutoStartType.Client))
            OnClick_Client();
    }


    private void OnDestroy()
    {
        if (_networkManager == null)
            return;

        _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
    }

    /// <summary>
    /// Updates img color baased on state.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="img"></param>
    private void UpdateIndicatorState(LocalConnectionState state, ref TMP_Text txt, ref Image img)
    {
        Color c;

        if (state == LocalConnectionState.Starting || state == LocalConnectionState.Started)
            _inputFieldParent.gameObject.SetActive(false);
        else if (state == LocalConnectionState.Stopping || state == LocalConnectionState.Stopped)
            _inputFieldParent.gameObject.SetActive(true);

        if (state == LocalConnectionState.Started)
            c = _startedColor;
        else if (state == LocalConnectionState.Stopped)
            c = _stoppedColor;
        else
            c = _changingColor;

        img.color = c;
        txt.text = GetNextStateText(state);
    }


    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        _clientState = obj.ConnectionState;
        UpdateIndicatorState(obj.ConnectionState, ref _clientIndicatorTxt, ref _clientIndicatorImg);
    }


    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        _serverState = obj.ConnectionState;
        UpdateIndicatorState(obj.ConnectionState, ref _serverIndicatorTxt, ref _serverIndicatorImg);
    }


    public void OnClick_Server()
    {
        if (_networkManager == null)
            return;

        if (_serverState != LocalConnectionState.Stopped)
            _networkManager.ServerManager.StopConnection(true);
        else
        {
            if (ServerAddress != DEFAULT_SERVER_IP)
            {
                _networkManager.TransportManager.
                    Transport.SetServerBindAddress(ServerAddress, AddressType);
            }
            _networkManager.ServerManager.StartConnection((ushort)Port);
            
            var transport = _networkManager.TransportManager.Transport;
            Debug.Log("\n\n");
            Debug.Log($"Starting Server connection with address:{transport.GetServerBindAddress(AddressType)} and port:{transport.GetPort()}");
        }

        DeselectButtons();
    }


    public void OnClick_Client()
    {
        if (_networkManager == null)
            return;

        if (_clientState != LocalConnectionState.Stopped)
            _networkManager.ClientManager.StopConnection();
        else
            _networkManager.ClientManager.StartConnection(ServerAddress, (ushort)Port);

        var transport = _networkManager.TransportManager.Transport;
        Debug.Log("\n\n");
        Debug.Log($"Starting Client connection with address:{transport.GetClientAddress()} and port:{transport.GetPort()}");

        DeselectButtons();
    }


    private void SetEventSystem()
    {
#if !ENABLE_INPUT_SYSTEM
        if (_eventSystem != null)
            return;
        _eventSystem = FindObjectOfType<EventSystem>();
        if (_eventSystem == null)
            _eventSystem = gameObject.AddComponent<EventSystem>();
#endif
    }

    private void DeselectButtons()
    {
#if !ENABLE_INPUT_SYSTEM
        SetEventSystem();
        _eventSystem?.SetSelectedGameObject(null);
#endif
    }
}