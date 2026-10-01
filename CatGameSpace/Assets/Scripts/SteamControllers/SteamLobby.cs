using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if STEAMWORKS_NET
using Steamworks;
#endif
using TMPro;
using Mirror;
using System;
using UnityEngine.UIElements;
using System.Linq;
using Unity.VisualScripting;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;

#if STEAMWORKS_NET
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEnter;
    protected Callback<LobbyChatMsg_t> LobbyChatMsg;
    protected Callback<LobbyMatchList_t> LobbyMatchList;
#endif

    public ulong CurrentLobbyID;
    public string currentCode;
    private const string HostAdressKey = "HostAddress";
    public CustomNetworkManager manager;


    public ChatManager chatManager;

    public List<PlayerController> GamePlayers { get; } = new List<PlayerController>();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }
    private void OnSceneLoad(Scene activeScene, LoadSceneMode arg1)
    {
        if(activeScene.name == "OnlineGamePlay")
        {
            chatManager = GameObject.FindAnyObjectByType<ChatManager>();
        }
        else if(activeScene.name == "Menu")
        {
            Destroy(this.gameObject);
        }
    }

#if STEAMWORKS_NET
    private void Start()
    {
        if (!SteamManager.Initialized) return;

        if (instance == null) instance = this;
        else Destroy(this);


        manager = GetComponent<CustomNetworkManager>();

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnChatMessage);
        LobbyMatchList = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);

        DontDestroyOnLoad(this.gameObject);

        if (GameDataTracker.isHost)
        {
            HostLobby();
        }
        else
        {
            JoinLobby();
        }

    }

    private void OnDestroy()
    {
        LobbyCreated.Dispose();
        JoinRequest.Dispose();
        LobbyEnter.Dispose();
        LobbyChatMsg.Dispose();
        LobbyMatchList.Dispose();

        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, manager.maxConnections);
    }

    public void JoinLobby()
    {
        CSteamID lobbyID = new CSteamID(GameDataTracker.LobbyId); // veya direk CSteamID varsa
        SteamMatchmaking.JoinLobby(lobbyID);
    }


    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) return;



        manager.StartHost();


        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey, SteamUser.GetSteamID().ToString());

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "selectedID", GameDataTracker.selectedPaintID);

        CurrentLobbyID = callback.m_ulSteamIDLobby;

        TryGenerateUniqueCode();
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {

        LobbyEnterFeedBack((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse);
        if ((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess) return;
        //Everyone


        LobbyController.instance.toLobby();

        CurrentLobbyID = callback.m_ulSteamIDLobby;

        //Clients

        if (NetworkServer.active) return;


        CurrentLobbyID = callback.m_ulSteamIDLobby;

        currentCode = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "joinCode");

        LobbyController.instance.SetDisplayLobbyNumber();


        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey);


        manager.StartClient();

    }



    private void LobbyEnterFeedBack(EChatRoomEnterResponse response)
    {
        if (response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            Debug.Log("Lobiye başarıyla katıldın!");
        }
        else
        {
            string mainMsg = $"Lobiye katılamadı: {response}";

            switch (response)
            {
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseDoesntExist:
                    ShowError("Bu lobi mevcut değil.");
                    break;

                case EChatRoomEnterResponse.k_EChatRoomEnterResponseNotAllowed:
                    ShowError("Bu lobiye katılma iznin yok.");
                    break;

                case EChatRoomEnterResponse.k_EChatRoomEnterResponseBanned:
                    ShowError("Bu lobiye erişimin engellenmiş.");
                    break;

                default:
                    ShowError("Lobiye katılamadı. Kod: " + response);
                    break;
            }
        }
    }

    void ShowError(string message)
    {
        GameDataTracker.ErrorMessage = message;
        SceneManager.LoadScene("Menu");
        Debug.LogWarning(message);
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }





    private void OnChatMessage(LobbyChatMsg_t callback)
    {
        byte[] data = new byte[4096];
        CSteamID userID;
        int dataSize = SteamMatchmaking.GetLobbyChatEntry(new CSteamID(callback.m_ulSteamIDLobby), (int)callback.m_iChatID, out userID, data, data.Length, out EChatEntryType EType);
        string message = System.Text.Encoding.UTF8.GetString(data, 0, dataSize);



        string sender = null;
        foreach(PlayerController player in manager.GamePlayers)
        {
            if (player.PlayerSteamID == callback.m_ulSteamIDUser)
            {
                sender = player.PlayerName;
            }
        }
        if(string.IsNullOrEmpty(sender))
        {
            Debug.Log("Sender Couldn't find");
            return;
        }

        if(SceneManager.GetActiveScene().name == "OnlineGamePlay")
        {
            if(sender != GameController.Instance.LocalPlayerOnlinePlayer.PlayerName && GameController.Instance.gameState != StateManager.GameState.InChat)
            {
                GameController.Instance.ChatReceivedWhileClosed();
            }
        }

        chatManager.AddChat(sender, message, callback.m_ulSteamIDUser == GameObject.Find("LocalGamePlayer").GetComponent<PlayerController>().PlayerSteamID);
    }
#endif

    public void SendChatMessage(string msg)
    {
#if STEAMWORKS_NET
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(msg);


        bool success = SteamMatchmaking.SendLobbyChatMsg(
            new CSteamID(CurrentLobbyID),
            bytes,
            bytes.Length
        );

        Debug.Log("SendLobbyChatMsg success: " + success);
#endif
    }

    public string GetLobbyData(string key)
    {
#if STEAMWORKS_NET
        return SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), key);
#else
        return "";
#endif
    }

    public void SetLobbyData(string key, string value)
    {
#if STEAMWORKS_NET
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), key, value);
#endif
    }

    public void LeaveCurrentLobby()
    {
#if STEAMWORKS_NET
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
#endif
    }

    public ulong GetLobbyMemberSteamID(int index)
    {
#if STEAMWORKS_NET
        return (ulong)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(CurrentLobbyID), index);
#else
        return 0;
#endif
    }

#if STEAMWORKS_NET
    private void TryGenerateUniqueCode()
    {
        currentCode = GenerateRandomCode(6);
        SteamMatchmaking.AddRequestLobbyListStringFilter(
            "joinCode", currentCode, ELobbyComparison.k_ELobbyComparisonEqual
        );
        SteamMatchmaking.RequestLobbyList();
    }

    private void OnLobbyMatchList(LobbyMatchList_t cb)
    {
        if (cb.m_nLobbiesMatching > 0)
        {
            // Aynı kodlu lobby var, tekrar dene
            TryGenerateUniqueCode();
        }
        else
        {
            // Eşsiz kod bulundu
            SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "joinCode", currentCode);
            LobbyController.instance.SetDisplayLobbyNumber();
            Debug.Log("Lobby kodun: " + currentCode);
        }
    }
#endif

    private string GenerateRandomCode(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var rng = new System.Random();
        var sb = new System.Text.StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[rng.Next(chars.Length)]);
        }
        return sb.ToString();
    }
}
