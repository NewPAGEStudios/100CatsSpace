#if STEAMWORKS_NET
using Steamworks;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardListItem : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TextMeshProUGUI PlayerNameText;
    public TextMeshProUGUI NumberText;
    public TextMeshProUGUI ScoreText;
    public RawImage PlayerIcon;

#if STEAMWORKS_NET
    public CSteamID steamID;

    public void SetPlayerInfos(int number, string name, int ImageID, CSteamID csteamID, int score)
    {
        steamID = csteamID;

        // --- SKOR ---
        if (number < 0)
        {
            NumberText.text = "--";
            ScoreText.text = "--";
        }
        else
        {
            NumberText.text = number.ToString();
            int minutes = score / 60;
            int seconds = score % 60;
            ScoreText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // --- İSİM (Direkt yazıyoruz) ---
        // Veri zaten detaylardan geldiği için "Loading" beklemeye gerek yok.
        PlayerNameText.text = name;

        // --- AVATAR ---
        if (ImageID != -1)
        {
            SetSteamImageAsTexture(ImageID);
        }
        else
        {
            PlayerIcon.texture = null;
        }
    }

    public void SetSteamImageAsTexture(int iImage)
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
        PlayerIcon.texture = texture;
    }
#endif
}
