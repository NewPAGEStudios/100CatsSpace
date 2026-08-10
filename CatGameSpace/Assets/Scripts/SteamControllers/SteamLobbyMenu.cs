using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class SteamLobbyMenu : MonoBehaviour
{
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyMatchList_t> LobbyList;

    public MenuController menuController;
    private void OnEnable()
    {
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyList = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
    }


    private void OnDisable()
    {
        JoinRequest.Dispose();
        LobbyList.Dispose();
    }


    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        GameDataTracker.skipSave = true;

        GameDataTracker.LobbyId = callback.m_steamIDLobby.m_SteamID;
        GameDataTracker.isHost = false;

        SceneManager.LoadScene("LobbyScene");
    }


    public void searchForLobby(string lobbyID)
    {
        if (string.IsNullOrEmpty(lobbyID.Trim()))
        {
            Debug.Log("Entered Lobby id is invalid");
            return;
        }
        MenuController.instance.WaitPanelOpen();

        SteamMatchmaking.AddRequestLobbyListStringFilter(
            "joinCode",
            lobbyID,
            ELobbyComparison.k_ELobbyComparisonEqual
        );

        SteamMatchmaking.RequestLobbyList();
    }

    private void OnLobbyMatchList(LobbyMatchList_t callback)
    {
        if (callback.m_nLobbiesMatching > 0)
        {
            CSteamID foundLobby = SteamMatchmaking.GetLobbyByIndex(0);
            menuController.JoinGameAccepted((ulong)foundLobby);
            Debug.Log($"Kod bulundu, lobiye katýlýnýyor…");
        }
        else
        {
            Debug.LogWarning("Bu kodla eþleþen lobi yok.");
            MenuController.instance.WaitPanelClose();
        }

    }


}
