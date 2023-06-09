using FishNet.Object;
using FishNet.Object.Synchronizing;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerViewUI : NetworkBehaviour
{
    [SerializeField] private RawImage iconImg;
    [SerializeField] private TextMeshProUGUI nameTxt;
    private Texture2D avatarTex;

    [field: SyncVar] public CSteamID PlayerID { get; [ServerRpc] set; } = (CSteamID)0;
    

    protected Callback<AvatarImageLoaded_t> AvatarLoaded;
    private void Start()
    {
        AvatarLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImgLoaded);
    }

    private void OnAvatarImgLoaded(AvatarImageLoaded_t imgLoaded)
    {
        if (imgLoaded.m_steamID != PlayerID)
            return; // other player

        Debug.Log($"PlayerID[{PlayerID}]'s AvatarImgData Loaded!");
        FetchPlayerIcon(imgLoaded.m_iImage); // update player icon
    }

    private void FetchPlayerIcon(int imageID = -1)
    {
        Debug.Log("Fetching Player Icon.");
        if (imageID == -1)
        {
            imageID = SteamFriends.GetLargeFriendAvatar(PlayerID);
            if (imageID == -1)
            {
                Debug.LogWarning("Player Avatar Fetch Failed! Reason: Invalid image-id.");
                return;
            }
        }

        if (TryConvertSteamImageAsTexture(imageID, out string msg))
            iconImg.texture = avatarTex;
        else
            Debug.LogWarning("Player Avatar Fetch Failed! Reason: convertion of image-id to texture failed." +
                "\nLog: " + msg);
    }

    private bool TryConvertSteamImageAsTexture(int iImage, out string msg)
    {
        //avatarTex = null;
        msg = "";
        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            msg += "img-Size Fetch Valid + ";
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                msg += "img-RGBA Fetch Valid";
                avatarTex = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                avatarTex.LoadRawTextureData(image);
                avatarTex.Apply();
                return true;
            }
            else
                msg += "img-RGBA Fetch Invalid";
        }
        msg += "img-Size Fetch Invalid";
        return false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        MainMenuManager.OnPlayerJoined(this);
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        MainMenuManager.OnPlayerLeft(this);
    }

    public void SetPlayerInfo(CSteamID playerID)
    {
        this.PlayerID = playerID;
        nameTxt.text = SteamFriends.GetPersonaName();

        Debug.Log($"Setting Player View UI...");
        FetchPlayerIcon();
    }
}
