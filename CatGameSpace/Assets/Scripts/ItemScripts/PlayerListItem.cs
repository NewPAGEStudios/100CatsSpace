using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if STEAMWORKS_NET
using Steamworks;
#endif

public class PlayerListItem : MonoBehaviour
{
    public string PlayerName;
    public int ConnectionID;
    public ulong PlayersSteamID;
    private bool AvatarReceived;
    public Color SelectedColor;

    public TextMeshProUGUI PlayerNameText;
    public Image PlayerColorImage;
    public RawImage PlayerIcon;
    public GameObject playerHostGO;
    public GameObject playerReadyGO;
    public bool Ready;

#if STEAMWORKS_NET
        protected Callback<AvatarImageLoaded_t> ImageLoaded;
#endif

    public void ChangeReadyStatus()
    {
        if (Ready)
        {
            playerReadyGO.SetActive(true);
        }
        else
        {
            playerReadyGO.SetActive(false);
        }



    }


#if STEAMWORKS_NET
    private void Start()
    {
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }
#endif

    void GetPlayerIcon()
    {
#if STEAMWORKS_NET
        int ImageID = SteamFriends.GetLargeFriendAvatar((CSteamID)PlayersSteamID);
        if (ImageID == -1) return;
        PlayerIcon.texture = GetSteamImageAsTexture(ImageID);
#endif
    }

    public void SetPlayerValues()
    {
        PlayerNameText.text = PlayerName;

        PlayerColorImage.color = SelectedColor;


        if(ConnectionID == 0) playerHostGO.SetActive(true);
        else playerHostGO.SetActive(false);

        if (!AvatarReceived) { GetPlayerIcon(); }

        ChangeReadyStatus();

    }

#if STEAMWORKS_NET
    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == PlayersSteamID)
        {
            PlayerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else
        {
            return;
        }
    }
    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        AvatarReceived = true;
        return texture;
    }
#endif

}
