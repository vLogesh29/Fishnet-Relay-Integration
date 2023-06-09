using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySelectionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameTxt;
    [SerializeField] private Button joinBtn;
    private CSteamID lobbyID;

    private void Start()
    {
        joinBtn.onClick.AddListener(() =>
        {
            JoinLobby();
        });
    }

    public void SetLobbyInfo(CSteamID lobbyID)
    {
        this.lobbyID = lobbyID;
        lobbyNameTxt.text = SteamMatchmaking.GetLobbyData(lobbyID, BootstrapManager.LOBBY_KEY);
    }
    public void JoinLobby()
    {
        BootstrapManager.JoinByID(lobbyID);
    }
}
